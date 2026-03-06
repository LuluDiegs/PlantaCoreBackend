using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.DTOs.Identificacao;
using PlantaCoreAPI.Application.DTOs.Planta;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Domain.Entities;

namespace PlantaCoreAPI.Infrastructure.Services;

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

            var plantaExistente = await _repositorioPlanta.ObterPorNomeCientificoAsync(nomeCientifico);
            if (plantaExistente != null)
                return Resultado<PlantaDTOSaida>.Ok(MapearPlantaPara(plantaExistente));

            var nomeParaBuscar = nomeCientifico;
            var tarefaTrefle = _servicioTrefle.BuscarPlantasAsync(nomeParaBuscar, 0);
            var tarefaGemini = _servicioGemini.GerarDescricaoPlantaAsync(new DadosPlantaParaIA { NomeCientifico = nomeCientifico });

            await Task.WhenAll(tarefaTrefle, tarefaGemini);

            var resultadoTrefle = await tarefaTrefle;
            var plantaTrefle = resultadoTrefle?.Dados?.FirstOrDefault();

            if (plantaTrefle == null && nomeCientifico.Contains(" L."))
            {
                nomeParaBuscar = nomeCientifico.Replace(" L.", "").Trim();
                resultadoTrefle = await _servicioTrefle.BuscarPlantasAsync(nomeParaBuscar, 0);
                plantaTrefle = resultadoTrefle?.Dados?.FirstOrDefault();
            }

            var dadosEnriquecidos = ExtrairDadosDoGemini(await tarefaGemini, nomeCientifico, plantaTrefle);

            var planta = CriarPlantaDeEnriquecidos(usuarioId, dadosEnriquecidos, entrada.UrlImagem);

            await _repositorioPlanta.AdicionarAsync(planta);
            await _repositorioPlanta.SalvarMudancasAsync();

            return Resultado<PlantaDTOSaida>.Ok(MapearPlantaPara(planta));
        }
        catch (Exception ex)
        {
            return Resultado<PlantaDTOSaida>.Erro($"Erro ao identificar planta: {ex.Message}");
        }
    }
}
