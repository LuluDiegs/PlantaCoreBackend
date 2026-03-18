// Ajustando IdentificarEPostarDTO para herdar de IdentificacaoDTOEntrada
using PlantaCoreAPI.Application.DTOs.Identificacao;

namespace PlantaCoreAPI.Application.DTOs.Planta;

public class IdentificarEPostarDTO : IdentificacaoDTOEntrada
{
    public string? Comentario { get; set; }
}