using PlantaCoreAPI.Domain.Entities;

namespace PlantaCoreAPI.Domain.Interfaces;

public interface IRepositorioLoja
{
    
    Task<IEnumerable<Loja>> ObterTodosAsync(CancellationToken cancellationToken);
    Task<IEnumerable<Loja>> ObterPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken);
    Task<Loja?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task AdicionarAsync(Loja loja, CancellationToken cancellationToken);
    void Atualizar(Loja loja);
    void Remover(Loja loja);
    Task<bool> SalvarMudancasAsync(CancellationToken cancellationToken);
}