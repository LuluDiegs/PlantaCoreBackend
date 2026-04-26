using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantaCoreAPI.API.Utils;
using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.DTOs.Evento;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Domain.Entities;

namespace PlantaCoreAPI.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
[Tags("Loja")]
public class LojaController : ControllerBase
{
    private readonly ILojaService _servicoLoja;

    public LojaController(ILojaService servicoLoja)
    {
        _servicoLoja = servicoLoja;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterTodosAsync(CancellationToken cancellationToken)
    {
        Resultado<IEnumerable<Loja>> resultado = await _servicoLoja.ObterTodosAsync(cancellationToken);

        if (!resultado.Sucesso)
        {
            IEnumerable<string> mensagensErro = [resultado.Mensagem ?? "Erro"];
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, mensagensErro));
        }

        return Ok(ResponseHelper.Padrao(true, resultado.Dados));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterPorIdAsync(CancellationToken cancellationToken, Guid id)
    {
        Resultado<Loja> resultado = await _servicoLoja.ObterPorIdAsync(id, cancellationToken);

        if (!resultado.Sucesso)
        {
            IEnumerable<string> mensagensErro = [resultado.Mensagem ?? "Erro"];
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, mensagensErro));
        }

        return Ok(ResponseHelper.Padrao(true, resultado.Dados));
    }

    [HttpGet("minhas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterPorUsuarioAsync(CancellationToken cancellationToken)
    {
        Claim? claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim is null) return Unauthorized();

        string? valor = claim.Value;
        if (valor is null) return Unauthorized();

        bool sucesso = Guid.TryParse(valor, out Guid usuarioId);
        if (!sucesso) return Unauthorized();

        Resultado<IEnumerable<Loja>> resultado = await _servicoLoja.ObterPorUsuarioAsync(usuarioId, cancellationToken);

        if (!resultado.Sucesso)
        {
            IEnumerable<string> mensagensErro = [resultado.Mensagem ?? "Erro"];
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, mensagensErro));
        }

        return Ok(ResponseHelper.Padrao(true, resultado.Dados));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AdicionarAsync(
        CancellationToken cancellationToken,
        [FromBody] CriarAtualizarLojaDTO entrada)
    {
        Claim? claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim is null) return Unauthorized();

        string? valor = claim.Value;
        if (valor is null) return Unauthorized();

        bool sucesso = Guid.TryParse(valor, out Guid usuarioId);
        if (!sucesso) return Unauthorized();

        Loja loja = new()
        {
            Id = Guid.NewGuid(),
            Nome = entrada.Nome,
            Descricao = entrada.Descricao,
            Email = entrada.Email,
            Telefone = entrada.Telefone,
            ImagemUrl = entrada.ImagemUrl,
            SomenteOnline = entrada.SomenteOnline,
            Cidade = entrada.Cidade,
            Estado = entrada.Estado,
            Endereco = entrada.Endereco,
            UsuarioId = usuarioId
        };

        Resultado resultado = await _servicoLoja.AdicionarAsync(loja, cancellationToken);

        if (!resultado.Sucesso)
        {
            IEnumerable<string> mensagensErro = [resultado.Mensagem ?? "Erro"];
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, mensagensErro));
        }

        var metadados = new { mensagem = resultado.Mensagem };
        return Ok(ResponseHelper.Padrao<object>(true, null, metadados));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AtualizarAsync(
        CancellationToken cancellationToken,
        [FromRoute] Guid id,
        [FromBody] CriarAtualizarLojaDTO entrada)
    {
        Claim? claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim is null) return Unauthorized();

        string? valor = claim.Value;
        if (valor is null) return Unauthorized();

        bool sucesso = Guid.TryParse(valor, out Guid usuarioId);
        if (!sucesso) return Unauthorized();

        Loja loja = new()
        {
            Id = id,
            Nome = entrada.Nome,
            Descricao = entrada.Descricao,
            Email = entrada.Email,
            Telefone = entrada.Telefone,
            ImagemUrl = entrada.ImagemUrl,
            SomenteOnline = entrada.SomenteOnline,
            Cidade = entrada.Cidade,
            Estado = entrada.Estado,
            Endereco = entrada.Endereco,
            UsuarioId = usuarioId
        };

        Resultado resultado = await _servicoLoja.AtualizarAsync(loja, cancellationToken);

        if (!resultado.Sucesso)
        {
            IEnumerable<string> mensagensErro = [resultado.Mensagem ?? "Erro"];
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, mensagensErro));
        }

        var metadados = new { mensagem = resultado.Mensagem };
        return Ok(ResponseHelper.Padrao<object>(true, null, metadados));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoverAsync(
        CancellationToken cancellationToken,
        [FromRoute] Guid id)
    {
        Claim? claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim is null) return Unauthorized();

        string? valor = claim.Value;
        if (valor is null) return Unauthorized();

        bool sucesso = Guid.TryParse(valor, out Guid usuarioId);
        if (!sucesso) return Unauthorized();

        Resultado resultado = await _servicoLoja.RemoverAsync(id, usuarioId, cancellationToken);

        if (!resultado.Sucesso)
        {
            IEnumerable<string> mensagensErro = [resultado.Mensagem ?? "Erro"];
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, mensagensErro));
        }

        var metadados = new { mensagem = resultado.Mensagem };
        return Ok(ResponseHelper.Padrao<object>(true, null, metadados));
    }
}