using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PlantaCoreAPI.API.Utils;
using PlantaCoreAPI.Application.DTOs.Comunidade;
using PlantaCoreAPI.Application.DTOs.Usuario;
using PlantaCoreAPI.Application.Interfaces;

using System.Security.Claims;

namespace PlantaCoreAPI.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Tags("Comunidade")]
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
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao(true, resultado.Dados));
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
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao(true, resultado.Dados));
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
        if (!resultado.Sucesso)
            return NotFound(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao(true, resultado.Dados));
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarComunidades([FromQuery] int pagina = 1, [FromQuery] int tamanho = 10)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var usuarioId = usuarioIdClaim != null && Guid.TryParse(usuarioIdClaim, out var id) ? id : Guid.Empty;
        var resultado = await _servicioComunidade.ListarComunidadesAsync(pagina, tamanho, usuarioId);
        if (!resultado.Sucesso || resultado.Dados == null)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        var meta = new
        {
            pagina = resultado.Dados.Pagina,
            tamanho = resultado.Dados.TamanhoPagina,
            total = resultado.Dados.Total,
            totalPaginas = (int)Math.Ceiling((double)resultado.Dados.Total / resultado.Dados.TamanhoPagina)
        };
        return Ok(ResponseHelper.Padrao(true, resultado.Dados.Itens, meta));
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
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao(true, resultado.Dados));
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
        if (!resultado.Sucesso || resultado.Dados == null)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        var meta = new
        {
            pagina = resultado.Dados.Pagina,
            tamanho = resultado.Dados.TamanhoPagina,
            total = resultado.Dados.Total,
            totalPaginas = (int)Math.Ceiling((double)resultado.Dados.Total / resultado.Dados.TamanhoPagina)
        };
        return Ok(ResponseHelper.Padrao(true, resultado.Dados.Itens, meta));
    }

    [HttpGet("usuario/{usuarioId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarComunidadesDoUsuario(Guid usuarioId, [FromQuery] int pagina = 1, [FromQuery] int tamanho = 10)
    {
        var resultado = await _servicioComunidade.ListarComunidadesDoUsuarioAsync(usuarioId, pagina, tamanho);
        if (!resultado.Sucesso || resultado.Dados == null)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        var meta = new
        {
            pagina = resultado.Dados.Pagina,
            tamanho = resultado.Dados.TamanhoPagina,
            total = resultado.Dados.Total,
            totalPaginas = (int)Math.Ceiling((double)resultado.Dados.Total / resultado.Dados.TamanhoPagina)
        };
        return Ok(ResponseHelper.Padrao(true, resultado.Dados.Itens, meta));
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
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao<object>(true, null, meta: new { mensagem = resultado.Mensagem }));
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
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao<object>(true, null, meta: new { mensagem = resultado.Mensagem }));
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
        if (!resultado.Sucesso || resultado.Dados == null)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        var meta = new
        {
            pagina = resultado.Dados.Pagina,
            tamanho = resultado.Dados.TamanhoPagina,
            total = resultado.Dados.Total,
            totalPaginas = (int)Math.Ceiling((double)resultado.Dados.Total / resultado.Dados.TamanhoPagina)
        };
        return Ok(ResponseHelper.Padrao(true, resultado.Dados.Itens, meta));
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
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao<object>(true, null, meta: new { mensagem = resultado.Mensagem }));
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
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao<object>(true, null, meta: new { mensagem = resultado.Mensagem }));
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
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao<object>(true, null, meta: new { mensagem = resultado.Mensagem }));
    }

    [HttpGet("recomendadas")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ComunidadesRecomendadas([FromQuery] int quantidade = 10)
    {
        var recomendadas = await _servicioComunidade.ListarRecomendadasAsync(quantidade);
        return Ok(ResponseHelper.Padrao(true, recomendadas));
    }

    [HttpGet("{comunidadeId:guid}/membros")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarMembros(Guid comunidadeId)
    {
        var resultado = await _servicioComunidade.ListarMembrosAsync(comunidadeId);
        return Ok(ResponseHelper.Padrao(true, resultado));
    }

    [HttpGet("{comunidadeId:guid}/admins")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarAdmins(Guid comunidadeId)
    {
        var resultado = await _servicioComunidade.ListarAdminsAsync(comunidadeId);
        return Ok(ResponseHelper.Padrao(true, resultado));
    }

    [HttpGet("{comunidadeId:guid}/sou-membro")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SouMembro(Guid comunidadeId)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();
        var ehMembro = await _servicioComunidade.SouMembroAsync(comunidadeId, usuarioId);
        return Ok(ResponseHelper.Padrao(true, new { ehMembro }));
    }

    [HttpPost("{comunidadeId:guid}/solicitar-entrada")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SolicitarEntrada(Guid comunidadeId)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();
        var resultado = await _servicioComunidade.SolicitarEntradaAsync(comunidadeId, usuarioId);
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao<object>(true, null, meta: new { mensagem = resultado.Mensagem }));
    }

    [HttpGet("{comunidadeId:guid}/solicitacoes")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListarSolicitacoes(Guid comunidadeId)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();
        var comunidade = await _servicioComunidade.ObterComunidadeAsync(comunidadeId, usuarioId);
        if (!comunidade.Sucesso || !(comunidade.Dados?.UsuarioEhAdmin ?? false))
            return StatusCode(403, ResponseHelper.Padrao<object>(false, null, null, new[] { "Apenas administradores podem ver solicitações" }));
        var solicitacoes = await _servicioComunidade.ListarSolicitacoesAsync(comunidadeId);
        return Ok(ResponseHelper.Padrao(true, solicitacoes));
    }

    [HttpPut("{comunidadeId:guid}/solicitacoes/{usuarioId:guid}/aprovar")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AprovarSolicitacao(Guid comunidadeId, Guid usuarioId)
    {
        var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(adminIdClaim, out var adminId))
            return Unauthorized();
        var resultado = await _servicioComunidade.AprovarSolicitacaoAsync(comunidadeId, usuarioId, adminId);
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao<object>(true, null, meta: new { mensagem = resultado.Mensagem }));
    }
}
