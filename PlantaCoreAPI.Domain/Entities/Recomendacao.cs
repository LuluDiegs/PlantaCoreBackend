namespace PlantaCoreAPI.Domain.Entities;

public class Recomendacao
{
    public Guid Id { get; set; }
    public string NomeComum { get; set; } = string.Empty;
    public string UrlImagem { get; set; } = string.Empty;
    public string Justificativa { get; set; } = string.Empty;
    public string Experiencia { get; set; } = string.Empty;
    public string Iluminacao { get; set; } = string.Empty;
    public string Regagem { get; set; } = string.Empty;
    public string Seguranca { get; set; } = string.Empty;
    public string Proposito { get; set; } = string.Empty;
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
}