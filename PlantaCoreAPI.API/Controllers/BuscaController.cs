using Microsoft.AspNetCore.Mvc;
using PlantaCoreAPI.Application.Interfaces;

namespace PlantaCoreAPI.API.Controllers;

[ApiController]
[Route("api/v1/busca")]
[Tags("Busca")]
public class BuscaController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IPostService _postService;
    private readonly IComunidadeService _comunidadeService;
    private readonly IPlantService _plantService;

    public BuscaController(IUserService userService, IPostService postService, IComunidadeService comunidadeService, IPlantService plantService)
    {
        _userService = userService;
        _postService = postService;
        _comunidadeService = comunidadeService;
        _plantService = plantService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Buscar([FromQuery] string termo)
    {
        if (string.IsNullOrWhiteSpace(termo))
            return Ok(new { usuarios = new object[0], posts = new object[0], comunidades = new object[0], plantas = new object[0] });

        var usuarios = await _userService.BuscarUsuariosPorNomeAsync(termo);
        var posts = await _postService.BuscarPostsPorPalavraChaveAsync(termo);
        var comunidades = await _comunidadeService.BuscarComunidadesAsync(termo, Guid.Empty);
        var plantas = await _plantService.BuscarPlantasPorNomeAsync(termo);

        return Ok(new
        {
            usuarios,
            posts,
            comunidades = comunidades.Dados ?? Enumerable.Empty<object>(),
            plantas
        });
    }
}
