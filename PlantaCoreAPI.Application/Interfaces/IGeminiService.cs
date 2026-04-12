namespace PlantaCoreAPI.Application.Interfaces;

public class DadosPlantaParaIA
{
    public string? NomeCientifico { get; set; }
    public string? NomeComum { get; set; }
    public string? Familia { get; set; }
    public string? Genero { get; set; }
    public string? Toxicidade { get; set; }
    public bool? ToxicoPets { get; set; }
    public List<string>? Luz { get; set; }
    public string? Rega { get; set; }
    public string? HardinessZona { get; set; }
    public string? NivelCuidado { get; set; }
    public List<string>? TempoFlorada { get; set; }
    public List<string>? CorFlor { get; set; }
    public string? Descricao { get; set; }
    public bool? ToleranciaSeca { get; set; }
    public bool? AtraiPolinizadores { get; set; }
}

public class DadosRecomendacaoPlantaParaIA
{
    public string Experiencia { get; set; } = string.Empty;
    public string Iluminacao { get; set; } = string.Empty;
    public string Regagem { get; set; } = string.Empty;
    public string Seguranca { get; set; } = string.Empty;
    public string Proposito { get; set; } = string.Empty;
}

public interface IGeminiService
{
    Task<string?> GerarDescricaoPlantaAsync(DadosPlantaParaIA dados);
    Task<string?> GerarReflexaoPlantaAsync(DadosPlantaParaIA dados, string respostaPrincipal);
    Task<string?> GerarRecomendacaoPlantaAsync(DadosRecomendacaoPlantaParaIA dados);
}
