using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using PlantaCoreAPI.API.Options;
using PlantaCoreAPI.Application.Interfaces;

using System.Security.Claims;

namespace PlantaCoreAPI.API.Controllers;

[ApiController]
[Route("api/v1/armazenamento")]
[Tags("Armazenamento")]
[Produces("application/json")]
public class ArmazenamentoController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;
    private readonly string _adminChaveSecreta;
    public ArmazenamentoController(IFileStorageService fileStorageService, IOptions<AdminOptions> adminOptions)
    {
        _fileStorageService = fileStorageService;
        _adminChaveSecreta = adminOptions.Value.ChaveSecreta;
    }

    [HttpGet("fotos/listar")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListarTodasAsFotos([FromHeader(Name = "X-Admin-Key")] string? adminKey)
    {
        if (string.IsNullOrWhiteSpace(_adminChaveSecreta) || adminKey != _adminChaveSecreta)
            return StatusCode(403, new { sucesso = false, mensagem = "Acesso negado. Chave de administrador inválida." });
        var urls = await _fileStorageService.ListarTodosArquivosAsync();
        return Ok(new { sucesso = true, total = urls.Count, urls });
    }

    [HttpPost("foto/upload")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FazerUploadFoto(IFormFile foto)
    {
        if (foto == null || foto.Length == 0)
            return BadRequest(new { sucesso = false, mensagem = "Nenhuma foto enviada" });
        if (foto.Length > 5 * 1024 * 1024)
            return BadRequest(new { sucesso = false, mensagem = "Arquivo excede o tamanho máximo de 5MB" });
        if (!foto.ContentType.StartsWith("image/"))
            return BadRequest(new { sucesso = false, mensagem = "Arquivo deve ser uma imagem" });
        using var stream = new MemoryStream();
        await foto.CopyToAsync(stream);
        var urlPublica = await _fileStorageService.FazerUploadAsync(stream.ToArray(), foto.FileName, foto.ContentType);
        return Ok(new { sucesso = true, url = urlPublica });
    }

    [HttpPost("foto-perfil/upload")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> FazerUploadFotoPerfil(IFormFile foto)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();
        if (foto == null || foto.Length == 0)
            return BadRequest(new { sucesso = false, mensagem = "Nenhuma foto enviada" });
        if (foto.Length > 5 * 1024 * 1024)
            return BadRequest(new { sucesso = false, mensagem = "Arquivo excede o tamanho máximo de 5MB" });
        if (!foto.ContentType.StartsWith("image/"))
            return BadRequest(new { sucesso = false, mensagem = "Arquivo deve ser uma imagem" });
        using var stream = new MemoryStream();
        await foto.CopyToAsync(stream);
        var urlPublica = await _fileStorageService.FazerUploadFotoPerfilAsync(stream.ToArray(), foto.FileName, usuarioId);
        return Ok(new { sucesso = true, url = urlPublica });
    }

    [HttpPost("foto-planta/upload")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> FazerUploadFotoPlanta(IFormFile foto)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();
        if (foto == null || foto.Length == 0)
            return BadRequest(new { sucesso = false, mensagem = "Nenhuma foto enviada" });
        if (foto.Length > 5 * 1024 * 1024)
            return BadRequest(new { sucesso = false, mensagem = "Arquivo excede o tamanho máximo de 5MB" });
        if (!foto.ContentType.StartsWith("image/"))
            return BadRequest(new { sucesso = false, mensagem = "Arquivo deve ser uma imagem" });
        using var stream = new MemoryStream();
        await foto.CopyToAsync(stream);
        var urlPublica = await _fileStorageService.FazerUploadFotoPlantaAsync(stream.ToArray(), foto.FileName, usuarioId);
        return Ok(new { sucesso = true, url = urlPublica });
    }

    [HttpDelete("foto")]
    [Authorize]
    public async Task<IActionResult> ExcluirFoto([FromQuery] string nomeArquivo)
    {
        if (string.IsNullOrWhiteSpace(nomeArquivo))
            return BadRequest(new { sucesso = false, mensagem = "nomeArquivo é obrigatório" });
        var sucesso = await _fileStorageService.ExcluirArquivoAsync(nomeArquivo);
        return sucesso
            ? Ok(new { sucesso = true, mensagem = "Arquivo excluído com sucesso" })
            : BadRequest(new { sucesso = false, mensagem = "Não foi possível excluir o arquivo" });
    }

    [HttpDelete("fotos/todas")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExcluirTodasAsFotos([FromHeader(Name = "X-Admin-Key")] string? adminKey)
    {
        if (string.IsNullOrWhiteSpace(_adminChaveSecreta) || adminKey != _adminChaveSecreta)
            return StatusCode(403, new { sucesso = false, mensagem = "Acesso negado. Chave de administrador inválida." });
        var total = await _fileStorageService.ExcluirTodosArquivosAsync();
        return Ok(new { sucesso = true, mensagem = $"{total} arquivo(s) excluído(s) com sucesso", total });
    }
}
