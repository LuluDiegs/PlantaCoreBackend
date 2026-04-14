namespace PlantaCoreAPI.Application.Services.Plant;

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

    public void GarantirConsistenciaToxicidade()
    {
        if (!string.IsNullOrWhiteSpace(DescricaoToxicidade))
        {
            var desc = DescricaoToxicidade.Trim().ToLowerInvariant();
            if (desc.StartsWith("sim") || desc.Contains("tóxic") || desc.Contains("toxina") || desc.Contains("veneno") || desc.Contains("grayanotoxina") || desc.Contains("alcaloide") || desc.Contains("glicosídeo") || desc.Contains("diterpeno"))
                Toxica = "Sim";
            else if (desc.StartsWith("não") || desc.StartsWith("nao"))
                Toxica = "Não";
        }

        if (Toxica != "Sim" && Toxica != "Não")
            Toxica = "Não";

        if (!string.IsNullOrWhiteSpace(DescricaoToxicidadeAnimais))
        {
            var desc = DescricaoToxicidadeAnimais.Trim().ToLowerInvariant();
            if (desc.StartsWith("sim") || desc.Contains("tóxic") || desc.Contains("toxina") || desc.Contains("veneno") || desc.Contains("grayanotoxina") || desc.Contains("alcaloide") || desc.Contains("glicosídeo") || desc.Contains("diterpeno"))
                ToxicaAnimais = "Sim";
            else if (desc.StartsWith("não") || desc.StartsWith("nao"))
                ToxicaAnimais = "Não";
        }

        if (ToxicaAnimais != "Sim" && ToxicaAnimais != "Não")
            ToxicaAnimais = "Não";

        if (!string.IsNullOrWhiteSpace(DescricaoToxicidadeCriancas))
        {
            var desc = DescricaoToxicidadeCriancas.Trim().ToLowerInvariant();
            if (desc.StartsWith("sim") || desc.Contains("tóxic") || desc.Contains("toxina") || desc.Contains("veneno") || desc.Contains("grayanotoxina") || desc.Contains("alcaloide") || desc.Contains("glicosídeo") || desc.Contains("diterpeno"))
                ToxicaCriancas = "Sim";
            else if (desc.StartsWith("não") || desc.StartsWith("nao"))
                ToxicaCriancas = "Não";
        }

        if (ToxicaCriancas != "Sim" && ToxicaCriancas != "Não")
            ToxicaCriancas = "Não";
    }
}
