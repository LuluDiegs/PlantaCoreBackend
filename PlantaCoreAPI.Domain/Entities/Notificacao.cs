namespace PlantaCoreAPI.Domain.Entities;

public class Notificacao
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Usuario? Usuario { get; private set; }
    public Guid? UsuarioOrigemId { get; private set; }
    public Usuario? UsuarioOrigem { get; private set; }
    public Guid? PlantaId { get; private set; }
    public Planta? Planta { get; private set; }
    public Guid? PostId { get; private set; }
    public Post? Post { get; private set; }
    public Enums.TipoNotificacao Tipo { get; private set; }
    public string Mensagem { get; private set; } = null!;
    public bool Lida { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataLeitura { get; private set; }
    public DateTime? DataDelecao { get; private set; }

    private Notificacao() { }

    public static Notificacao Criar(
        Guid usuarioId,
        Enums.TipoNotificacao tipo,
        string mensagem,
        Guid? usuarioOrigemId = null,
        Guid? plantaId = null,
        Guid? postId = null)
    {
        if (string.IsNullOrWhiteSpace(mensagem))
            throw new Exceptions.DomainException("Mensagem não pode estar vazia");

        return new Notificacao
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Tipo = tipo,
            Mensagem = mensagem.Trim(),
            UsuarioOrigemId = usuarioOrigemId,
            PlantaId = plantaId,
            PostId = postId,
            Lida = false,
            DataCriacao = DateTime.UtcNow
        };
    }

    public void MarcarComoLida()
    {
        Lida = true;
        DataLeitura = DateTime.UtcNow;
    }

    public void Deletar()
    {
        DataDelecao = DateTime.UtcNow;
    }
}
