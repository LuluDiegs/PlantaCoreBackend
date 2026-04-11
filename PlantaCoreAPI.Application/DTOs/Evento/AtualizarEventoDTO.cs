using System.ComponentModel.DataAnnotations;

namespace PlantaCoreAPI.Application.DTOs.Evento;

public class AtualizarEventoDTO
{
    [Required(ErrorMessage = "Titulo é obrigatório")]
    [StringLength(200, ErrorMessage = "Titulo não pode ter mais de 200 caracteres")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Descrição é obrigatória")]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "Localização é obrigatória")]
    public string Localizacao { get; set; } = string.Empty;

    [Required(ErrorMessage = "Data de início é obrigatória")]
    public DateTime DataInicio { get; set; }
}
