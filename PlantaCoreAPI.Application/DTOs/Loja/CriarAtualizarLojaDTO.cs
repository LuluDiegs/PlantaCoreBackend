namespace PlantaCoreAPI.Application.DTOs.Evento;

public class CriarAtualizarLojaDTO
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }

    public string? ImagemUrl { get; set; }
    public bool SomenteOnline { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public string? Endereco { get; set; }
}