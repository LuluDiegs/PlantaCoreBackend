namespace PlantaCoreAPI.Domain.Entities;

public class Curtida
{
    public Guid Id { get; private set; }
    public Guid? PostId { get; private set; }
    public Post? Post { get; private set; }
    public Guid? ComentarioId { get; private set; }
    public Comentario? Comentario { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Usuario? Usuario { get; private set; }
    public DateTime DataCriacao { get; private set; } = DateTime.UtcNow;

    private Curtida() { }

    public static Curtida Criar(Guid postId, Guid usuarioId)
    {
        return new Curtida
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            UsuarioId = usuarioId,
            DataCriacao = DateTime.UtcNow
        };
    }

    public static Curtida CriarParaComentario(Guid comentarioId, Guid usuarioId)
    {
        return new Curtida
        {
            Id = Guid.NewGuid(),
            ComentarioId = comentarioId,
            UsuarioId = usuarioId,
            DataCriacao = DateTime.UtcNow
        };
    }
}
