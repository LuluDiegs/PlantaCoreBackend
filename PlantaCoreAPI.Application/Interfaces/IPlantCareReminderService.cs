using PlantaCoreAPI.Domain.Entities;

namespace PlantaCoreAPI.Application.Interfaces;

public interface IPlantCareReminderService
{
    Task GerarLembreteCuidadoAsync(Guid plantaId);
    Task GerarLembretesParaTodosPlantas();
    Task<IEnumerable<Planta>> ObterPlantasComNotificacoesHabilitadasAsync();
    Task EnviarNotificacaoAsync(Guid usuarioId, Guid plantaId, string mensagem);
}
