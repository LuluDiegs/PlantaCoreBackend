using PlantaCoreAPI.Domain.Comuns;

namespace PlantaCoreAPI.Domain.Interfaces;

public interface IRepositorioPlanta : IRepositorio<Entities.Planta>
{
    Task<Entities.Planta?> ObterPorNomeCientificoAsync(string nomeCientifico);
    Task<IEnumerable<Entities.Planta>> ObterPorUsuarioAsync(Guid usuarioId);
    Task<PaginaResultado<Entities.Planta>> ObterPorUsuarioPaginadoAsync(Guid usuarioId, int pagina, int tamanho);
    Task<IEnumerable<Entities.Planta>> BuscarPorNomeAsync(string termo);
}
