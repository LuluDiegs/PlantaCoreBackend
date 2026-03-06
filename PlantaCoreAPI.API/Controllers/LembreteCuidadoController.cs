using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantaCoreAPI.Application.Interfaces;

namespace PlantaCoreAPI.API.Controllers;

[ApiController]
[Route("api/v1/lembretes-cuidado")]
[Authorize]
[Produces("application/json")]
public class LembreteCuidadoController : ControllerBase
{
    private readonly IPlantCareReminderService _servicoLembrete;

    public LembreteCuidadoController(IPlantCareReminderService servicoLembrete)
    {
        _servicoLembrete = servicoLembrete;
    }

    [HttpPost("gerar-para-todas-plantas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GerarLembretesParaTodas()
    {
        await _servicoLembrete.GerarLembretesParaTodosPlantas();
        return Ok(new { sucesso = true, mensagem = "Lembretes gerados para todas as plantas" });
    }
}
