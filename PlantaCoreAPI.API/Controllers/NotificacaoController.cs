using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.API.Utils;
using System.Security.Claims;

namespace PlantaCoreAPI.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
[Tags("Notificacao")]
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
    public async Task<IActionResult> ObterNotificacoes([FromQuery] int pagina = 1, [FromQuery] int tamanho = 10)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _servicioNotificacao.ObterNotificacoesPaginadasAsync(usuarioId, pagina, tamanho);
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao(true, resultado.Dados));
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

        var resultado = await _servicioNotificacao.ObterNotificacoesPaginadasAsync(usuarioId, 1, int.MaxValue);
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        var naoLidas = resultado.Dados?.Notificacoes?.Where(n => !n.Lida).ToList() ?? new List<PlantaCoreAPI.Application.DTOs.Notificacao.NotificacaoDTOSaida>();
        return Ok(ResponseHelper.Padrao(true, naoLidas));
    }

    [HttpPut("{notificacaoId:guid}/marcar-como-lida")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarcarComoLida(Guid notificacaoId)
    {
        var resultado = await _servicioNotificacao.MarcarComoLidaAsync(notificacaoId);
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao<object>(true, null, meta: new { mensagem = resultado.Mensagem }));
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
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao<object>(true, null, meta: new { mensagem = resultado.Mensagem }));
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
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao<object>(true, null, meta: new { mensagem = resultado.Mensagem }));
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
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao<object>(true, null, meta: new { mensagem = resultado.Mensagem }));
    }

    [HttpGet("configuracoes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterConfiguracoes()
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();
        var resultado = await _servicioNotificacao.ObterConfiguracoesAsync(usuarioId);
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao(true, resultado.Dados));
    }

    [HttpPut("configuracoes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AtualizarConfiguracoes([FromBody] PlantaCoreAPI.Application.DTOs.Notificacao.ConfiguracoesNotificacaoDTOEntrada entrada)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();
        var resultado = await _servicioNotificacao.AtualizarConfiguracoesAsync(usuarioId, entrada);
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao<object>(true, null, meta: new { mensagem = resultado.Mensagem }));
    }
}
