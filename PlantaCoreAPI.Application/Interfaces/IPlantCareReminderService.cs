namespace PlantaCoreAPI.Application.Interfaces;

public interface IPlantCareReminderService
{
    Task GerarLembreteCuidadoAsync(Guid plantaId);
    Task GerarLembretesParaTodosPlantas();
}
