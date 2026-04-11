using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.DTOs.Planta;
using PlantaCoreAPI.Application.DTOs.Post;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Domain.Comuns;
using PlantaCoreAPI.Domain.Entities;
using PlantaCoreAPI.Domain.Interfaces;
using PlantaCoreAPI.Application.Comuns.Eventos;
using Microsoft.Extensions.Logging;

namespace PlantaCoreAPI.Application.Services;

public sealed partial class PlantService : IPlantService
{
    private readonly IRepositorioPlanta _repositorioPlanta;
    private readonly IRepositorioNotificacao _repositorioNotificacao;
    private readonly ILogger<PlantService> _logger;
    private readonly IPlantNetService _servicioPlantNet;
    private readonly ITrefleService _servicioTrefle;
    private readonly IGeminiService _servicioGemini;
    private readonly IFileStorageService _servicioPlantaStorage;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IRepositorioPost _repositorioPost;
    private readonly IEventoDispatcher _eventoDispatcher;

    public PlantService(
        IRepositorioPlanta repositorioPlanta,
        IRepositorioNotificacao repositorioNotificacao,
        IPlantNetService servicioPlantNet,
        ITrefleService servicioTrefle,
        IGeminiService servicioGemini,
        IFileStorageService servicioPlantaStorage,
        IHttpClientFactory httpClientFactory,
        IRepositorioPost repositorioPost,
        IEventoDispatcher eventoDispatcher,
        ILogger<PlantService> logger)
    {
        _repositorioPlanta = repositorioPlanta;
        _repositorioNotificacao = repositorioNotificacao;
        _logger = logger;
        _servicioPlantNet = servicioPlantNet;
        _servicioTrefle = servicioTrefle;
        _servicioGemini = servicioGemini;
        _servicioPlantaStorage = servicioPlantaStorage;
        _httpClientFactory = httpClientFactory;
        _repositorioPost = repositorioPost;
        _eventoDispatcher = eventoDispatcher;
    }

    public async Task<Resultado<PaginaResultado<PlantaDTOSaida>>> BuscarPlantasUsuarioAsync(Guid usuarioId, string termo, int pagina, int tamanho)
    {
        try
        {
            var paginado = await _repositorioPlanta.BuscarPorUsuarioETermoAsync(usuarioId, termo, pagina, tamanho);
            return Resultado<PaginaResultado<PlantaDTOSaida>>.Ok(new PaginaResultado<PlantaDTOSaida>
            {
                Itens = paginado.Itens.Select(MapearPlantaPara).ToList(),
                Pagina = paginado.Pagina,
                TamanhoPagina = paginado.TamanhoPagina,
                Total = paginado.Total
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar plantas do usuário {UsuarioId}", usuarioId);
            return Resultado<PaginaResultado<PlantaDTOSaida>>.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado<PostDTOSaida>> PostarFotoIdentificacaoAsync(Guid usuarioId, Guid plantaId, string conteudo)
    {
        var planta = await _repositorioPlanta.ObterPorIdAsync(plantaId);
        if (planta == null)
            return Resultado<PostDTOSaida>.Erro("Planta não encontrada.");
        var post = Post.Criar(usuarioId, conteudo, plantaId, null);
        await _repositorioPost.AdicionarAsync(post);
        await _repositorioPost.SalvarMudancasAsync();
        return Resultado<PostDTOSaida>.Ok(new PostDTOSaida
        {
            Id = post.Id,
            PlantaId = post.PlantaId,
            UsuarioId = post.UsuarioId,
            NomeUsuario = "",
            Conteudo = post.Conteudo,
            TotalCurtidas = 0,
            TotalComentarios = 0,
            CurtiuUsuario = false,
            DataCriacao = post.DataCriacao,
            DataAtualizacao = post.DataAtualizacao
        });
    }

    public async Task<IEnumerable<PlantaDTOSaida>> BuscarPlantasPorNomeAsync(string termo)
    {
        var todas = await _repositorioPlanta.ObterTodosAsync();
        return todas
            .Where(p => (p.NomeCientifico != null && p.NomeCientifico.Contains(termo, StringComparison.OrdinalIgnoreCase)) ||
                        (p.NomeComum != null && p.NomeComum.Contains(termo, StringComparison.OrdinalIgnoreCase)))
            .Select(p => new PlantaDTOSaida
            {
                Id = p.Id,
                NomeCientifico = p.NomeCientifico,
                NomeComum = p.NomeComum,
                FotoPlanta = p.FotoPlanta
            });
    }

    public async Task<IEnumerable<PostDTOSaida>> ListarPostsDaPlantaAsync(Guid plantaId)
    {
        var posts = await _repositorioPost.ObterPorPlantaAsync(plantaId);
        return posts.Select(p => new PostDTOSaida
        {
            Id = p.Id,
            Conteudo = p.Conteudo,
            UsuarioId = p.UsuarioId,
            DataCriacao = p.DataCriacao
        });
    }
}
