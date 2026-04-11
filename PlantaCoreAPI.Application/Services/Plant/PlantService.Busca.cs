using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.DTOs.Planta;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Application.Services.Plant;
using Microsoft.Extensions.Logging;

namespace PlantaCoreAPI.Application.Services;

public sealed partial class PlantService
{
    public async Task<Resultado<ResultadoBuscaPlantaDTOSaida>> BuscarPlantasTrefleAsync(string nomePlanta, int pagina)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(nomePlanta))
                return Resultado<ResultadoBuscaPlantaDTOSaida>.Erro("Nome da planta obrigatório");
            if (pagina < 0)
                pagina = 0;
            var resultadoTrefle = await _servicioTrefle.BuscarPlantasAsync(nomePlanta, pagina);
            if (resultadoTrefle == null || resultadoTrefle.Dados == null || resultadoTrefle.Dados.Count == 0)
                return Resultado<ResultadoBuscaPlantaDTOSaida>.Erro("Nenhuma planta encontrada");
            var plantasUnicas = resultadoTrefle.Dados
                .GroupBy(p => p.Id)
                .Select(g => g.First())
                .ToList();
            var plantas = plantasUnicas.Select(p => new PlantaBuscaDTOSaida
            {
                Id = p.Id,
                NomeComum = p.NomeComum,
                NomeCientifico = p.NomeCientifico,
                Slug = p.Slug,
                UrlImagem = p.UrlImagem,
                Genero = p.Genero,
                Familia = p.Familia
            }).ToList();
            return Resultado<ResultadoBuscaPlantaDTOSaida>.Ok(new ResultadoBuscaPlantaDTOSaida
            {
                Plantas = plantas,
                Paginacao = new MetadadosPaginacaoDTOSaida
                {
                    Total = resultadoTrefle.Metadados?.Total ?? 0,
                    TotalPaginas = resultadoTrefle.Metadados?.TotalPaginas ?? 0,
                    PaginaAtual = resultadoTrefle.Metadados?.Pagina ?? 0
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar plantas no Trefle para {NomePlanta}", nomePlanta);
            return Resultado<ResultadoBuscaPlantaDTOSaida>.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado<PlantaDTOSaida>> BuscarPlantaAsync(Guid usuarioId, BuscaPlantaDTOEntrada entrada)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(entrada.NomePlanta))
                return Resultado<PlantaDTOSaida>.Erro("Nome da planta obrigatório");
            if (entrada.Pagina < 0)
                entrada.Pagina = 0;
            var plantaExistente = await _repositorioPlanta.BuscarPorNomeAsync(entrada.NomePlanta);
            if (plantaExistente.Any())
                return Resultado<PlantaDTOSaida>.Ok(MapearPlantaPara(plantaExistente.First()));
            var tarefaTrefle = _servicioTrefle.BuscarPlantasAsync(entrada.NomePlanta, entrada.Pagina);
            var tarefaGemini = _servicioGemini.GerarDescricaoPlantaAsync(new DadosPlantaParaIA { NomeCientifico = entrada.NomePlanta });
            await Task.WhenAll(tarefaTrefle, tarefaGemini);
            var resultadoTrefle = await tarefaTrefle;
            var descricaoGemini = await tarefaGemini;
            if ((resultadoTrefle == null || resultadoTrefle.Dados == null || resultadoTrefle.Dados.Count == 0) &&
                string.IsNullOrWhiteSpace(descricaoGemini))
                return Resultado<PlantaDTOSaida>.Erro("Planta não encontrada");
            var plantaTrefle = resultadoTrefle?.Dados?.FirstOrDefault();
            var dadosEnriquecidos = ExtrairDadosDoGemini(descricaoGemini, entrada.NomePlanta, plantaTrefle);
            return Resultado<PlantaDTOSaida>.Ok(new PlantaDTOSaida
            {
                Id = Guid.Empty,
                NomeCientifico = dadosEnriquecidos.NomeCientifico,
                NomeComum = dadosEnriquecidos.NomeComum,
                Familia = dadosEnriquecidos.Familia,
                Genero = dadosEnriquecidos.Genero,
                Toxica = dadosEnriquecidos.Toxica == "Sim",
                DescricaoToxicidade = dadosEnriquecidos.DescricaoToxicidade,
                ToxicaAnimais = dadosEnriquecidos.ToxicaAnimais == "Sim",
                DescricaoToxicidadeAnimais = dadosEnriquecidos.DescricaoToxicidadeAnimais,
                ToxicaCriancas = dadosEnriquecidos.ToxicaCriancas == "Sim",
                DescricaoToxicidadeCriancas = dadosEnriquecidos.DescricaoToxicidadeCriancas,
                RequisitosLuz = dadosEnriquecidos.RequisitosLuz,
                RequisitosAgua = dadosEnriquecidos.RequisitosAgua,
                RequisitosTemperatura = dadosEnriquecidos.RequisitosTemperatura,
                Cuidados = dadosEnriquecidos.Cuidados,
                FotoPlanta = plantaTrefle?.UrlImagem,
                DataIdentificacao = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar planta {NomePlanta}", entrada.NomePlanta);
            return Resultado<PlantaDTOSaida>.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }
}
