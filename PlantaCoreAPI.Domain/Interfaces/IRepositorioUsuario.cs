using PlantaCoreAPI.Domain.Comuns;

namespace PlantaCoreAPI.Domain.Interfaces;

public interface IRepositorioUsuario : IRepositorio<Entities.Usuario>
{
    Task<Entities.Usuario?> ObterPorEmailAsync(string email);
    Task<bool> EmailJaExisteAsync(string email);
    Task<Entities.Usuario?> ObterComPlantasAsync(Guid usuarioId);
    Task<PaginaResultado<Entities.Usuario>> ObterSeguidoresPaginadoAsync(Guid usuarioId, int pagina, int tamanho);
    Task<PaginaResultado<Entities.Usuario>> ObterSeguindoPaginadoAsync(Guid usuarioId, int pagina, int tamanho);
}
