using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.Comuns.Eventos;
using PlantaCoreAPI.Application.DTOs.Identificacao;
using PlantaCoreAPI.Application.DTOs.Planta;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Application.Services.Plant;
using PlantaCoreAPI.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace PlantaCoreAPI.Application.Services;

public sealed partial class PlantService
{
    public async Task<Resultado<PlantaDTOSaida>> IdentificarPlantaAsync(Guid usuarioId, IdentificacaoDTOEntrada entrada)
    {
        try
        {
            ResultadoIdentificacaoPlantNet? resultadoPlantNet = null;
            if (!string.IsNullOrWhiteSpace(entrada.CaminhoTemp))
                resultadoPlantNet = await _servicioPlantNet.IdentificarPlantaPorArquivoAsync(entrada.CaminhoTemp);
            else if (!string.IsNullOrWhiteSpace(entrada.UrlImagem))
                resultadoPlantNet = await _servicioPlantNet.IdentificarPlantaPorUrlAsync(entrada.UrlImagem);
            if (resultadoPlantNet?.Resultados == null || !resultadoPlantNet.Resultados.Any())
                return Resultado<PlantaDTOSaida>.Erro("Nenhuma planta identificada");
            var melhorResultado = resultadoPlantNet.Resultados.OrderByDescending(r => r.Probabilidade).First();
            var nomeCientifico = melhorResultado.Especie?.NomeCientifico ?? "Desconhecido";
            var plantaExistente = await _repositorioPlanta.ObterPorNomeCientificoEUsuarioAsync(nomeCientifico, usuarioId);
            if (plantaExistente != null)
                return Resultado<PlantaDTOSaida>.Ok(MapearPlantaPara(plantaExistente));
            var nomeParaBuscar = nomeCientifico;
            var tarefaTrefle = _servicioTrefle.BuscarPlantasAsync(nomeParaBuscar, 0);
            var tarefaGemini = _servicioGemini.GerarDescricaoPlantaAsync(new DadosPlantaParaIA { NomeCientifico = nomeCientifico });
            await Task.WhenAll(tarefaTrefle, tarefaGemini);
            var resultadoTrefle = tarefaTrefle.Result;
            var descricaoGemini = tarefaGemini.Result;
            var plantaTrefle = resultadoTrefle?.Dados?.FirstOrDefault();
            if (plantaTrefle == null && nomeCientifico.Contains(" L."))
            {
                nomeParaBuscar = nomeCientifico.Replace(" L.", "").Trim();
                resultadoTrefle = await _servicioTrefle.BuscarPlantasAsync(nomeParaBuscar, 0);
                plantaTrefle = resultadoTrefle?.Dados?.FirstOrDefault();
            }

            var dadosEnriquecidos = ExtrairDadosDoGemini(descricaoGemini, nomeCientifico, plantaTrefle);
            var planta = CriarPlantaDeEnriquecidos(usuarioId, dadosEnriquecidos, entrada.UrlImagem);
            await _repositorioPlanta.AdicionarAsync(planta);
            await _repositorioPlanta.SalvarMudancasAsync();
            var notificacao = Notificacao.Criar(
                usuarioId,
                Domain.Enums.TipoNotificacao.PlantaIdentificada,
                $"Sua planta '{planta.NomeComum ?? planta.NomeCientifico}' foi identificada com sucesso!",
                usuarioOrigemId: null,
                plantaId: planta.Id
            );
            await _repositorioNotificacao.AdicionarAsync(notificacao);
            await _repositorioNotificacao.SalvarMudancasAsync();
            await _eventoDispatcher.PublicarAsync(new PlantaIdentificadaEvento { UsuarioId = usuarioId, PlantaId = planta.Id });
            return Resultado<PlantaDTOSaida>.Ok(MapearPlantaPara(planta));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao identificar planta para usuário {UsuarioId}", usuarioId);
            return Resultado<PlantaDTOSaida>.Erro("Erro ao identificar planta. Tente novamente.");
        }
    }
}
