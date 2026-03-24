using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantaCoreAPI.Application.DTOs.Auth;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.API.Utils;
using System.Security.Claims;

namespace PlantaCoreAPI.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AutenticacaoController : ControllerBase
{
    private readonly IAuthenticationService _servicioAutenticacao;

    public AutenticacaoController(IAuthenticationService servicioAutenticacao)
    {
        _servicioAutenticacao = servicioAutenticacao;
    }

    [HttpPost("registrar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Registrar([FromBody] RegistroDTOEntrada entrada)
    {
        var resultado = await _servicioAutenticacao.RegistrarAsync(entrada);
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao<object>(true, null, meta: new { mensagem = resultado.Mensagem }));
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDTOEntrada entrada)
    {
        var resultado = await _servicioAutenticacao.LoginAsync(entrada);
        if (!resultado.Sucesso)
            return Unauthorized(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao(true, resultado.Dados));
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDTOEntrada entrada)
    {
        var resultado = await _servicioAutenticacao.RefreshTokenAsync(entrada.TokenRefresh);
        if (!resultado.Sucesso)
            return Unauthorized(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao(true, resultado.Dados));
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _servicioAutenticacao.LogoutAsync(usuarioId);
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao<object>(true, null, meta: new { mensagem = resultado.Mensagem }));
    }

    [HttpPost("confirmar-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmarEmail([FromBody] ConfirmarEmailDTOEntrada entrada)
    {
        var resultado = await _servicioAutenticacao.ConfirmarEmailAsync(entrada);
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao<object>(true, null, meta: new { mensagem = resultado.Mensagem }));
    }

    [HttpPost("resetar-senha")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetarSenha([FromBody] ResetarSenhaDTOEntrada entrada)
    {
        var resultado = await _servicioAutenticacao.ResetarSenhaAsync(entrada);
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao<object>(true, null, meta: new { mensagem = resultado.Mensagem }));
    }

    [HttpPost("nova-senha")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> NovaSenha([FromBody] NovaSenhaDTOEntrada entrada)
    {
        var resultado = await _servicioAutenticacao.NovaSenhaAsync(entrada);
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao<object>(true, null, meta: new { mensagem = resultado.Mensagem }));
    }

    [HttpPost("trocar-senha")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> TrocarSenha([FromBody] TrocarSenhaDTOEntrada entrada)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _servicioAutenticacao.TrocarSenhaAsync(usuarioId, entrada);
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao<object>(true, null, meta: new { mensagem = resultado.Mensagem }));
    }

    [HttpPost("reenviar-confirmacao")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReenviarConfirmacaoEmail([FromBody] ResetarSenhaDTOEntrada entrada)
    {
        var resultado = await _servicioAutenticacao.ReenviarConfirmacaoEmailAsync(entrada.Email);
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao<object>(true, null, meta: new { mensagem = resultado.Mensagem }));
    }
}
