namespace PlantaCoreAPI.Application.DTOs.Planta;

public class PlantaDTOSaida
{
    public Guid Id { get; set; }
    public string NomeCientifico { get; set; } = null!;
    public string? NomeComum { get; set; } 
    public string? Familia { get; set; }
    public string? Genero { get; set; }
    public bool Toxica { get; set; }
    public string? DescricaoToxicidade { get; set; }
    public bool ToxicaAnimais { get; set; }
    public string? DescricaoToxicidadeAnimais { get; set; }
    public bool ToxicaCriancas { get; set; }
    public string? DescricaoToxicidadeCriancas { get; set; }
    public string? RequisitosLuz { get; set; }
    public string? RequisitosAgua { get; set; }
    public string? RequisitosTemperatura { get; set; }
    public string? Cuidados { get; set; }
    public string? FotoPlanta { get; set; }
    public DateTime DataIdentificacao { get; set; }
}

public class BuscaPlantaDTOEntrada
{
    public string NomePlanta { get; set; } = null!;
    public int Pagina { get; set; } = 0;
}

public class ResultadoBuscaPlantaDTOSaida
{
    public List<PlantaBuscaDTOSaida> Plantas { get; set; } = new();
    public MetadadosPaginacaoDTOSaida? Paginacao { get; set; }
}

public class PlantaBuscaDTOSaida
{
    public int Id { get; set; }
    public string? NomeComum { get; set; }
    public string? NomeCientifico { get; set; }
    public string? Slug { get; set; }
    public string? UrlImagem { get; set; }
    public string? Genero { get; set; }
    public string? Familia { get; set; }
}

public class MetadadosPaginacaoDTOSaida
{
    public int Total { get; set; }
    public int TotalPaginas { get; set; }
    public int PaginaAtual { get; set; }
}
