using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantaCoreAPI.API.Utils;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Domain.Interfaces;

namespace PlantaCoreAPI.API.Controllers;

[ApiController]
[Route("api/v1/internal")]
[ApiExplorerSettings(IgnoreApi = true)]
[Tags("Internal")]
[Authorize(Roles = "Admin")]
public class InternalController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IPostService _postService;
    private readonly IRepositorioUsuario _repoUsuario;
    private readonly IRepositorioPost _repoPost;
    private readonly IRepositorioComunidade _repoComunidade;

    public InternalController(IUserService userService, IPostService postService, IRepositorioUsuario repoUsuario, IRepositorioPost repoPost, IRepositorioComunidade repoComunidade)
    {
        _userService = userService;
        _postService = postService;
        _repoUsuario = repoUsuario;
        _repoPost = repoPost;
        _repoComunidade = repoComunidade;
    }

    [HttpPost("recalcular-contadores")]
    public async Task<IActionResult> RecalcularContadores()
    {
        var usuarios = await _repoUsuario.ObterTodosAsync();
        var posts = await _repoPost.ObterTodosAsync();
        var comunidades = await _repoComunidade.ObterTodosAsync();
        int totalCurtidas = posts.Sum(p => p.Curtidas.Count);
        int totalComentarios = posts.Sum(p => p.Comentarios.Count);
        int totalSeguidores = usuarios.Sum(u => u.Seguidores.Count);
        int totalMembros = comunidades.Sum(c => c.Membros.Count);
        int totalPostsComunidades = comunidades.Sum(c => c.Posts.Count);
        var meta = new { totalCurtidas, totalComentarios, totalSeguidores, totalMembros, totalPostsComunidades };
        return Ok(ResponseHelper.Padrao(true, meta));
    }

    [HttpPost("reconstruir-feed")]
    public IActionResult ReconstruirFeed()
    {
        // Simulação: em sistemas reais, dispararia eventos ou jobs
        return Ok(ResponseHelper.Padrao<object>(true, null, meta: new { mensagem = "Reconstrução do feed disparada (simulado)" }));
    }
}
