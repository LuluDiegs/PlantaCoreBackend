namespace PlantaCoreAPI.Application.DTOs.Comentario;

public class ComentarioDTOSaida
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Guid UsuarioId { get; set; }
    public string NomeUsuario { get; set; } = null!;
    public string? FotoUsuario { get; set; }
    public string Conteudo { get; set; } = null!;
    public int TotalCurtidas { get; set; }
    public bool CurtiuUsuario { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public int TotalRespostas { get; set; }
    public Guid? ComentarioPaiId { get; set; }
}

public class CriarComentarioDTOEntrada
{
    public Guid PostId { get; set; }
    public string Conteudo { get; set; } = null!;
}

public class AtualizarComentarioDTOEntrada
{
    public string Conteudo { get; set; } = null!;
}
