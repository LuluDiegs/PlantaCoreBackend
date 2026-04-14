using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Application.DTOs.Planta;
using PlantaCoreAPI.Application.Comuns;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using PlantaCoreAPI.Domain.Entities;

namespace PlantaCoreAPI.Application.Services;

public sealed partial class PlantService
{
    public async Task<Resultado<RecomendacaoPlantaComImagemDTO>> GerarRecomendacaoPlantaAsync(DadosRecomendacaoPlantaParaIA dados)
    {
        string? respostaGeminiJson = await _servicioGemini.GerarRecomendacaoPlantaAsync(dados);

        _logger.LogInformation(
            "Resposta do Gemini recebida para recomendação de planta. Resposta: {Resposta}",
            respostaGeminiJson);

        if (string.IsNullOrWhiteSpace(respostaGeminiJson))
            return Resultado<RecomendacaoPlantaComImagemDTO>.Erro(
                "A resposta do Gemini está vazia ou inválida ao gerar recomendação de planta.");

        RecomendacaoPlantaDTO? recomendacaoDto;

        try
        {
            recomendacaoDto = JsonSerializer.Deserialize<RecomendacaoPlantaDTO>(respostaGeminiJson);
        }
        catch (JsonException ex)
        {
            return Resultado<RecomendacaoPlantaComImagemDTO>.Erro(
                $"Erro ao converter resposta do Gemini para RecomendacaoPlantaDTO. Detalhes: {ex.Message}");
        }

        if (recomendacaoDto is null)
            return Resultado<RecomendacaoPlantaComImagemDTO>.Erro(
                "Não foi possível interpretar a resposta do Gemini como uma recomendação válida de planta.");

        _logger.LogInformation(
            "Recomendação do Gemini processada com sucesso para a planta: {NomeCientifico}",
            recomendacaoDto.NomeCientifico);

        Resultado<ResultadoBuscaPlantaDTOSaida> resultadoTrefle =
            await BuscarPlantasTrefleAsync(recomendacaoDto.NomeCientifico, 1);

        if (!resultadoTrefle.Sucesso)
            return Resultado<RecomendacaoPlantaComImagemDTO>.Erro(
                $"Falha ao consultar o Trefle para a planta '{recomendacaoDto.NomeCientifico}': {resultadoTrefle.Mensagem}");

        if (resultadoTrefle.Dados is null)
            return Resultado<RecomendacaoPlantaComImagemDTO>.Erro(
                $"O Trefle retornou uma resposta vazia para a planta '{recomendacaoDto.NomeCientifico}': {resultadoTrefle.Mensagem}");

        PlantaBuscaDTOSaida plantaTrefle = resultadoTrefle.Dados.Plantas.First();

        if (string.IsNullOrWhiteSpace(plantaTrefle.UrlImagem))
            return Resultado<RecomendacaoPlantaComImagemDTO>.Erro(
                "A planta encontrada no Trefle não possui imagem disponível.");

        var recomendacaoImagemDto = new RecomendacaoPlantaComImagemDTO
        {
            NomeComum = recomendacaoDto.NomeComum,
            UrlImagem = plantaTrefle.UrlImagem,
            Justificativa = recomendacaoDto.Justificativa,
        };

        return Resultado<RecomendacaoPlantaComImagemDTO>.Ok(recomendacaoImagemDto);
    }

    public async Task<Resultado<RecomendacaoPlantaComImagemDTO>> GerarSalvarRecomendacaoPlantaAsync(DadosRecomendacaoPlantaParaIA dados, Guid usuarioId)
    {
        var resultado = await GerarRecomendacaoPlantaAsync(dados);

        if (!resultado.Sucesso)
            return Resultado<RecomendacaoPlantaComImagemDTO>.Erro(
                "Falha ao gerar recomendação.");

        if (resultado.Dados is null)
            return Resultado<RecomendacaoPlantaComImagemDTO>.Erro(
                "Resultado da recomendação retornou uma resposta vazia.");

        var recomendacao = new Recomendacao
        {
            Id = Guid.NewGuid(),
            NomeComum = resultado.Dados.NomeComum,
            UrlImagem = resultado.Dados.UrlImagem,
            Justificativa = resultado.Dados.Justificativa,
            Experiencia = dados.Experiencia,
            Iluminacao = dados.Iluminacao,
            Regagem = dados.Regagem,
            Seguranca = dados.Seguranca,
            Proposito = dados.Proposito,
            UsuarioId = usuarioId,
        };

        await _repositorioRecomendacao.AdicionarAsync(recomendacao);
        await _repositorioNotificacao.SalvarMudancasAsync();

        return resultado;
    }
}
