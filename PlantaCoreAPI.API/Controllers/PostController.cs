using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantaCoreAPI.Application.DTOs.Post;
using PlantaCoreAPI.Application.DTOs.Comentario;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Application.Comuns.RateLimit;
using System.Security.Claims;
using PlantaCoreAPI.API.Utils;

namespace PlantaCoreAPI.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Tags("Post")]
public class PostController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly IRateLimitService _rateLimitService;

    public PostController(IPostService postService, IRateLimitService rateLimitService)
    {
        _postService = postService;
        _rateLimitService = rateLimitService;
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CriarPost([FromBody] CriarPostDTOEntrada entrada)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _postService.CriarPostAsync(usuarioId, entrada);
        if (!resultado.Sucesso)
            return BadRequest(resultado);

        return CreatedAtAction(nameof(ObterPost), new { postId = resultado.Dados!.Id }, resultado);
    }

    [Authorize]
    [HttpPut("{postId:guid}")]
    public async Task<IActionResult> AtualizarPost(Guid postId, [FromBody] AtualizarPostDTOEntrada entrada)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _postService.AtualizarPostAsync(usuarioId, postId, entrada);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [Authorize]
    [HttpDelete("{postId:guid}")]
    public async Task<IActionResult> ExcluirPost(Guid postId)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _postService.ExcluirPostAsync(usuarioId, postId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpGet("{postId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPost(Guid postId)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var usuarioId = usuarioIdClaim != null && Guid.TryParse(usuarioIdClaim, out var id) ? id : Guid.Empty;

        var resultado = await _postService.ObterPostAsync(postId, usuarioId);
        return resultado.Sucesso ? Ok(resultado) : NotFound(resultado);
    }

    [HttpGet("feed")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterFeed([FromQuery] int pagina = 1, [FromQuery] int tamanho = 10, [FromQuery] string? ordenarPor = null)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _postService.ObterFeedAsync(usuarioId, pagina, tamanho);
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));

        return Ok(ResponseHelper.Padrao(true, resultado.Dados));
    }

    [HttpPost("{postId:guid}/curtir")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CurtirPost(Guid postId)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var rateKey = $"curtir:{usuarioId}";
        if (_rateLimitService.IsLimited(rateKey, 30, TimeSpan.FromMinutes(1)))
            return StatusCode(429, new { sucesso = false, mensagem = "Limite de curtidas atingido. Tente novamente em instantes." });

        var resultado = await _postService.CurtirPostAsync(usuarioId, postId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpDelete("{postId:guid}/curtida")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoverCurtida(Guid postId)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _postService.RemoverCurtidaAsync(usuarioId, postId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPost("comentario")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CriarComentario([FromBody] CriarComentarioDTOEntrada entrada)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var rateKey = $"comentar:{usuarioId}";
        if (_rateLimitService.IsLimited(rateKey, 20, TimeSpan.FromMinutes(1)))
            return StatusCode(429, new { sucesso = false, mensagem = "Limite de comentários atingido. Tente novamente em instantes." });

        var resultado = await _postService.CriarComentarioAsync(usuarioId, entrada);
        return !resultado.Sucesso ? BadRequest(resultado) : Ok(resultado);
    }

    [HttpPut("comentario/{comentarioId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AtualizarComentario(Guid comentarioId, [FromBody] AtualizarComentarioDTOEntrada entrada)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _postService.AtualizarComentarioAsync(usuarioId, comentarioId, entrada);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpDelete("comentario/{comentarioId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExcluirComentario(Guid comentarioId)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _postService.ExcluirComentarioAsync(usuarioId, comentarioId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpGet("{postId:guid}/comentarios")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListarComentariosPost(Guid postId, [FromQuery] int pagina = 1, [FromQuery] int tamanho = 20, [FromQuery] string? ordenar = null)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var usuarioAutenticadoId = usuarioIdClaim != null && Guid.TryParse(usuarioIdClaim, out var id) ? id : Guid.Empty;

        var resultado = await _postService.ListarComentariosPostAsync(postId, usuarioAutenticadoId, pagina, tamanho, ordenar);
        return resultado.Sucesso ? Ok(resultado) : NotFound(resultado);
    }

    [HttpGet("usuario/{usuarioId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListarPostsUsuario(Guid usuarioId, [FromQuery] int pagina = 1, [FromQuery] int tamanho = 10, [FromQuery] string? ordenarPor = null)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var usuarioAutenticadoId = usuarioIdClaim != null && Guid.TryParse(usuarioIdClaim, out var id) ? id : Guid.Empty;

        var resultado = await _postService.ListarPostsUsuarioAsync(usuarioId, usuarioAutenticadoId, pagina, tamanho, ordenarPor);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpGet("explorar")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Explorar([FromQuery] int pagina = 1, [FromQuery] int tamanho = 10, [FromQuery] string? ordenarPor = null)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var usuarioAutenticadoId = usuarioIdClaim != null && Guid.TryParse(usuarioIdClaim, out var id) ? id : Guid.Empty;

        var resultado = await _postService.ObterExploradorAsync(usuarioAutenticadoId, pagina, tamanho, ordenarPor);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpGet("usuario/{usuarioId:guid}/curtidos")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListarPostsCurtidos(Guid usuarioId)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var usuarioAutenticadoId = usuarioIdClaim != null && Guid.TryParse(usuarioIdClaim, out var id) ? id : Guid.Empty;

        var resultado = await _postService.ListarPostsCurtidosAsync(usuarioId, usuarioAutenticadoId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPost("comentario/{comentarioId:guid}/curtir")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CurtirComentario(Guid comentarioId)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _postService.CurtirComentarioAsync(usuarioId, comentarioId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpDelete("comentario/{comentarioId:guid}/curtida")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoverCurtidaComentario(Guid comentarioId)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _postService.RemoverCurtidaComentarioAsync(usuarioId, comentarioId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpDelete("{postId:guid}/comentario/{comentarioId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExcluirComentarioComoDonoPost(Guid postId, Guid comentarioId)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _postService.ExcluirComentarioComoDonoPostAsync(usuarioId, comentarioId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpGet("trending")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> TrendingPosts([FromQuery] int quantidade = 10)
    {
        var resultado = await _postService.ObterTrendingPostsAsync(quantidade);
        return Ok(resultado);
    }

    [HttpPost("{postId:guid}/salvar")]
    [Authorize]
    public async Task<IActionResult> SalvarPost(Guid postId)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _postService.SalvarPostAsync(usuarioId, postId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpDelete("{postId:guid}/salvar")]
    [Authorize]
    public async Task<IActionResult> RemoverPostSalvo(Guid postId)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _postService.RemoverPostSalvoAsync(usuarioId, postId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPost("{postId:guid}/compartilhar")]
    [Authorize]
    public async Task<IActionResult> CompartilharPost(Guid postId)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _postService.CompartilharPostAsync(usuarioId, postId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPost("{postId:guid}/visualizar")]
    [Authorize]
    public async Task<IActionResult> VisualizarPost(Guid postId)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _postService.VisualizarPostAsync(usuarioId, postId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPost("comentario/{comentarioId:guid}/responder")]
    [Authorize]
    public async Task<IActionResult> ResponderComentario(Guid comentarioId, [FromBody] ResponderComentarioDTOEntrada entrada)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _postService.ResponderComentarioAsync(usuarioId, comentarioId, entrada.Conteudo);
        return !resultado.Sucesso ? BadRequest(resultado) : Ok(resultado);
    }

    [HttpGet("comunidade/{comunidadeId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> ListarPostsComunidade(Guid comunidadeId, [FromQuery] int pagina = 1, [FromQuery] int tamanho = 10, [FromQuery] string? ordenarPor = null)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var usuarioId = usuarioIdClaim != null && Guid.TryParse(usuarioIdClaim, out var id) ? id : Guid.Empty;

        var resultado = await _postService.ListarPostsComunidadeAsync(comunidadeId, usuarioId, pagina, tamanho, ordenarPor);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }
}
