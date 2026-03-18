using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.DTOs.Planta;
using PlantaCoreAPI.Domain.Comuns;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Domain.Interfaces;
using PlantaCoreAPI.Application.DTOs.Post;
using PlantaCoreAPI.Domain.Entities;

namespace PlantaCoreAPI.Infrastructure.Services;

public sealed partial class PlantService : IPlantService
{
    private readonly IRepositorioPlanta _repositorioPlanta;
    private readonly IRepositorioNotificacao _repositorioNotificacao;
    private readonly IPlantNetService _servicioPlantNet;
    private readonly ITrefleService _servicioTrefle;
    private readonly IGeminiService _servicioGemini;
    private readonly IFileStorageService _servicioPlantaStorage;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IRepositorioPost _repositorioPost;

    public PlantService(
        IRepositorioPlanta repositorioPlanta,
        IRepositorioNotificacao repositorioNotificacao,
        IPlantNetService servicioPlantNet,
        ITrefleService servicioTrefle,
        IGeminiService servicioGemini,
        IFileStorageService servicioPlantaStorage,
        IHttpClientFactory httpClientFactory,
        IRepositorioPost repositorioPost)
    {
        _repositorioPlanta = repositorioPlanta;
        _repositorioNotificacao = repositorioNotificacao;
        _servicioPlantNet = servicioPlantNet;
        _servicioTrefle = servicioTrefle;
        _servicioGemini = servicioGemini;
        _servicioPlantaStorage = servicioPlantaStorage;
        _httpClientFactory = httpClientFactory;
        _repositorioPost = repositorioPost;
    }

    public async Task<Resultado<PaginaResultado<PlantaDTOSaida>>> BuscarPlantasUsuarioAsync(Guid usuarioId, string termo, int pagina, int tamanho)
    {
        try
        {
            var plantas = await _repositorioPlanta.BuscarPorNomeAsync(termo);
            var plantasUsuario = plantas.Where(p => p.UsuarioId == usuarioId);

            var total = plantasUsuario.Count();
            var itens = plantasUsuario
                .Skip((pagina - 1) * tamanho)
                .Take(tamanho)
                .Select(MapearPlantaPara)
                .ToList();

            return Resultado<PaginaResultado<PlantaDTOSaida>>.Ok(new PaginaResultado<PlantaDTOSaida>
            {
                Itens = itens,
                Pagina = pagina,
                TamanhoPagina = tamanho,
                Total = total
            });
        }
        catch (Exception ex)
        {
            return Resultado<PaginaResultado<PlantaDTOSaida>>.Erro($"Erro ao buscar plantas: {ex.Message}");
        }
    }

    // Adicionando funcionalidade para postar foto após upload na identificação de plantas
    // Corrigindo o método para usar o método de fábrica Post.Criar
    public async Task<Resultado<PostDTOSaida>> PostarFotoIdentificacaoAsync(Guid usuarioId, Guid plantaId, string conteudo)
    {
        var planta = await _repositorioPlanta.ObterPorIdAsync(plantaId);
        if (planta == null)
            return Resultado<PostDTOSaida>.Erro("Planta não encontrada.");

        var post = Post.Criar(usuarioId, conteudo, plantaId, null); // Usando o método de fábrica

        await _repositorioPost.AdicionarAsync(post);
        await _repositorioPost.SalvarMudancasAsync();

        return Resultado<PostDTOSaida>.Ok(new PostDTOSaida
        {
            Id = post.Id,
            PlantaId = post.PlantaId,
            UsuarioId = post.UsuarioId,
            Conteudo = post.Conteudo,
            DataCriacao = post.DataCriacao
        });
    }
}
