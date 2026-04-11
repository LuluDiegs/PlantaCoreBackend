namespace PlantaCoreAPI.Domain.Entities;

public class Evento
{
    public Guid Id { get; private set; }
    public string Titulo { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public string Localizacao { get; private set; } = string.Empty;
    public DateTime DataInicio { get; private set; }
    public Guid AnfitriaoId { get; private set; }
    public Usuario Anfitriao { get; private set; } = null!;
    public List<EventoParticipante> Participantes { get; set; } = new();
    private Evento() { }
    public static Evento Criar(Guid anfitriaoId, string titulo, string descricao, string localizacao, DateTime dataInicio)
    {
        var evento = new Evento
        {
            Id = Guid.NewGuid(),
            AnfitriaoId = anfitriaoId,
            Titulo = titulo,
            Descricao = descricao,
            Localizacao = localizacao,
            DataInicio = DateTime.SpecifyKind(dataInicio, DateTimeKind.Utc)
        };
        evento.ValidarDados();
        return evento;
    }

    public void Atualizar(string titulo, string descricao, string localizacao, DateTime dataInicio)
    {
        Titulo = titulo;
        Descricao = descricao;
        Localizacao = localizacao;
        DataInicio = DateTime.SpecifyKind(dataInicio, DateTimeKind.Utc);
        ValidarDados();
    }

    public void ValidarDados()
    {
        if (string.IsNullOrWhiteSpace(Titulo))
            throw new Exceptions.DomainException("Título do evento não pode estar vazio");
        if (Titulo.Length > 200)
            throw new Exceptions.DomainException("Título não pode ter mais de 200 caracteres");
        if (DataInicio <= DateTime.UtcNow)
            throw new Exceptions.DomainException("A data de início deve ser no futuro");
    }
}
