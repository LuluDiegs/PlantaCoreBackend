using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PlantaCoreAPI.Application.Comuns.RateLimit;
using PlantaCoreAPI.Application.DTOs.Usuario;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.API.Utils;

using System.Security.Claims;

namespace PlantaCoreAPI.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
[Tags("Usuario")]
public class UsuarioController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IRateLimitService _rateLimitService;
    private readonly IPostService _postService;
    private readonly IComunidadeService _servicoComunidade;
    public UsuarioController(IUserService userService, IRateLimitService rateLimitService, IPostService postService, IComunidadeService servicoComunidade)
    {
        _userService = userService;
        _rateLimitService = rateLimitService;
        _postService = postService;
        _servicoComunidade = servicoComunidade;
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
        var resultado = await _userService.ObterPerfilAsync(usuarioId);
        if (!resultado.Sucesso)
            return NotFound(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao(true, resultado.Dados));
    }

    [HttpGet("perfil-publico/{usuarioId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPerfilPublico(Guid usuarioId)
    {
        var usuarioAutenticadoIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var usuarioAutenticadoId = usuarioAutenticadoIdClaim != null && Guid.TryParse(usuarioAutenticadoIdClaim, out var id) ? id : Guid.Empty;
        var resultado = await _userService.ObterPerfilPublicoAsync(usuarioId, usuarioAutenticadoId);
        return resultado.Sucesso ? Ok(resultado) : NotFound(resultado);
    }

    [HttpPut("nome")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AtualizarNome([FromBody] AtualizarNomeDTOEntrada entrada)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();
        var resultado = await _userService.AtualizarNomeAsync(usuarioId, entrada.NovoNome);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPut("biografia")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AtualizarBiografia([FromBody] AtualizarBiografiaDTOEntrada entrada)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();
        var perfilEntrada = new AtualizarPerfilDTOEntrada { Biografia = entrada.Biografia };
        var resultado = await _userService.AtualizarPerfilAsync(usuarioId, perfilEntrada);
        return resultado.Sucesso
            ? Ok(new { sucesso = true, mensagem = "Biografia atualizada com sucesso" })
            : BadRequest(resultado);
    }

    [HttpPut("privacidade")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AlterarPrivacidade([FromBody] AlterarPrivacidadePerfilDTOEntrada entrada)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();
        var resultado = await _userService.AlterarPrivacidadePerfilAsync(usuarioId, entrada.Privado);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPost("foto-perfil")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AtualizarFotoPerfil(IFormFile foto)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();
        if (foto == null || foto.Length == 0)
            return BadRequest(new { sucesso = false, mensagem = "Nenhuma foto enviada" });
        if (foto.Length > 5 * 1024 * 1024)
            return BadRequest(new { sucesso = false, mensagem = "Arquivo excede o tamanho máximo de 5MB" });
        using var stream = foto.OpenReadStream();
        var resultado = await _userService.AtualizarFotoPerfilAsync(usuarioId, stream, foto.FileName);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpDelete("conta")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExcluirConta()
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();
        var resultado = await _userService.ExcluirContaAsync(usuarioId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPost("reativar/solicitar")]
    [AllowAnonymous]
    public async Task<IActionResult> SolicitarReativacao([FromBody] SolicitarReativacaoDTOEntrada entrada)
    {
        var resultado = await _userService.SolicitarReativacaoAsync(entrada.Email);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPost("reativar/confirmar")]
    [AllowAnonymous]
    public async Task<IActionResult> ReativarComToken([FromBody] ReativarComTokenDTOEntrada entrada)
    {
        var resultado = await _userService.ReativarComTokenAsync(entrada.Email, entrada.Token, entrada.NovaSenha);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPost("reativar/verificar-token")]
    [AllowAnonymous]
    public async Task<IActionResult> VerificarTokenReativacao([FromBody] VerificarTokenReativacaoDTOEntrada entrada)
    {
        var resultado = await _userService.VerificarTokenReativacaoAsync(entrada.Email, entrada.Token);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPost("seguir/{usuarioIdParaSeguir:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Seguir(Guid usuarioIdParaSeguir)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();
        var rateKey = $"seguir:{usuarioId}";
        if (_rateLimitService.IsLimited(rateKey, 10, TimeSpan.FromMinutes(1)))
            return StatusCode(429, new { sucesso = false, mensagem = "Limite de requisições atingido. Tente novamente em instantes." });
        var resultado = await _userService.SegurUserAsync(usuarioId, usuarioIdParaSeguir);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpDelete("seguir/{usuarioIdParaDeseguir:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Deseguir(Guid usuarioIdParaDeseguir)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();
        var resultado = await _userService.DesSeguirUserAsync(usuarioId, usuarioIdParaDeseguir);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpGet("meus-seguidores")]
    public async Task<IActionResult> MeusSeguidores([FromQuery] int pagina = 1, [FromQuery] int tamanho = 20)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();
        var resultado = await _userService.ListarSeguidoresAsync(usuarioId, pagina, tamanho);
        if (!resultado.Sucesso || resultado.Dados == null)
            return NotFound(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        var meta = new
        {
            pagina = resultado.Dados.Pagina,
            tamanho = resultado.Dados.TamanhoPagina,
            total = resultado.Dados.Total,
            totalPaginas = (int)Math.Ceiling((double)resultado.Dados.Total / resultado.Dados.TamanhoPagina)
        };
        return Ok(ResponseHelper.Padrao(true, resultado.Dados.Itens, meta));
    }

    [HttpGet("meu-seguindo")]
    public async Task<IActionResult> MeuSeguindo([FromQuery] int pagina = 1, [FromQuery] int tamanho = 20)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();
        var resultado = await _userService.ListarSeguindoAsync(usuarioId, pagina, tamanho);
        if (!resultado.Sucesso || resultado.Dados == null)
            return NotFound(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        var meta = new
        {
            pagina = resultado.Dados.Pagina,
            tamanho = resultado.Dados.TamanhoPagina,
            total = resultado.Dados.Total,
            totalPaginas = (int)Math.Ceiling((double)resultado.Dados.Total / resultado.Dados.TamanhoPagina)
        };
        return Ok(ResponseHelper.Padrao(true, resultado.Dados.Itens, meta));
    }

    [HttpGet("{usuarioId:guid}/seguidores")]
    [AllowAnonymous]
    public async Task<IActionResult> ListarSeguidores(Guid usuarioId, [FromQuery] int pagina = 1, [FromQuery] int tamanho = 20)
    {
        var resultado = await _userService.ListarSeguidoresAsync(usuarioId, pagina, tamanho);
        if (!resultado.Sucesso || resultado.Dados == null)
            return NotFound(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        var meta = new
        {
            pagina = resultado.Dados.Pagina,
            tamanho = resultado.Dados.TamanhoPagina,
            total = resultado.Dados.Total,
            totalPaginas = (int)Math.Ceiling((double)resultado.Dados.Total / resultado.Dados.TamanhoPagina)
        };
        return Ok(ResponseHelper.Padrao(true, resultado.Dados.Itens, meta));
    }

    [HttpGet("{usuarioId:guid}/seguindo")]
    [AllowAnonymous]
    public async Task<IActionResult> ListarSeguindo(Guid usuarioId, [FromQuery] int pagina = 1, [FromQuery] int tamanho = 20)
    {
        var resultado = await _userService.ListarSeguindoAsync(usuarioId, pagina, tamanho);
        if (!resultado.Sucesso || resultado.Dados == null)
            return NotFound(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        var meta = new
        {
            pagina = resultado.Dados.Pagina,
            tamanho = resultado.Dados.TamanhoPagina,
            total = resultado.Dados.Total,
            totalPaginas = (int)Math.Ceiling((double)resultado.Dados.Total / resultado.Dados.TamanhoPagina)
        };
        return Ok(ResponseHelper.Padrao(true, resultado.Dados.Itens, meta));
    }

    [HttpGet("{usuarioId:guid}/seguidores/lista")]
    public async Task<IActionResult> ListarSeguidoresLista(Guid usuarioId, [FromQuery] int pagina = 1, [FromQuery] int tamanho = 20)
    {
        var usuarioAutenticadoIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioAutenticadoIdClaim, out var usuarioAutenticadoId))
            return Unauthorized();
        var resultado = await _userService.ListarSeguidoresListaAsync(usuarioId, usuarioAutenticadoId, pagina, tamanho);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpGet("{usuarioId:guid}/seguindo/lista")]
    public async Task<IActionResult> ListarSeguindoLista(Guid usuarioId, [FromQuery] int pagina = 1, [FromQuery] int tamanho = 20)
    {
        var usuarioAutenticadoIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioAutenticadoIdClaim, out var usuarioAutenticadoId))
            return Unauthorized();
        var resultado = await _userService.ListarSeguindoListaAsync(usuarioId, usuarioAutenticadoId, pagina, tamanho);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPost("solicitacao-seguir/{alvoId:guid}")]
    public async Task<IActionResult> EnviarSolicitacaoSeguir(Guid alvoId)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();
        var rateKey = $"solicitacao-seguir:{usuarioId}";
        if (_rateLimitService.IsLimited(rateKey, 10, TimeSpan.FromMinutes(1)))
            return StatusCode(429, new { sucesso = false, mensagem = "Limite de solicitações atingido. Tente novamente em instantes." });
        var resultado = await _userService.EnviarSolicitacaoSeguirAsync(usuarioId, alvoId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpGet("solicitacoes-seguir")]
    public async Task<IActionResult> ListarSolicitacoesPendentes()
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();
        var resultado = await _userService.ListarSolicitacoesPendentesAsync(usuarioId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPost("solicitacoes-seguir/{solicitacaoId:guid}/aceitar")]
    public async Task<IActionResult> AceitarSolicitacao(Guid solicitacaoId)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();
        var resultado = await _userService.AceitarSolicitacaoSeguirAsync(usuarioId, solicitacaoId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPost("solicitacoes-seguir/{solicitacaoId:guid}/rejeitar")]
    public async Task<IActionResult> RejeitarSolicitacao(Guid solicitacaoId)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();
        var resultado = await _userService.RejeitarSolicitacaoSeguirAsync(usuarioId, solicitacaoId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpGet("{usuarioId:guid}/plantas")]
    [AllowAnonymous]
    public async Task<IActionResult> ListarPlantasUsuario(Guid usuarioId, [FromQuery] int pagina = 1, [FromQuery] int tamanho = 12)
    {
        var usuarioAutenticadoIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var usuarioAutenticadoId = usuarioAutenticadoIdClaim != null && Guid.TryParse(usuarioAutenticadoIdClaim, out var id) ? id : Guid.Empty;
        var resultado = await _userService.ListarPlantasUsuarioAsync(usuarioId, usuarioAutenticadoId, pagina, tamanho);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpGet("{usuarioId:guid}/posts")]
    [AllowAnonymous]
    public async Task<IActionResult> ListarPostsUsuario(Guid usuarioId, [FromQuery] int pagina = 1, [FromQuery] int tamanho = 10)
    {
        var usuarioAutenticadoIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var usuarioAutenticadoId = usuarioAutenticadoIdClaim != null && Guid.TryParse(usuarioAutenticadoIdClaim, out var id) ? id : Guid.Empty;
        var resultado = await _userService.ListarPostsPerfilAsync(usuarioId, usuarioAutenticadoId, pagina, tamanho);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpGet("{usuarioId:guid}/relacao")]
    public async Task<IActionResult> ObterRelacaoUsuario(Guid usuarioId)
    {
        var usuarioAutenticadoIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioAutenticadoIdClaim, out var usuarioAutenticadoId))
            return Unauthorized();
        var resultado = await _userService.ObterRelacaoUsuarioAsync(usuarioAutenticadoId, usuarioId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpGet("sugestoes")]
    public async Task<IActionResult> SugestoesParaSeguir([FromQuery] int quantidade = 10)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();
        var resultado = await _userService.SugerirUsuariosParaSeguirAsync(usuarioId, quantidade);
        return Ok(resultado);
    }

    [HttpGet("posts-salvos")]
    public async Task<IActionResult> ListarPostsSalvos()
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();
        var resultado = await _postService.ListarPostsSalvosAsync(usuarioId);
        return Ok(resultado);
    }

    [HttpGet("{usuarioId:guid}/comunidades")]
    [AllowAnonymous]
    public async Task<IActionResult> ListarComunidadesDoUsuario(Guid usuarioId, [FromQuery] int pagina = 1, [FromQuery] int tamanho = 10)
    {
        var resultado = await _servicoComunidade.ListarComunidadesDoUsuarioAsync(usuarioId, pagina, tamanho);
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
}
