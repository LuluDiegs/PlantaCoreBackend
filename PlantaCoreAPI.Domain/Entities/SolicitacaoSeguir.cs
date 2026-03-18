namespace PlantaCoreAPI.Domain.Entities;

public class SolicitacaoSeguir
{
    public Guid Id { get; private set; }
    public Guid SolicitanteId { get; private set; }
    public Usuario? Solicitante { get; private set; }
    public Guid AlvoId { get; private set; }
    public Usuario? Alvo { get; private set; }
    public DateTime DataSolicitacao { get; private set; }
    public bool Pendente { get; private set; } = true;

    private SolicitacaoSeguir() { }

    public static SolicitacaoSeguir Criar(Guid solicitanteId, Guid alvoId)
    {
        if (solicitanteId == alvoId)
            throw new Exceptions.DomainException("Você não pode enviar solicitação para si mesmo");

        return new SolicitacaoSeguir
        {
            Id = Guid.NewGuid(),
            SolicitanteId = solicitanteId,
            AlvoId = alvoId,
            DataSolicitacao = DateTime.UtcNow,
            Pendente = true
        };
    }

    public void Aceitar()
    {
        Pendente = false;
    }

    public void Rejeitar()
    {
        Pendente = false;
    }
}
