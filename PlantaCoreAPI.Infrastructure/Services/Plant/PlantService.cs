using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Domain.Interfaces;

namespace PlantaCoreAPI.Infrastructure.Services;

public sealed partial class PlantService : IPlantService
{
    private readonly IRepositorioPlanta _repositorioPlanta;
    private readonly IRepositorioNotificacao _repositorioNotificacao;
    private readonly IPlantNetService _servicioPlantNet;
    private readonly ITrefleService _servicioTrefle;
    private readonly IGeminiService _servicioGemini;
    private readonly IFileStorageService _servicioPlantaStorage;
    private readonly IHttpClientFactory _httpClientFactory;

    public PlantService(
        IRepositorioPlanta repositorioPlanta,
        IRepositorioNotificacao repositorioNotificacao,
        IPlantNetService servicioPlantNet,
        ITrefleService servicioTrefle,
        IGeminiService servicioGemini,
        IFileStorageService servicioPlantaStorage,
        IHttpClientFactory httpClientFactory)
    {
        _repositorioPlanta = repositorioPlanta;
        _repositorioNotificacao = repositorioNotificacao;
        _servicioPlantNet = servicioPlantNet;
        _servicioTrefle = servicioTrefle;
        _servicioGemini = servicioGemini;
        _servicioPlantaStorage = servicioPlantaStorage;
        _httpClientFactory = httpClientFactory;
    }
}
