using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.DTOs.Evento;
using PlantaCoreAPI.Application.DTOs.Usuario;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Domain.Entities;
using PlantaCoreAPI.Domain.Exceptions;
using PlantaCoreAPI.Domain.Interfaces;

namespace PlantaCoreAPI.Application.Services;

public class EventoService : IEventoService
{
    private readonly IRepositorioEvento _repositorioEvento;
    public EventoService(IRepositorioEvento repositorioEvento)
    {
        _repositorioEvento = repositorioEvento;
    }

    private static EventoDTOSaida MapearEvento(Evento e) => new()
    {
        Id = e.Id,
        Titulo = e.Titulo,
        Descricao = e.Descricao,
        Localizacao = e.Localizacao,
        DataInicio = e.DataInicio,
        AnfitriaoId = e.AnfitriaoId,
        ParticipantesIds = e.Participantes.Select(p => p.UsuarioId).ToList(),
        TotalParticipantes = e.Participantes.Count
    };
    public async Task<Resultado<List<EventoDTOSaida>>> ObterEventosAsync()
    {
        var eventos = await _repositorioEvento.ObterTodosComParticipantesAsync();
        return Resultado<List<EventoDTOSaida>>.Ok(eventos.Select(MapearEvento).ToList());
    }

    public async Task<Resultado<EventoDTOSaida>> ObterEventoPorIdAsync(Guid id)
    {
        var evento = await _repositorioEvento.ObterPorIdAsync(id);
        if (evento is null)
            return Resultado<EventoDTOSaida>.Erro("Evento não encontrado");
        return Resultado<EventoDTOSaida>.Ok(MapearEvento(evento));
    }

    public async Task<Resultado<Guid>> AdicionarEventoAsync(CriarEventoDTO eventoDTO, Guid anfitriaoId)
    {
        var eventoIgual = await _repositorioEvento.ObterPorTituloAsync(eventoDTO.Titulo);
        if (eventoIgual is not null)
            return Resultado<Guid>.Erro("Já existe um evento com este título");
        Evento evento;
        try
        {
            evento = Evento.Criar(anfitriaoId, eventoDTO.Titulo, eventoDTO.Descricao, eventoDTO.Localizacao, eventoDTO.DataInicio);
        }

        catch (DomainException ex)
        {
            return Resultado<Guid>.Erro(ex.Message);
        }

        evento.Participantes.Add(new EventoParticipante { EventoId = evento.Id, UsuarioId = anfitriaoId });
        await _repositorioEvento.AdicionarAsync(evento);
        await _repositorioEvento.SalvarMudancasAsync();
        return Resultado<Guid>.Ok(evento.Id, "Evento criado com sucesso");
    }

    public async Task<Resultado> MarcarParticipacaoEvento(Guid eventoId, Guid usuarioId)
    {
        var evento = await _repositorioEvento.ObterPorIdAsync(eventoId);
        if (evento is null)
            return Resultado.Erro("Evento não encontrado");
        if (evento.Participantes.Any(ep => ep.UsuarioId == usuarioId))
            return Resultado.Erro("Você já participa deste evento");
        evento.Participantes.Add(new EventoParticipante { EventoId = eventoId, UsuarioId = usuarioId });
        await _repositorioEvento.SalvarMudancasAsync();
        return Resultado.Ok("Participação no evento marcada com sucesso");
    }

    public async Task<Resultado> DesmarcarParticipacaoEvento(Guid eventoId, Guid usuarioId)
    {
        var evento = await _repositorioEvento.ObterPorIdAsync(eventoId);
        if (evento is null)
            return Resultado.Erro("Evento não encontrado");
        if (evento.AnfitriaoId == usuarioId)
            return Resultado.Erro("Anfitrião não pode sair do evento");
        var participante = evento.Participantes.FirstOrDefault(p => p.UsuarioId == usuarioId);
        if (participante is null)
            return Resultado.Erro("Você não participa deste evento");
        evento.Participantes.Remove(participante);
        await _repositorioEvento.SalvarMudancasAsync();
        return Resultado.Ok("Participação do evento desmarcada com sucesso");
    }

    public async Task<Resultado> AtualizarEvento(Guid id, AtualizarEventoDTO eventoDTO, Guid usuarioId)
    {
        var evento = await _repositorioEvento.ObterPorIdAsync(id);
        if (evento is null)
            return Resultado.Erro("Evento não encontrado");
        if (evento.AnfitriaoId != usuarioId)
            return Resultado.Erro("Apenas o anfitrião pode atualizar informações do evento");
        var eventoIgual = await _repositorioEvento.ObterPorTituloAsync(eventoDTO.Titulo);
        if (eventoIgual is not null && eventoIgual.Id != id)
            return Resultado.Erro("Já existe um evento com este título");
        Resultado resultado;
        try
        {
            evento.Atualizar(eventoDTO.Titulo, eventoDTO.Descricao, eventoDTO.Localizacao, eventoDTO.DataInicio);
            resultado = Resultado.Ok("Evento atualizado com sucesso");
        }

        catch (DomainException ex)
        {
            return Resultado.Erro(ex.Message);
        }

        await _repositorioEvento.AtualizarAsync(evento);
        await _repositorioEvento.SalvarMudancasAsync();
        return resultado;
    }

    public async Task<Resultado> RemoverEvento(Guid eventoId, Guid usuarioId)
    {
        var evento = await _repositorioEvento.ObterPorIdAsync(eventoId);
        if (evento is null)
            return Resultado.Erro("Evento não encontrado");
        if (evento.AnfitriaoId != usuarioId)
            return Resultado.Erro("Apenas o anfitrião pode excluir o evento");
        await _repositorioEvento.RemoverAsync(evento);
        await _repositorioEvento.SalvarMudancasAsync();
        return Resultado.Ok("Evento removido com sucesso");
    }

    public async Task<Resultado<IEnumerable<UsuarioListaDTOSaida>>> ListarParticipantesAsync(Guid eventoId)
    {
        var evento = await _repositorioEvento.ObterPorIdAsync(eventoId);
        if (evento is null)
            return Resultado<IEnumerable<UsuarioListaDTOSaida>>.Erro("Evento não encontrado");
        var participantes = await _repositorioEvento.ListarParticipantesAsync(eventoId);
        var dtos = participantes.Select(u => new UsuarioListaDTOSaida
        {
            Id = u.Id,
            Nome = u.Nome,
            Seguindo = false
        });
        return Resultado<IEnumerable<UsuarioListaDTOSaida>>.Ok(dtos);
    }
}
