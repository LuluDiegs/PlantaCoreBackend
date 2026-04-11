using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.DTOs.Planta;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Application.Services.Plant;
using PlantaCoreAPI.Domain.Comuns;
using Microsoft.Extensions.Logging;

namespace PlantaCoreAPI.Application.Services;

public sealed partial class PlantService
{
    public async Task<Resultado<IEnumerable<PlantaDTOSaida>>> ListarPlantasUsuarioAsync(Guid usuarioId)
    {
        try
        {
            var plantas = await _repositorioPlanta.ObterPorUsuarioAsync(usuarioId);
            return Resultado<IEnumerable<PlantaDTOSaida>>.Ok(plantas.Select(MapearPlantaPara).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar plantas do usuário {UsuarioId}", usuarioId);
            return Resultado<IEnumerable<PlantaDTOSaida>>.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado<PlantaDTOSaida>> ObterPlantaAsync(Guid plantaId)
    {
        try
        {
            var planta = await _repositorioPlanta.ObterPorIdAsync(plantaId);
            if (planta == null)
                return Resultado<PlantaDTOSaida>.Erro("Planta não encontrada");
            return Resultado<PlantaDTOSaida>.Ok(MapearPlantaPara(planta));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter planta {PlantaId}", plantaId);
            return Resultado<PlantaDTOSaida>.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado<bool>> ExcluirPlantaAsync(Guid plantaId, Guid usuarioId)
    {
        try
        {
            var planta = await _repositorioPlanta.ObterPorIdAsync(plantaId);
            if (planta == null)
                return Resultado<bool>.Erro("Planta não encontrada");
            if (planta.UsuarioId != usuarioId)
                return Resultado<bool>.Erro("Você não tem permissão para excluir esta planta");
            await _repositorioPlanta.RemoverAsync(planta);
            await _repositorioPlanta.SalvarMudancasAsync();
            return Resultado<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir planta {PlantaId}", plantaId);
            return Resultado<bool>.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado<PlantaDTOSaida>> AdicionarPlantaDoTrefleAsync(Guid usuarioId, int plantaTrefleId, string? nomeCientifico, string? urlImagem)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(urlImagem))
                return Resultado<PlantaDTOSaida>.Erro("urlImagem é obrigatório");
            if (string.IsNullOrWhiteSpace(nomeCientifico) && plantaTrefleId <= 0)
                return Resultado<PlantaDTOSaida>.Erro("nomeCientifico ou plantaTrefleId são obrigatórios");
            var plantaTrefle = plantaTrefleId > 0
                ? await _servicioTrefle.ObterPlantaPorIdAsync(plantaTrefleId)
                : null;
            var nomeCientificoCorreto = plantaTrefle?.NomeCientifico
                ?? nomeCientifico
                ?? "Desconhecido";
            var plantaExistente = await _repositorioPlanta.ObterPorNomeCientificoAsync(nomeCientificoCorreto);
            if (plantaExistente != null && plantaExistente.UsuarioId == usuarioId)
                return Resultado<PlantaDTOSaida>.Ok(MapearPlantaPara(plantaExistente));
            var descricaoGemini = await _servicioGemini.GerarDescricaoPlantaAsync(new DadosPlantaParaIA { NomeCientifico = nomeCientificoCorreto });
            var dadosEnriquecidos = ExtrairDadosDoGemini(descricaoGemini, nomeCientificoCorreto, plantaTrefle);
            var fotoFinal = await BaixarESalvarFotoAsync(urlImagem, usuarioId);
            var novaPlanta = CriarPlantaDeEnriquecidos(usuarioId, dadosEnriquecidos, fotoFinal);
            await _repositorioPlanta.AdicionarAsync(novaPlanta);
            await _repositorioPlanta.SalvarMudancasAsync();
            return Resultado<PlantaDTOSaida>.Ok(MapearPlantaPara(novaPlanta));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar planta do Trefle para usuário {UsuarioId}", usuarioId);
            return Resultado<PlantaDTOSaida>.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado<PaginaResultado<PlantaDTOSaida>>> ListarPlantasUsuarioPaginadoAsync(Guid usuarioId, int pagina, int tamanho)
    {
        try
        {
            var paginaPlanta = await _repositorioPlanta.ObterPorUsuarioPaginadoAsync(usuarioId, pagina, tamanho);
            return Resultado<PaginaResultado<PlantaDTOSaida>>.Ok(new PaginaResultado<PlantaDTOSaida>
            {
                Itens = paginaPlanta.Itens.Select(MapearPlantaPara),
                Pagina = paginaPlanta.Pagina,
                TamanhoPagina = paginaPlanta.TamanhoPagina,
                Total = paginaPlanta.Total
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar plantas paginadas do usuário {UsuarioId}", usuarioId);
            return Resultado<PaginaResultado<PlantaDTOSaida>>.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }
}
