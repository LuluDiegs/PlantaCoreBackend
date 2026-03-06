using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantaCoreAPI.Application.DTOs.Planta;
using PlantaCoreAPI.Application.DTOs.Identificacao;
using PlantaCoreAPI.Application.Interfaces;
using System.Security.Claims;

namespace PlantaCoreAPI.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class PlantaController : ControllerBase
{
    private readonly IPlantService _servicioPlanta;
    private readonly IFileStorageService _fileStorageService;

    public PlantaController(IPlantService servicioPlanta, IFileStorageService fileStorageService)
    {
        _servicioPlanta = servicioPlanta;
        _fileStorageService = fileStorageService;
    }

    [HttpPost("identificar")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> IdentificarPlanta(IFormFile foto)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        if (foto == null || foto.Length == 0)
            return BadRequest(new { sucesso = false, mensagem = "Nenhuma foto enviada" });

        if (!foto.ContentType.StartsWith("image/"))
            return BadRequest(new { sucesso = false, mensagem = "Arquivo deve ser uma imagem" });

        string? caminhoTemp = null;
        string? urlFoto = null;

        try
        {
            caminhoTemp = Path.Combine(Path.GetTempPath(), $"planta_{Guid.NewGuid()}_{foto.FileName}");
            using (var fileStream = System.IO.File.Create(caminhoTemp))
                await foto.CopyToAsync(fileStream);

            try
            {
                var bytes = await System.IO.File.ReadAllBytesAsync(caminhoTemp);
                urlFoto = await _fileStorageService.FazerUploadFotoPlantaAsync(bytes, foto.FileName, usuarioId);
            }
            catch { }

            var resultado = await _servicioPlanta.IdentificarPlantaAsync(usuarioId, new IdentificacaoDTOEntrada
            {
                CaminhoTemp = caminhoTemp,
                UrlImagem = urlFoto
            });

            return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
        }
        catch (Exception ex)
        {
            return BadRequest(new { sucesso = false, mensagem = $"Erro ao processar imagem: {ex.Message}" });
        }
        finally
        {
            if (caminhoTemp != null && System.IO.File.Exists(caminhoTemp))
                try { System.IO.File.Delete(caminhoTemp); } catch { }
        }
    }

    [HttpPost("buscar")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BuscarPlanta([FromBody] BuscaPlantaDTOEntrada entrada)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        if (entrada.Pagina < 0)
            entrada.Pagina = 0;

        var resultado = await _servicioPlanta.BuscarPlantasTrefleAsync(entrada.NomePlanta, entrada.Pagina);

        if (!resultado.Sucesso)
            return NotFound(new { sucesso = false, mensagem = resultado.Mensagem, dados = new { plantas = new List<object>() } });

        return Ok(new { sucesso = true, dados = resultado.Dados });
    }

    [HttpPost("buscar/adicionar")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> BuscarEAdicionarPlanta([FromBody] AdicionarPlantaTrefleDTO entrada)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        if (entrada?.PlantaTrefleId <= 0)
            return BadRequest(new { sucesso = false, mensagem = "plantaTrefleId obrigatório e deve ser maior que 0" });

        var resultado = await _servicioPlanta.AdicionarPlantaDoTrefleAsync(usuarioId, entrada.PlantaTrefleId, entrada.NomeCientifico, entrada.UrlImagem);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpGet("minhas-plantas")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListarMinhasPlantas([FromQuery] int pagina = 1, [FromQuery] int tamanho = 10)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _servicioPlanta.ListarPlantasUsuarioPaginadoAsync(usuarioId, pagina, tamanho);
        return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpGet("{plantaId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPlanta(Guid plantaId)
    {
        var resultado = await _servicioPlanta.ObterPlantaAsync(plantaId);
        return resultado.Sucesso ? Ok(resultado) : NotFound(resultado);
    }

    [HttpDelete("{plantaId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExcluirPlanta(Guid plantaId)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _servicioPlanta.ExcluirPlantaAsync(plantaId, usuarioId);
        return resultado.Sucesso ? Ok(new { sucesso = true, mensagem = "Planta excluída com sucesso" }) : BadRequest(resultado);
    }

    [HttpPost("{plantaId:guid}/gerar-lembrete-cuidado")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GerarLembreteCuidado(Guid plantaId, [FromServices] IPlantCareReminderService servicoLembrete)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        await servicoLembrete.GerarLembreteCuidadoAsync(plantaId);
        return Ok(new { sucesso = true, mensagem = "Lembrete de cuidado gerado com sucesso" });
    }
}
