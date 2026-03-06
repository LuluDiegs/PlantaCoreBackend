namespace PlantaCoreAPI.Domain.Interfaces;

public interface IRepositorioTokenRefresh : IRepositorio<Entities.TokenRefresh>
{
    Task<Entities.TokenRefresh?> ObterPorTokenAsync(string token);
    Task<IEnumerable<Entities.TokenRefresh>> ObterPorUsuarioAsync(Guid usuarioId);
    Task RevogarTokensUsuarioAsync(Guid usuarioId);
}
