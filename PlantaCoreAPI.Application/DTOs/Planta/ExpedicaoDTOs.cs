namespace PlantaCoreAPI.Application.DTOs.Planta;

public class BuscarPlantasProximasDTOEntrada
{
    public float Latitude { get; set; }
    public float Longitude { get; set; }
    public double RaioKm { get; set; } = 2;
    public int Limite { get; set; } = 30;
}

public class CapturarPlantaProximaDTOEntrada
{
    public Guid PlantaId { get; set; }
    public float Latitude { get; set; }
    public float Longitude { get; set; }
}

public class PlantaProximaDTOSaida
{
    public Guid Id { get; set; }
    public string NomeCientifico { get; set; } = null!;
    public string? NomeComum { get; set; }
    public string? FotoPlanta { get; set; }
    public string? Familia { get; set; }
    public string? Genero { get; set; }
    public string? DonoNome { get; set; }
    public float Latitude { get; set; }
    public float Longitude { get; set; }
    public double DistanciaMetros { get; set; }
    public string Raridade { get; set; } = "Comum";
    public bool JaColecionada { get; set; }
    public bool DisponivelParaCaptura { get; set; }
}

public class ExpedicaoPlantasProximasDTOSaida
{
    public IReadOnlyCollection<PlantaProximaDTOSaida> Plantas { get; set; } = Array.Empty<PlantaProximaDTOSaida>();
    public int Total { get; set; }
    public int NovasEspecies { get; set; }
    public int JaColecionadas { get; set; }
    public int AlcanceCapturaMetros { get; set; }
    public double RaioKm { get; set; }
}

public class CapturaExpedicaoDTOSaida
{
    public PlantaDTOSaida PlantaCapturada { get; set; } = null!;
    public double DistanciaMetros { get; set; }
    public string Raridade { get; set; } = "Comum";
    public int AlcanceCapturaMetros { get; set; }
}
