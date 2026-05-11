using PlantaCoreAPI.Domain.Comuns;
using PlantaCoreAPI.Domain.Entities;

namespace PlantaCoreAPI.Domain.Interfaces;

public interface IRepositorioPlanta : IRepositorio<Planta>
{
    Task<IEnumerable<Planta>> ObterTodosComUsuarioAsync();
    Task<Planta?> ObterPorNomeCientificoAsync(string nomeCientifico);
    Task<Planta?> ObterPorNomeCientificoEUsuarioAsync(string nomeCientifico, Guid usuarioId);
    Task<IEnumerable<Planta>> ObterPorUsuarioAsync(Guid usuarioId);
    Task<PaginaResultado<Planta>> ObterPorUsuarioPaginadoAsync(Guid usuarioId, int pagina, int tamanho);
    Task<IEnumerable<Planta>> BuscarPorNomeAsync(string termo);
    Task<PaginaResultado<Planta>> BuscarPorUsuarioETermoAsync(Guid usuarioId, string termo, int pagina, int tamanho);
    Task<IEnumerable<Planta>> ObterTodasParaLembreteAsync(int skip, int take);
}
