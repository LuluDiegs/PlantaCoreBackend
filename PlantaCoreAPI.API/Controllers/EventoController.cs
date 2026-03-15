using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.DTOs;
using PlantaCoreAPI.Application.DTOs.Evento;
using PlantaCoreAPI.Application.Services;

namespace PlantaCoreAPI.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class EventoController : ControllerBase
{
    private readonly EventoService _servicoEvento;

    public EventoController(EventoService servicoEvento)
    {
        _servicoEvento = servicoEvento;
    }

    [HttpGet]
    public async Task<IActionResult> ObterEventos()
    {
        Resultado<List<EventoDTOSaida>> resultado = await _servicoEvento.ObterEventosAsync();
        return Ok(resultado.Dados);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterEvento([FromRoute] Guid id)
    {
        Resultado<EventoDTOSaida> resultado = await _servicoEvento.ObterEventoPorIdAsync(id);

        return resultado.Sucesso
            ? Ok(resultado.Dados)
            : NotFound(resultado.Mensagem);
    }

    [HttpPost]
    public async Task<IActionResult> AdicionarEvento([FromBody] CriarEventoDTO eventoDTO)
    {
        Claim? claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim is null) return Unauthorized();

        string valor = claim.Value;

        bool sucesso = Guid.TryParse(valor, out Guid anfitriaoId);
        if (!sucesso) return Unauthorized();

        Resultado resultado = await _servicoEvento.AdicionarEventoAsync(eventoDTO, anfitriaoId);
  
        return resultado.Sucesso
            ? Ok(resultado.Mensagem)
            : BadRequest(resultado.Mensagem);
    }

    [HttpPut("marcar-participacao")]
    public async Task<IActionResult> MarcarParticipacaoEvento(Guid eventoId)
    {
        Claim? claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim is null) return Unauthorized();

        string valor = claim.Value;

        bool sucesso = Guid.TryParse(valor, out Guid usuarioId);
        if (!sucesso) return Unauthorized();

        Resultado resultado = await _servicoEvento.MarcarParticipacaoEvento(eventoId, usuarioId);

        return resultado.Sucesso
            ? Ok(resultado.Mensagem)
            : BadRequest(resultado.Mensagem);
    }

    [HttpPut("desmarcar-participacao")]
    public async Task<IActionResult> DesmarcarParticipacaoEvento(Guid eventoId)
    {
        Claim? claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim is null) return Unauthorized();

        string valor = claim.Value;

        bool sucesso = Guid.TryParse(valor, out Guid usuarioId);
        if (!sucesso) return Unauthorized();

        Resultado resultado = await _servicoEvento.DesmarcarParticipacaoEvento(eventoId, usuarioId);

        return resultado.Sucesso
            ? Ok(resultado.Mensagem)
            : BadRequest(resultado.Mensagem);
    }

    [HttpPut]
    public async Task<IActionResult> AtualizarEvento([FromBody] AtualizarEventoDTO eventoDTO)
    {
        Claim? claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim is null) return Unauthorized();

        string valor = claim.Value;

        bool sucesso = Guid.TryParse(valor, out Guid usuarioId);
        if (!sucesso) return Unauthorized();

        Resultado resultado = await _servicoEvento.AtualizarEvento(eventoDTO, usuarioId);

        return resultado.Sucesso
            ? Ok(resultado.Mensagem)
            : BadRequest(resultado.Mensagem);
    }

    [HttpDelete("{eventoId:guid}")]
    public async Task<IActionResult> RemoverEvento([FromRoute] Guid eventoId)
    {
        Claim? claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim is null) return Unauthorized();

        string valor = claim.Value;

        bool sucesso = Guid.TryParse(valor, out Guid usuarioId);
        if (!sucesso) return Unauthorized();

        Resultado resultado = await _servicoEvento.RemoverEvento(eventoId, usuarioId);

        return resultado.Sucesso
            ? Ok(resultado.Mensagem)
            : BadRequest(resultado.Mensagem);
    }
}