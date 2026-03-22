using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantaCoreAPI.Application.DTOs.Planta;
using PlantaCoreAPI.Application.DTOs.Identificacao;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Application.DTOs.Post;
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
    public async Task<IActionResult> Identificar([FromForm] IdentificarEPostarDTO entrada, [FromServices] IPostService postService)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        if (entrada.Foto == null || entrada.Foto.Length == 0)
            return BadRequest(new { sucesso = false, mensagem = "Nenhuma foto enviada" });

        if (!entrada.Foto.ContentType.StartsWith("image/"))
            return BadRequest(new { sucesso = false, mensagem = "Arquivo deve ser uma imagem" });

        string? caminhoTemp = null;
        string? urlFoto = null;

        try
        {
            caminhoTemp = Path.Combine(Path.GetTempPath(), $"planta_{Guid.NewGuid()}_{entrada.Foto.FileName}");
            using (var fileStream = System.IO.File.Create(caminhoTemp))
                await entrada.Foto.CopyToAsync(fileStream);

            try
            {
                var bytes = await System.IO.File.ReadAllBytesAsync(caminhoTemp);
                urlFoto = await _fileStorageService.FazerUploadFotoPlantaAsync(bytes, entrada.Foto.FileName, usuarioId);
            }
            catch { }

            var resultadoIdentificacao = await _servicioPlanta.IdentificarPlantaAsync(usuarioId, new IdentificacaoDTOEntrada
            {
                CaminhoTemp = caminhoTemp,
                UrlImagem = urlFoto
            });

            if (!resultadoIdentificacao.Sucesso)
                return BadRequest(resultadoIdentificacao);

            var plantaIdentificada = resultadoIdentificacao.Dados;

            // Verifica se o comentário foi fornecido, caso contrário, gera um padrão
            var comentario = string.IsNullOrWhiteSpace(entrada.Comentario)
                ? $"Identificação: {plantaIdentificada.NomeCientifico ?? plantaIdentificada.NomeComum ?? "Planta"}{(string.IsNullOrWhiteSpace(plantaIdentificada.NomeComum) ? "" : $" ({plantaIdentificada.NomeComum})")}"
                : entrada.Comentario;

            if (entrada.CriarPostagem)
            {
                var resultadoPostagem = await postService.CriarPostAsync(usuarioId, new CriarPostDTOEntrada
                {
                    PlantaId = plantaIdentificada.Id,
                    Conteudo = comentario,
                    ComunidadeId = null
                });

                if (!resultadoPostagem.Sucesso)
                    return BadRequest(resultadoPostagem);

                return Ok(new
                {
                    sucesso = true,
                    mensagem = "Planta identificada e postagem criada com sucesso",
                    planta = plantaIdentificada,
                    postagem = resultadoPostagem.Dados
                });
            }

            return Ok(new
            {
                sucesso = true,
                mensagem = "Planta identificada com sucesso",
                planta = plantaIdentificada
            });
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

    [HttpGet("minhas-plantas/buscar")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> BuscarMinhasPlantas([FromQuery] string termo, [FromQuery] int pagina = 1, [FromQuery] int tamanho = 10)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var resultado = await _servicioPlanta.BuscarPlantasUsuarioAsync(usuarioId, termo, pagina, tamanho);
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
