namespace PlantaCoreAPI.Application.Interfaces;

public class ResultadoBuscaTrefle
{
    public List<PlantaTrefle> Dados { get; set; } = new();
    public MetadadosTrefle? Metadados { get; set; }
    public LinksTrefle? Links { get; set; }
}

public class PlantaTrefle
{
    public int Id { get; set; }
    public string? NomeComum { get; set; }
    public string? NomeCientifico { get; set; }
    public string? Slug { get; set; }
    public string? UrlImagem { get; set; }
    public string? Genero { get; set; }
    public string? Familia { get; set; }
}

public class MetadadosTrefle
{
    public int Total { get; set; }
    public int TotalPaginas { get; set; }
    public int Pagina { get; set; }
}

public class LinksTrefle
{
    public string? Primeiro { get; set; }
    public string? Ultimo { get; set; }
    public string? Proximo { get; set; }
}

public interface ITrefleService
{
    Task<ResultadoBuscaTrefle?> BuscarPlantasAsync(string termo, int pagina = 0);
    Task<PlantaTrefle?> ObterPlantaPorIdAsync(int plantaId);
}
