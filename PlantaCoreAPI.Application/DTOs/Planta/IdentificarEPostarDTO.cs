// Ajustando IdentificarEPostarDTO para não herdar de IdentificacaoDTOEntrada
using Microsoft.AspNetCore.Http;
using PlantaCoreAPI.Application.DTOs.Identificacao;

namespace PlantaCoreAPI.Application.DTOs.Planta;

public class IdentificarEPostarDTO
{
    public IFormFile Foto { get; set; } = null!;
    public string? Comentario { get; set; }
    public bool CriarPostagem { get; set; } = false;
}