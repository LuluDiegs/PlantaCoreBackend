namespace PlantaCoreAPI.Domain.Entities;

public class PostShare
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Guid PostId { get; private set; }
    public DateTime DataCriacao { get; private set; }

    private PostShare() { }

    public static PostShare Criar(Guid usuarioId, Guid postId)
    {
        return new PostShare
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            PostId = postId,
            DataCriacao = DateTime.UtcNow
        };
    }
}
