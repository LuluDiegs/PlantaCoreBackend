namespace PlantaCoreAPI.Application.Interfaces;

public class ResultadoIdentificacaoPlantNet
{
    public List<ResultadoPlantNet> Resultados { get; set; } = new();
    public QueryPlantNet? Query { get; set; }
}

public class QueryPlantNet
{
    public string? Data { get; set; }
    public List<string>? Imagens { get; set; }
}

public class ResultadoPlantNet
{
    public EspeciePlantNet? Especie { get; set; }
    public double Probabilidade { get; set; }
    public double Score { get; set; }
}

public class EspeciePlantNet
{
    public string? NomeCientifico { get; set; }
    public List<string>? NomesComuns { get; set; }
}

public interface IPlantNetService
{
    Task<ResultadoIdentificacaoPlantNet?> IdentificarPlantaPorArquivoAsync(string caminhoArquivo);
    Task<ResultadoIdentificacaoPlantNet?> IdentificarPlantaPorUrlAsync(string urlImagem);
}
