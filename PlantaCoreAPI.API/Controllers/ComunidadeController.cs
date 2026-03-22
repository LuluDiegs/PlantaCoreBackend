using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantaCoreAPI.Application.DTOs.Comunidade;
using PlantaCoreAPI.Application.Interfaces;
using System.Security.Claims;

namespace PlantaCoreAPI.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class ComunidadeController : ControllerBase
{
    private readonly IComunidadeService _servicioComunidade;

    public ComunidadeController(IComunidadeService servicioComunidade)
    {
        _servicioComunidade = servicioComunidade;
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CriarComunidade([FromBody] CriarComunidadeDTOEntrada entrada)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _servicioComunidade.CriarComunidadeAsync(usuarioId, entrada);

        if (!resultado.Sucesso)
            return BadRequest(resultado);

        return CreatedAtAction(nameof(ObterComunidade), new { comunidadeId = resultado.Dados!.Id }, resultado);
    }

    [HttpPut("{comunidadeId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AtualizarComunidade(Guid comunidadeId, [FromBody] AtualizarComunidadeDTOEntrada entrada)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _servicioComunidade.AtualizarComunidadeAsync(usuarioId, comunidadeId, entrada);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpGet("{comunidadeId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterComunidade(Guid comunidadeId)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var usuarioId = usuarioIdClaim != null && Guid.TryParse(usuarioIdClaim, out var id) ? id : Guid.Empty;

        var resultado = await _servicioComunidade.ObterComunidadeAsync(comunidadeId, usuarioId);
        return resultado.Sucesso ? Ok(resultado) : NotFound(resultado);
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarComunidades([FromQuery] int pagina = 1, [FromQuery] int tamanho = 10)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var usuarioId = usuarioIdClaim != null && Guid.TryParse(usuarioIdClaim, out var id) ? id : Guid.Empty;

        var resultado = await _servicioComunidade.ListarComunidadesAsync(pagina, tamanho, usuarioId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpGet("buscar")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BuscarComunidades([FromQuery] string termo)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var usuarioId = usuarioIdClaim != null && Guid.TryParse(usuarioIdClaim, out var id) ? id : Guid.Empty;

        var resultado = await _servicioComunidade.BuscarComunidadesAsync(termo, usuarioId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpGet("minhas")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListarMinhasComunidades([FromQuery] int pagina = 1, [FromQuery] int tamanho = 10)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _servicioComunidade.ListarComunidadesDoUsuarioAsync(usuarioId, pagina, tamanho);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpGet("usuario/{usuarioId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarComunidadesDoUsuario(Guid usuarioId, [FromQuery] int pagina = 1, [FromQuery] int tamanho = 10)
    {
        var resultado = await _servicioComunidade.ListarComunidadesDoUsuarioAsync(usuarioId, pagina, tamanho);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPost("{comunidadeId:guid}/entrar")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> EntrarNaComunidade(Guid comunidadeId)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _servicioComunidade.EntrarNaComunidadeAsync(usuarioId, comunidadeId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpDelete("{comunidadeId:guid}/sair")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SairDaComunidade(Guid comunidadeId)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _servicioComunidade.SairDaComunidadeAsync(usuarioId, comunidadeId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpGet("{comunidadeId:guid}/posts")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterPostsComunidade(Guid comunidadeId, [FromQuery] int pagina = 1, [FromQuery] int tamanho = 10)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _servicioComunidade.ObterPostsComunidadeAsync(comunidadeId, usuarioId, pagina, tamanho);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpDelete("{comunidadeId:guid}/expulsar/{usuarioId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExpulsarUsuario(Guid comunidadeId, Guid usuarioId)
    {
        var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(adminIdClaim, out var adminId))
            return Unauthorized();

        var resultado = await _servicioComunidade.ExpulsarUsuarioAsync(adminId, comunidadeId, usuarioId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpDelete("{comunidadeId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExcluirComunidade(Guid comunidadeId)
    {
        var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(adminIdClaim, out var adminId))
            return Unauthorized();

        var resultado = await _servicioComunidade.ExcluirComunidadeAsync(adminId, comunidadeId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPut("{comunidadeId:guid}/transferir-admin")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> TransferirAdmin(Guid comunidadeId, [FromBody] TransferirAdminDTOEntrada entrada)
    {
        var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(adminIdClaim, out var adminId))
            return Unauthorized();

        var resultado = await _servicioComunidade.TransferirAdminAsync(adminId, comunidadeId, entrada.NovoAdminId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }
}
