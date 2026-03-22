using System.ComponentModel.DataAnnotations;

public class AtualizarEventoDTO
{
    [Required(ErrorMessage = "Titulo é obrigatório")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Descrição é obrigatória")]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "Localização é obrigatória")]
    public string Localizacao { get; set; } = string.Empty;

    public DateTime DataInicio { get; set; }
}