using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantaCoreAPI.Application.DTOs.Usuario;
using PlantaCoreAPI.Application.Interfaces;
using System.Security.Claims;

namespace PlantaCoreAPI.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class UsuarioController : ControllerBase
{
    private readonly IUserService _servicioUsuario;

    public UsuarioController(IUserService servicioUsuario)
    {
        _servicioUsuario = servicioUsuario;
    }

    [HttpGet("perfil")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPerfil()
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _servicioUsuario.ObterPerfilAsync(usuarioId);

        if (!resultado.Sucesso)
            return NotFound(resultado);

        return Ok(resultado);
    }

    [HttpGet("perfil-publico/{usuarioId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPerfilPublico(Guid usuarioId)
    {
        var usuarioAutenticadoIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var usuarioAutenticadoId = usuarioAutenticadoIdClaim != null && Guid.TryParse(usuarioAutenticadoIdClaim, out var id) ? id : Guid.Empty;

        var resultado = await _servicioUsuario.ObterPerfilPublicoAsync(usuarioId, usuarioAutenticadoId);

        if (!resultado.Sucesso)
            return NotFound(resultado);

        return Ok(resultado);
    }

    [HttpPut("nome")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AtualizarNome([FromBody] AtualizarNomeDTOEntrada entrada)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _servicioUsuario.AtualizarNomeAsync(usuarioId, entrada.NovoNome);

        if (!resultado.Sucesso)
            return BadRequest(resultado);

        return Ok(resultado);
    }

    [HttpPut("biografia")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AtualizarBiografia([FromBody] AtualizarBiografiaDTOEntrada entrada)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var perfilEntrada = new AtualizarPerfilDTOEntrada { Biografia = entrada.Biografia };
        var resultado = await _servicioUsuario.AtualizarPerfilAsync(usuarioId, perfilEntrada);

        if (!resultado.Sucesso)
            return BadRequest(resultado);

        return Ok(new { sucesso = true, mensagem = "Biografia atualizada com sucesso" });
    }

    [HttpPost("foto-perfil")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AtualizarFotoPerfil(IFormFile foto)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        if (foto == null || foto.Length == 0)
            return BadRequest(new { sucesso = false, mensagem = "Nenhuma foto enviada" });

        using var stream = foto.OpenReadStream();
        var resultado = await _servicioUsuario.AtualizarFotoPerfilAsync(usuarioId, stream, foto.FileName);

        if (!resultado.Sucesso)
            return BadRequest(resultado);

        return Ok(resultado);
    }

    [HttpDelete("conta")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExcluirConta()
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _servicioUsuario.ExcluirContaAsync(usuarioId);

        if (!resultado.Sucesso)
            return BadRequest(resultado);

        return Ok(resultado);
    }

    [HttpPost("reativar/solicitar")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SolicitarReativacao([FromBody] SolicitarReativacaoDTOEntrada entrada)
    {
        var resultado = await _servicioUsuario.SolicitarReativacaoAsync(entrada.Email);

        if (!resultado.Sucesso)
            return BadRequest(resultado);

        return Ok(resultado);
    }

    [HttpPost("reativar/confirmar")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReativarComToken([FromBody] ReativarComTokenDTOEntrada entrada)
    {
        var resultado = await _servicioUsuario.ReativarComTokenAsync(entrada.Email, entrada.Token, entrada.NovaSenha);

        if (!resultado.Sucesso)
            return BadRequest(resultado);

        return Ok(resultado);
    }

    [HttpPost("reativar/verificar-token")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerificarTokenReativacao([FromBody] VerificarTokenReativacaoDTOEntrada entrada)
    {
        var resultado = await _servicioUsuario.VerificarTokenReativacaoAsync(entrada.Email, entrada.Token);

        if (!resultado.Sucesso)
            return BadRequest(resultado);

        return Ok(resultado);
    }

    [HttpPost("reativar/resetar-senha")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetarSenhaSemToken([FromBody] ResetarSenhaSemTokenDTOEntrada entrada)
    {
        var resultado = await _servicioUsuario.ResetarSenhaSemTokenAsync(entrada.Email, entrada.NovaSenha);

        if (!resultado.Sucesso)
            return BadRequest(resultado);

        return Ok(resultado);
    }

    [HttpPost("seguir/{usuarioIdParaSeguir:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Seguir(Guid usuarioIdParaSeguir)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _servicioUsuario.SegurUserAsync(usuarioId, usuarioIdParaSeguir);

        if (!resultado.Sucesso)
            return BadRequest(resultado);

        return Ok(resultado);
    }

    [HttpDelete("seguir/{usuarioIdParaDeseguir:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Deseguir(Guid usuarioIdParaDeseguir)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _servicioUsuario.DesSeguirUserAsync(usuarioId, usuarioIdParaDeseguir);

        if (!resultado.Sucesso)
            return BadRequest(resultado);

        return Ok(resultado);
    }

    [HttpGet("{usuarioId:guid}/seguidores")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListarSeguidores(Guid usuarioId, [FromQuery] int pagina = 1, [FromQuery] int tamanho = 20)
    {
        var resultado = await _servicioUsuario.ListarSeguidoresAsync(usuarioId, pagina, tamanho);

        if (!resultado.Sucesso)
            return NotFound(resultado);

        return Ok(resultado);
    }

    [HttpGet("{usuarioId:guid}/seguindo")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListarSeguindo(Guid usuarioId, [FromQuery] int pagina = 1, [FromQuery] int tamanho = 20)
    {
        var resultado = await _servicioUsuario.ListarSeguindoAsync(usuarioId, pagina, tamanho);

        if (!resultado.Sucesso)
            return NotFound(resultado);

        return Ok(resultado);
    }
}
