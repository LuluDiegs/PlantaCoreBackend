namespace PlantaCoreAPI.Domain.Entities;

public class ActivityLog
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string Tipo { get; private set; } = string.Empty;
    public Guid? EntidadeId { get; private set; }
    public string? EntidadeTipo { get; private set; }
    public string? MetaDados { get; private set; }
    public DateTime DataCriacao { get; private set; }
    private ActivityLog() { }
    public static ActivityLog Criar(Guid usuarioId, string tipo, Guid? entidadeId = null, string? entidadeTipo = null, string? metaDados = null)
    {
        if (string.IsNullOrWhiteSpace(tipo))
            throw new Exceptions.DomainException("Tipo do log não pode estar vazio");
        return new ActivityLog
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Tipo = tipo,
            EntidadeId = entidadeId,
            EntidadeTipo = entidadeTipo,
            MetaDados = metaDados,
            DataCriacao = DateTime.UtcNow
        };
    }
}
