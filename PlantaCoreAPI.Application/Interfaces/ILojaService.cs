using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Domain.Entities;

namespace PlantaCoreAPI.Application.Interfaces;

public interface ILojaService
{
    IEnumerable<string> Validar(Loja loja);
    Task<Resultado<IEnumerable<Loja>>> ObterTodosAsync(CancellationToken cancellationToken);
    Task<Resultado<IEnumerable<Loja>>> ObterPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken);
    Task<Resultado<Loja>> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Resultado> AdicionarAsync(Loja loja, CancellationToken cancellationToken);
    Task<Resultado> AtualizarAsync(Loja loja, CancellationToken cancellationToken);
    Task<Resultado> RemoverAsync(Guid lojaId, Guid usuarioId, CancellationToken cancellationToken);
}