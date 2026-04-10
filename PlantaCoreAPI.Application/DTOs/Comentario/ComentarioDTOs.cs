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
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "PostId é obrigatório")]
    public Guid PostId { get; set; }

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Conteúdo é obrigatório")]
    [System.ComponentModel.DataAnnotations.MinLength(1, ErrorMessage = "Conteúdo não pode estar vazio")]
    public string Conteudo { get; set; } = null!;
}

public class AtualizarComentarioDTOEntrada
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Conteúdo é obrigatório")]
    [System.ComponentModel.DataAnnotations.MinLength(1, ErrorMessage = "Conteúdo não pode estar vazio")]
    public string Conteudo { get; set; } = null!;
}

public class ResponderComentarioDTOEntrada
{
    [System.ComponentModel.DataAnnotations.Required]
    public string Conteudo { get; set; } = null!;
}
