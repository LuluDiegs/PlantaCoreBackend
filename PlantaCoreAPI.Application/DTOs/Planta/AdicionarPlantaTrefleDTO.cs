using System.ComponentModel.DataAnnotations;

namespace PlantaCoreAPI.Application.DTOs.Planta;

public class AdicionarPlantaTrefleDTO
{
    [Required(ErrorMessage = "plantaTrefleId é obrigatório")]
    public int PlantaTrefleId { get; set; }

    [Required(ErrorMessage = "nomeCientifico é obrigatório")]
    public string NomeCientifico { get; set; } = "";

    [Required(ErrorMessage = "urlImagem é obrigatório")]
    public string UrlImagem { get; set; } = "";
    public string? Localizacao { get; set; }
}
