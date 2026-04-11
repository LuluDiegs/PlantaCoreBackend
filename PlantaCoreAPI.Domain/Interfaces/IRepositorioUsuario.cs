using PlantaCoreAPI.Domain.Comuns;

namespace PlantaCoreAPI.Domain.Interfaces;

public interface IRepositorioUsuario : IRepositorio<Entities.Usuario>
{
    Task<Entities.Usuario?> ObterPorEmailAsync(string email);
    Task<Entities.Usuario?> ObterPorEmailIncluindoInativosAsync(string email);
    Task<bool> EmailJaExisteAsync(string email);
    Task<Entities.Usuario?> ObterComPlantasAsync(Guid usuarioId);
    Task<PaginaResultado<Entities.Usuario>> ObterSeguidoresPaginadoAsync(Guid usuarioId, int pagina, int tamanho);
    Task<PaginaResultado<Entities.Usuario>> ObterSeguindoPaginadoAsync(Guid usuarioId, int pagina, int tamanho);
    Task<IEnumerable<Entities.Usuario>> BuscarPorNomeAsync(string termo);
    Task<IEnumerable<Entities.Usuario>> ObterSugestoesAsync(IEnumerable<Guid> excluirIds, int quantidade);
    Task<bool> UsuarioSegueAsync(Guid seguidorId, Guid seguidoId);
    Task<Entities.Usuario?> ObterComSeguindoAsync(Guid usuarioId);
    Task<Entities.Usuario?> ObterComSeguindoESeguidoresAsync(Guid usuarioId);
    Task<IEnumerable<Entities.Usuario>> ObterPorIdsAsync(IEnumerable<Guid> ids);
    Task<Entities.Usuario?> ObterPorIdTrackedAsync(Guid id);
}
