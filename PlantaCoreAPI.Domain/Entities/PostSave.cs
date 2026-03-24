using System;

namespace PlantaCoreAPI.Domain.Entities;

public class PostSave
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid PostId { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
}
