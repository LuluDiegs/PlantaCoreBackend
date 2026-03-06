namespace PlantaCoreAPI.Domain.Interfaces;

public interface IRepositorio<T> where T : class
{
    Task<T?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<T>> ObterTodosAsync();
    Task AdicionarAsync(T entidade);
    Task AtualizarAsync(T entidade);
    Task RemoverAsync(T entidade);
    Task<bool> SalvarMudancasAsync();
}
