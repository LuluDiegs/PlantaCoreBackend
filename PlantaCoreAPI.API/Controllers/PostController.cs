using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantaCoreAPI.Application.DTOs.Post;
using PlantaCoreAPI.Application.DTOs.Comentario;
using PlantaCoreAPI.Application.Interfaces;
using System.Security.Claims;

namespace PlantaCoreAPI.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class PostController : ControllerBase
{
    private readonly IPostService _servicioPost;

    public PostController(IPostService servicioPost)
    {
        _servicioPost = servicioPost;
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

        var resultado = await _servicioPost.CriarPostAsync(usuarioId, entrada);

        if (!resultado.Sucesso)
            return BadRequest(resultado);

        return CreatedAtAction(nameof(ObterPost), new { postId = resultado.Dados!.Id }, resultado);
    }

    [HttpPut("{postId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AtualizarPost(Guid postId, [FromBody] AtualizarPostDTOEntrada entrada)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _servicioPost.AtualizarPostAsync(usuarioId, postId, entrada);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpDelete("{postId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExcluirPost(Guid postId)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _servicioPost.ExcluirPostAsync(usuarioId, postId);
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

        var resultado = await _servicioPost.ObterPostAsync(postId, usuarioId);
        return resultado.Sucesso ? Ok(resultado) : NotFound(resultado);
    }

    [HttpGet("feed")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterFeed([FromQuery] int pagina = 1, [FromQuery] int tamanho = 10)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _servicioPost.ObterFeedAsync(usuarioId, pagina, tamanho);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
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

        var resultado = await _servicioPost.CurtirPostAsync(usuarioId, postId);
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

        var resultado = await _servicioPost.RemoverCurtidaAsync(usuarioId, postId);
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

        var resultado = await _servicioPost.CriarComentarioAsync(usuarioId, entrada);

        if (!resultado.Sucesso)
            return BadRequest(resultado);

        return CreatedAtAction(nameof(CriarComentario), resultado);
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

        var resultado = await _servicioPost.AtualizarComentarioAsync(usuarioId, comentarioId, entrada);
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

        var resultado = await _servicioPost.ExcluirComentarioAsync(usuarioId, comentarioId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpGet("{postId:guid}/comentarios")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListarComentariosPost(Guid postId, [FromQuery] int pagina = 1, [FromQuery] int tamanho = 20)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var usuarioAutenticadoId = usuarioIdClaim != null && Guid.TryParse(usuarioIdClaim, out var id) ? id : Guid.Empty;

        var resultado = await _servicioPost.ListarComentariosPostAsync(postId, usuarioAutenticadoId, pagina, tamanho);
        return resultado.Sucesso ? Ok(resultado) : NotFound(resultado);
    }

    [HttpGet("usuario/{usuarioId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListarPostsUsuario(Guid usuarioId, [FromQuery] int pagina = 1, [FromQuery] int tamanho = 10)
    {
        var usuarioAutenticadoIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var usuarioAutenticadoId = usuarioAutenticadoIdClaim != null && Guid.TryParse(usuarioAutenticadoIdClaim, out var id) ? id : Guid.Empty;

        var resultado = await _servicioPost.ListarPostsUsuarioAsync(usuarioId, usuarioAutenticadoId, pagina, tamanho);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpGet("explorar")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Explorar([FromQuery] int pagina = 1, [FromQuery] int tamanho = 10)
    {
        var usuarioAutenticadoIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var usuarioAutenticadoId = usuarioAutenticadoIdClaim != null && Guid.TryParse(usuarioAutenticadoIdClaim, out var id) ? id : Guid.Empty;

        var resultado = await _servicioPost.ObterExploradorAsync(usuarioAutenticadoId, pagina, tamanho);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpGet("usuario/{usuarioId:guid}/curtidos")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListarPostsCurtidos(Guid usuarioId)
    {
        var usuarioAutenticadoIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var usuarioAutenticadoId = usuarioAutenticadoIdClaim != null && Guid.TryParse(usuarioAutenticadoIdClaim, out var id) ? id : Guid.Empty;

        var resultado = await _servicioPost.ListarPostsCurtidosAsync(usuarioId, usuarioAutenticadoId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPost("comentario/{comentarioId:guid}/curtir")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CurtirComentario(Guid comentarioId)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _servicioPost.CurtirComentarioAsync(usuarioId, comentarioId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpDelete("comentario/{comentarioId:guid}/curtida")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoverCurtidaComentario(Guid comentarioId)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _servicioPost.RemoverCurtidaComentarioAsync(usuarioId, comentarioId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpDelete("{postId:guid}/comentario/{comentarioId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExcluirComentarioComoDonoPost(Guid postId, Guid comentarioId)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _servicioPost.ExcluirComentarioComoDonoPostAsync(usuarioId, comentarioId);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }
}
