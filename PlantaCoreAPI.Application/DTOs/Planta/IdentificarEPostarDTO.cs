using Microsoft.AspNetCore.Http;

namespace PlantaCoreAPI.Application.DTOs.Planta;

public class IdentificarEPostarDTO
{
    public IFormFile Foto { get; set; } = null!;
    public string? Comentario { get; set; }
    public bool CriarPostagem { get; set; } = false;
    public string? Localizacao { get; set; }
}
