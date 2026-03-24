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
    public int TotalCurtidas { get; set; }
    public int TotalComentarios { get; set; }
    public bool CurtiuUsuario { get; set; }
    public bool ComentadoPorMim { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}

public class CriarPostDTOEntrada
{
    public string Conteudo { get; set; } = null!;
    public Guid? PlantaId { get; set; } // Relacionamento opcional com uma planta
    public Guid? ComunidadeId { get; set; } // Relacionamento opcional com uma comunidade
}

public class AtualizarPostDTOEntrada
{
    public string Conteudo { get; set; } = null!;
}
