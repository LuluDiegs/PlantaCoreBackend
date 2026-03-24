using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.DTOs;
using PlantaCoreAPI.Application.DTOs.Evento;
using PlantaCoreAPI.Application.DTOs.Usuario;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Application.Services;
using PlantaCoreAPI.API.Utils;

namespace PlantaCoreAPI.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class EventoController : ControllerBase
{
    private readonly EventoService _servicoEvento;
    private readonly IRepositorioEvento _repositorioEvento;

    public EventoController(EventoService servicoEvento, IRepositorioEvento repositorioEvento)
    {
        _servicoEvento = servicoEvento;
        _repositorioEvento = repositorioEvento;
    }

    [HttpGet]
    public async Task<IActionResult> ObterEventos()
    {
        Resultado<List<EventoDTOSaida>> resultado = await _servicoEvento.ObterEventosAsync();
        return Ok(ResponseHelper.Padrao(true, resultado.Dados));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterEvento([FromRoute] Guid id)
    {
        Resultado<EventoDTOSaida> resultado = await _servicoEvento.ObterEventoPorIdAsync(id);
        if (!resultado.Sucesso)
            return NotFound(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao(true, resultado.Dados));
    }

    [HttpPost]
    public async Task<IActionResult> AdicionarEvento([FromBody] CriarEventoDTO eventoDTO)
    {
        Claim? claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim is null) return Unauthorized();
        string valor = claim.Value;
        bool sucesso = Guid.TryParse(valor, out Guid anfitriaoId);
        if (!sucesso) return Unauthorized();
        Resultado resultado = await _servicoEvento.AdicionarEventoAsync(eventoDTO, anfitriaoId);
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao<object>(true, null, meta: new { mensagem = resultado.Mensagem }));
    }

    [HttpPut("marcar-participacao")]
    public async Task<IActionResult> MarcarParticipacaoEvento(Guid eventoId)
    {
        Claim? claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim is null) return Unauthorized();
        string valor = claim.Value;
        bool sucesso = Guid.TryParse(valor, out Guid usuarioId);
        if (!sucesso) return Unauthorized();
        Resultado resultado = await _servicoEvento.MarcarParticipacaoEvento(eventoId, usuarioId);
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao<object>(true, null, meta: new { mensagem = resultado.Mensagem }));
    }

    [HttpPut("desmarcar-participacao")]
    public async Task<IActionResult> DesmarcarParticipacaoEvento(Guid eventoId)
    {
        Claim? claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim is null) return Unauthorized();
        string valor = claim.Value;
        bool sucesso = Guid.TryParse(valor, out Guid usuarioId);
        if (!sucesso) return Unauthorized();
        Resultado resultado = await _servicoEvento.DesmarcarParticipacaoEvento(eventoId, usuarioId);
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao<object>(true, null, meta: new { mensagem = resultado.Mensagem }));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> AtualizarEvento(
        [FromRoute] Guid id, 
        [FromBody] AtualizarEventoDTO eventoDTO)
    {
        Claim? claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim is null) return Unauthorized();
        string valor = claim.Value;
        bool sucesso = Guid.TryParse(valor, out Guid usuarioId);
        if (!sucesso) return Unauthorized();
        Resultado resultado = await _servicoEvento.AtualizarEvento(id, eventoDTO, usuarioId);
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao<object>(true, null, meta: new { mensagem = resultado.Mensagem }));
    }

    [HttpDelete("{eventoId:guid}")]
    public async Task<IActionResult> RemoverEvento([FromRoute] Guid eventoId)
    {
        Claim? claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim is null) return Unauthorized();
        string valor = claim.Value;
        bool sucesso = Guid.TryParse(valor, out Guid usuarioId);
        if (!sucesso) return Unauthorized();
        Resultado resultado = await _servicoEvento.RemoverEvento(eventoId, usuarioId);
        if (!resultado.Sucesso)
            return BadRequest(ResponseHelper.Padrao<object>(false, null, null, new[] { resultado.Mensagem ?? "Erro" }));
        return Ok(ResponseHelper.Padrao<object>(true, null, meta: new { mensagem = resultado.Mensagem }));
    }

    [HttpGet("{eventoId:guid}/participantes")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarParticipantes(Guid eventoId)
    {
        var participantes = await _repositorioEvento.ListarParticipantesAsync(eventoId);
        return Ok(ResponseHelper.Padrao(true, participantes));
    }
}