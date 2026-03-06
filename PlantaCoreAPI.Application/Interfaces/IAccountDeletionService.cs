using PlantaCoreAPI.Application.Comuns;

namespace PlantaCoreAPI.Application.Interfaces;

public interface IAccountDeletionService
{
    Task<Resultado> ExcluirContaCompleteAsync(Guid usuarioId);
}
