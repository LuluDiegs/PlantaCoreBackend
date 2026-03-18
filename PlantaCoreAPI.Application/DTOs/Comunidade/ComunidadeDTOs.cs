namespace PlantaCoreAPI.Application.DTOs.Comunidade;

public class ComunidadeDTOSaida
{
    public Guid Id { get; set; }
    public Guid CriadorId { get; set; }
    public string NomeCriador { get; set; } = null!;
    public string Nome { get; set; } = null!;
    public string? Descricao { get; set; }
    public string? FotoComunidade { get; set; }
    public int TotalMembros { get; set; }
    public bool UsuarioEhMembro { get; set; }
    public bool UsuarioEhAdmin { get; set; }
    public DateTime DataCriacao { get; set; }
}

public class CriarComunidadeDTOEntrada
{
    public string Nome { get; set; } = null!;
    public string? Descricao { get; set; }
}

public class AtualizarComunidadeDTOEntrada
{
    public string? Nome { get; set; }
    public string? Descricao { get; set; }
}
