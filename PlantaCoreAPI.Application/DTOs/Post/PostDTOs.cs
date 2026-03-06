namespace PlantaCoreAPI.Application.DTOs.Post;

public class PostDTOSaida
{
    public Guid Id { get; set; }
    public Guid PlantaId { get; set; }
    public Guid UsuarioId { get; set; }
    public string NomeUsuario { get; set; } = null!;
    public string? FotoUsuario { get; set; }
    public string Conteudo { get; set; } = null!;
    public int TotalCurtidas { get; set; }
    public int TotalComentarios { get; set; }
    public bool CurtiuUsuario { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}

public class CriarPostDTOEntrada
{
    public Guid PlantaId { get; set; }
    public string Conteudo { get; set; } = null!;
}

public class AtualizarPostDTOEntrada
{
    public string Conteudo { get; set; } = null!;
}
