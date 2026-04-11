using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.DTOs.Evento;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.API.Utils;

namespace PlantaCoreAPI.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
[Tags("Evento")]
public class EventoController : ControllerBase
{
    private readonly IEventoService _servicoEvento;

    public EventoController(IEventoService servicoEvento)
    {
        _servicoEvento = servicoEvento;
    }

    private bool TentarObterUsuarioId(out Guid usuarioId)
    {
        usuarioId = Guid.Empty;
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim is not null && Guid.TryParse(claim.Value, out usuarioId);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterEventos()
    {
        Resultado<List<EventoDTOSaida>> resultado = await _servicoEvento.ObterEventosAsync();
        if (!resultado.Sucesso)
            return StatusCode(StatusCodes.Status500InternalServerError, ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao(true, resultado.Dados));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterEvento([FromRoute] Guid id)
    {
        Resultado<EventoDTOSaida> resultado = await _servicoEvento.ObterEventoPorIdAsync(id);
        if (!resultado.Sucesso)
            return NotFound(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao(true, resultado.Dados));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AdicionarEvento([FromBody] CriarEventoDTO eventoDTO)
    {
        if (!TentarObterUsuarioId(out var anfitriaoId))
            return Unauthorized();

        var resultado = await _servicoEvento.AdicionarEventoAsync(eventoDTO, anfitriaoId);
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return StatusCode(StatusCodes.Status201Created, ResponseHelper.Padrao(true, new { id = resultado.Dados }, meta: new { mensagem = resultado.Mensagem }));
    }

    [HttpPost("{eventoId:guid}/participacao")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarcarParticipacao([FromRoute] Guid eventoId)
    {
        if (!TentarObterUsuarioId(out var usuarioId))
            return Unauthorized();

        Resultado resultado = await _servicoEvento.MarcarParticipacaoEvento(eventoId, usuarioId);
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao<object>(true, null, meta: new { mensagem = resultado.Mensagem }));
    }

    [HttpDelete("{eventoId:guid}/participacao")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DesmarcarParticipacao([FromRoute] Guid eventoId)
    {
        if (!TentarObterUsuarioId(out var usuarioId))
            return Unauthorized();

        Resultado resultado = await _servicoEvento.DesmarcarParticipacaoEvento(eventoId, usuarioId);
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao<object>(true, null, meta: new { mensagem = resultado.Mensagem }));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AtualizarEvento([FromRoute] Guid id, [FromBody] AtualizarEventoDTO eventoDTO)
    {
        if (!TentarObterUsuarioId(out var usuarioId))
            return Unauthorized();

        Resultado resultado = await _servicoEvento.AtualizarEvento(id, eventoDTO, usuarioId);
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao<object>(true, null, meta: new { mensagem = resultado.Mensagem }));
    }

    [HttpDelete("{eventoId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoverEvento([FromRoute] Guid eventoId)
    {
        if (!TentarObterUsuarioId(out var usuarioId))
            return Unauthorized();

        Resultado resultado = await _servicoEvento.RemoverEvento(eventoId, usuarioId);
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao<object>(true, null, meta: new { mensagem = resultado.Mensagem }));
    }

    [HttpGet("{eventoId:guid}/participantes")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListarParticipantes([FromRoute] Guid eventoId)
    {
        var resultado = await _servicoEvento.ListarParticipantesAsync(eventoId);
        if (!resultado.Sucesso)
            return NotFound(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao(true, resultado.Dados));
    }
}
