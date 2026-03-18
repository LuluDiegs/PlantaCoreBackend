using PlantaCoreAPI.Domain.Comuns;

namespace PlantaCoreAPI.Domain.Interfaces;

public interface IRepositorioComunidade : IRepositorio<Entities.Comunidade>
{
    Task<IEnumerable<Entities.Comunidade>> BuscarPorNomeAsync(string termo);
    Task<PaginaResultado<Entities.Comunidade>> ListarPaginadoAsync(int pagina, int tamanho);
    Task<Entities.Comunidade?> ObterComMembrosAsync(Guid comunidadeId);
    Task<bool> UsuarioEhMembroAsync(Guid comunidadeId, Guid usuarioId);
    Task<Entities.MembroComunidade?> ObterMembroAsync(Guid comunidadeId, Guid usuarioId);
    Task AdicionarMembroAsync(Entities.MembroComunidade membro);
    Task RemoverMembroAsync(Entities.MembroComunidade membro);
    Task<PaginaResultado<Entities.Comunidade>> ObterComunidadesDoUsuarioAsync(Guid usuarioId, int pagina, int tamanho);
}
