using System;

namespace PlantaCoreAPI.Domain.Entities;

public class ActivityLog
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string Tipo { get; set; } = null!; // Ex: "CURTIDA_POST", "COMENTARIO", etc.
    public Guid? EntidadeId { get; set; } // Id do post, comentário, etc.
    public string? EntidadeTipo { get; set; } // Ex: "Post", "Comentario", "Planta"
    public string? MetaDados { get; set; } // JSON ou string para detalhes extras
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
}
