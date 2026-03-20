namespace PlantaCoreAPI.Domain.Entities;

public class Curtida
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!; // Relacionamento com o usuário
    public Guid? PostId { get; set; } // Relacionamento opcional com um post
    public Post? Post { get; set; }
    public Guid? ComentarioId { get; set; } // Relacionamento opcional com um comentário
    public Comentario? Comentario { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    // Construtor público para permitir criação de instâncias
    public Curtida() { }

    // Método de fábrica para criar curtidas para comentários
    public static Curtida CriarParaComentario(Guid comentarioId, Guid usuarioId)
    {
        return new Curtida
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            ComentarioId = comentarioId,
            DataCriacao = DateTime.UtcNow
        };
    }
}
