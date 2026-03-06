namespace PlantaCoreAPI.Infrastructure.Services;

internal sealed class DadosPlantaEnriquecidos
{
    public string NomeCientifico { get; set; } = "";
    public string? NomeComum { get; set; }
    public string? Familia { get; set; }
    public string? Genero { get; set; }
    public string? Toxica { get; set; }
    public string? DescricaoToxicidade { get; set; }
    public string? ToxicaAnimais { get; set; }
    public string? DescricaoToxicidadeAnimais { get; set; }
    public string? ToxicaCriancas { get; set; }
    public string? DescricaoToxicidadeCriancas { get; set; }
    public string? RequisitosLuz { get; set; }
    public string? RequisitosAgua { get; set; }
    public string? RequisitosTemperatura { get; set; }
    public string? Cuidados { get; set; }
    public string? FotoPlanta { get; set; }
}
