using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantaCoreAPI.Application.Interfaces;
using System.Security.Claims;

namespace PlantaCoreAPI.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class NotificacaoController : ControllerBase
{
    private readonly INotificationService _servicioNotificacao;

    public NotificacaoController(INotificationService servicioNotificacao)
    {
        _servicioNotificacao = servicioNotificacao;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterNotificacoes()
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _servicioNotificacao.ObterNotificacoesAsync(usuarioId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpGet("nao-lidas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterNotificacoesNaoLidas()
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _servicioNotificacao.ObterNaoLidasAsync(usuarioId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPut("{notificacaoId:guid}/marcar-como-lida")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarcarComoLida(Guid notificacaoId)
    {
        var resultado = await _servicioNotificacao.MarcarComoLidaAsync(notificacaoId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPut("marcar-todas-como-lidas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarcarTodasComoLidas()
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _servicioNotificacao.MarcarTodasComoLidasAsync(usuarioId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpDelete("{notificacaoId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeletarNotificacao(Guid notificacaoId)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _servicioNotificacao.DeletarNotificacaoAsync(notificacaoId, usuarioId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeletarTodasNotificacoes()
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _servicioNotificacao.DeletarTodasNotificacoesAsync(usuarioId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }
}
