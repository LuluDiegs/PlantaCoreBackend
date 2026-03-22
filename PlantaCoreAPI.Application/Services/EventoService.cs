using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.DTOs;
using PlantaCoreAPI.Application.DTOs.Evento;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Domain.Entities;

namespace PlantaCoreAPI.Application.Services;

public class EventoService
{
    private readonly IRepositorioEvento _repositorioEvento;

    public EventoService(IRepositorioEvento repositorioEvento)
    {
        _repositorioEvento = repositorioEvento;
    }

    public async Task<Resultado<List<EventoDTOSaida>>> ObterEventosAsync()
    {
        List<Evento> eventos = await _repositorioEvento.ObterTodosAsync();

        List<EventoDTOSaida> eventosDTO = eventos.Select(e => new EventoDTOSaida
        {
            Id = e.Id,
            Titulo = e.Titulo,
            Descricao = e.Descricao,
            Localizacao = e.Localizacao,
            DataInicio = e.DataInicio,
            AnfitriaoId = e.AnfitriaoId,
            ParticipantesIds = e.Participantes.Select(p => p.UsuarioId).ToList()
        }).ToList();

        return Resultado<List<EventoDTOSaida>>.Ok(eventosDTO);
    }

    public async Task<Resultado<EventoDTOSaida>> ObterEventoPorIdAsync(Guid id)
    {
        Evento? evento = await _repositorioEvento.ObterPorIdAsync(id);

        if (evento is null)
            return Resultado<EventoDTOSaida>.Erro($"Evento com ID {id} não encontrado");

        EventoDTOSaida eventoDTO = new EventoDTOSaida
        {
            Id = evento.Id,
            Titulo = evento.Titulo,
            Descricao = evento.Descricao,
            Localizacao = evento.Localizacao,
            DataInicio = evento.DataInicio,
            AnfitriaoId = evento.AnfitriaoId,
            ParticipantesIds = evento.Participantes.Select(p => p.UsuarioId).ToList()
        };

        return Resultado<EventoDTOSaida>.Ok(eventoDTO);
    }

    public async Task<Resultado> AdicionarEventoAsync(CriarEventoDTO eventoDTO, Guid anfitriaoId)
    {
        Evento? eventoIgual = await _repositorioEvento.ObterPorTituloAsync(eventoDTO.Titulo);

        if (eventoIgual is not null)
            return Resultado.Erro($"Evento com titulo {eventoDTO.Titulo} já existe");

        Evento evento = new Evento
        {
            Id = Guid.NewGuid(),
            Titulo = eventoDTO.Titulo,
            Descricao = eventoDTO.Descricao,
            Localizacao = eventoDTO.Localizacao,
            DataInicio = eventoDTO.DataInicio,
            AnfitriaoId = anfitriaoId,
        };

        EventoParticipante participante = new EventoParticipante
        {
            EventoId = evento.Id,
            UsuarioId = anfitriaoId
        };

        evento.Participantes.Add(participante);

        await _repositorioEvento.AdicionarAsync(evento);
        await _repositorioEvento.SalvarMudancasAsync();

        return Resultado.Ok("Evento criado com sucesso");
    }

    public async Task<Resultado> MarcarParticipacaoEvento(Guid eventoId, Guid usuarioId)
    {
        Evento? evento = await _repositorioEvento.ObterPorIdAsync(eventoId);

        if (evento is null)
            return Resultado.Erro($"Evento com ID {eventoId} não encontrado");

        bool jaParticipa = evento.Participantes.Any(ep => ep.UsuarioId == usuarioId);

        if (jaParticipa)
            return Resultado.Erro($"Usuário com ID {usuarioId} já participa do evento");

        EventoParticipante participante = new EventoParticipante
        {
            EventoId = eventoId,
            UsuarioId = usuarioId
        };

        evento.Participantes.Add(participante);
        await _repositorioEvento.SalvarMudancasAsync();

        return Resultado.Ok("Evento criado com sucesso");
    }

    public async Task<Resultado> DesmarcarParticipacaoEvento(Guid eventoId, Guid usuarioId)
    {
        Evento? evento = await _repositorioEvento.ObterPorIdAsync(eventoId);

        if (evento is null)
            return Resultado.Erro($"Evento com ID {eventoId} não encontrado");

        if (evento.AnfitriaoId == usuarioId)
            return Resultado.Erro($"Anfitrião não pode sair do evento");

        EventoParticipante? participante = evento.Participantes.FirstOrDefault(p => p.UsuarioId == usuarioId);

        if (participante is null)
            return Resultado.Erro($"Usuário com ID {usuarioId} não participa do evento");

        evento.Participantes.Remove(participante);
        await _repositorioEvento.SalvarMudancasAsync();

        return Resultado.Ok("Participação do evento desmarcada com sucesso");
    }

    public async Task<Resultado> AtualizarEvento(Guid id, AtualizarEventoDTO eventoDTO, Guid usuarioId)
    {
        Evento? evento = await _repositorioEvento.ObterPorIdAsync(id);

        if (evento is null)
            return Resultado.Erro($"Evento com ID {id} não encontrado");

        if (evento.AnfitriaoId != usuarioId)
            return Resultado.Erro("Apenas o anfitrião pode atualizar informações do evento");

        Evento? eventoIgual = await _repositorioEvento.ObterPorTituloAsync(eventoDTO.Titulo);

        if (eventoIgual is not null)
            return Resultado.Erro($"Evento com titulo {eventoDTO.Titulo} já existe");

        evento.Titulo = eventoDTO.Titulo;
        evento.Descricao = eventoDTO.Descricao;
        evento.Localizacao = eventoDTO.Localizacao;
        evento.DataInicio = eventoDTO.DataInicio;

        _repositorioEvento.Atualizar(evento);
        await _repositorioEvento.SalvarMudancasAsync();

        return Resultado.Ok("Evento atualizado com sucesso");
    }

    public async Task<Resultado> RemoverEvento(Guid eventoId, Guid usuarioId)
    {
        Evento? evento = await _repositorioEvento.ObterPorIdAsync(eventoId);

        if (evento is null)
            return Resultado.Erro($"Evento com ID {eventoId} não encontrado");

        if (evento.AnfitriaoId != usuarioId)
            return Resultado.Erro("Apenas o anfitrião pode excluir o evento");

        _repositorioEvento.Remover(evento);
        await _repositorioEvento.SalvarMudancasAsync();

        return Resultado.Ok("Evento removido com sucesso");
    }
}