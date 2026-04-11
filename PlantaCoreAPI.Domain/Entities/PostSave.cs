namespace PlantaCoreAPI.Domain.Entities;

public class PostSave
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Guid PostId { get; private set; }
    public DateTime DataCriacao { get; private set; }

    private PostSave() { }

    public static PostSave Criar(Guid usuarioId, Guid postId)
    {
        return new PostSave
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            PostId = postId,
            DataCriacao = DateTime.UtcNow
        };
    }
}
