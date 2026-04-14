namespace PlantaCoreAPI.Application.DTOs.Post;

public class PostDTOSaida
{
    public Guid Id { get; set; }
    public Guid? PlantaId { get; set; }
    public Guid? ComunidadeId { get; set; }
    public string? NomeComunidade { get; set; }
    public Guid UsuarioId { get; set; }
    public string NomeUsuario { get; set; } = null!;
    public string? FotoUsuario { get; set; }
    public string? NomePlanta { get; set; }
    public string? FotoPlanta { get; set; }
    public string Conteudo { get; set; } = null!;
    public string? Localizacao { get; set; }
    public List<string> Hashtags { get; set; } = new();
    public List<string> Categorias { get; set; } = new();
    public List<string> PalavrasChave { get; set; } = new();
    public int TotalCurtidas { get; set; }
    public int TotalComentarios { get; set; }
    public bool CurtiuUsuario { get; set; }
    public bool ComentadoPorMim { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}

public class CriarPostDTOEntrada
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MinLength(1)]
    public string Conteudo { get; set; } = null!;
    public string? Localizacao { get; set; }
    public Guid? PlantaId { get; set; }
    public Guid? ComunidadeId { get; set; }
    public List<string>? Hashtags { get; set; }
    public List<string>? Categorias { get; set; }
    public List<string>? PalavrasChave { get; set; }
}

public class AtualizarPostDTOEntrada
{
    public string Conteudo { get; set; } = null!;
    public string? Localizacao { get; set; }
    public List<string>? Hashtags { get; set; }
    public List<string>? Categorias { get; set; }
    public List<string>? PalavrasChave { get; set; }
}
