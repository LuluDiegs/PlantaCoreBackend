namespace PlantaCoreAPI.Domain.Entities;

public class PostView
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Guid PostId { get; private set; }
    public DateTime DataCriacao { get; private set; }

    private PostView() { }

    public static PostView Criar(Guid usuarioId, Guid postId)
    {
        return new PostView
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            PostId = postId,
            DataCriacao = DateTime.UtcNow
        };
    }
}
