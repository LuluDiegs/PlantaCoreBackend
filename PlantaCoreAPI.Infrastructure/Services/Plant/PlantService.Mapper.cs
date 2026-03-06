using PlantaCoreAPI.Application.DTOs.Planta;
using PlantaCoreAPI.Domain.Entities;

namespace PlantaCoreAPI.Infrastructure.Services;

public sealed partial class PlantService
{
    private static PlantaDTOSaida MapearPlantaPara(Planta planta) => new()
    {
        Id = planta.Id,
        NomeCientifico = planta.NomeCientifico,
        NomeComum = planta.NomeComum,
        Familia = planta.Familia,
        Genero = planta.Genero,
        Toxica = planta.Toxica ? "Sim" : "Não",
        DescricaoToxicidade = planta.DescricaoToxicidade,
        ToxicaAnimais = planta.ToxicaAnimais ? "Sim" : "Não",
        DescricaoToxicidadeAnimais = planta.DescricaoToxicidadeAnimais,
        ToxicaCriancas = planta.ToxicaCriancas ? "Sim" : "Não",
        DescricaoToxicidadeCriancas = planta.DescricaoToxicidadeCriancas,
        RequisitosLuz = planta.RequisitosLuz,
        RequisitosAgua = planta.RequisitosAgua,
        RequisitosTemperatura = planta.RequisitosTemperatura,
        Cuidados = planta.Cuidados,
        FotoPlanta = planta.FotoPlanta,
        DataIdentificacao = planta.DataIdentificacao
    };

    private static Planta CriarPlantaDeEnriquecidos(Guid usuarioId, DadosPlantaEnriquecidos d, string? fotoOverride = null) =>
        Planta.Criar(
            usuarioId,
            d.NomeCientifico,
            d.NomeComum,
            d.Familia,
            d.Genero,
            string.Equals(d.Toxica, "Sim", StringComparison.OrdinalIgnoreCase),
            d.DescricaoToxicidade,
            string.Equals(d.ToxicaAnimais, "Sim", StringComparison.OrdinalIgnoreCase),
            d.DescricaoToxicidadeAnimais,
            string.Equals(d.ToxicaCriancas, "Sim", StringComparison.OrdinalIgnoreCase),
            d.DescricaoToxicidadeCriancas,
            d.RequisitosLuz,
            d.RequisitosAgua,
            d.RequisitosTemperatura,
            d.Cuidados,
            fotoOverride ?? d.FotoPlanta);

    private async Task<string?> BaixarESalvarFotoAsync(string urlFoto, Guid usuarioId)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient(nameof(PlantService));
            var bytes = await httpClient.GetByteArrayAsync(urlFoto);

            var nomeArquivo = Path.GetFileName(new Uri(urlFoto).AbsolutePath);
            if (string.IsNullOrWhiteSpace(nomeArquivo))
                nomeArquivo = $"planta-{Guid.NewGuid()}.jpg";

            return await _servicioPlantaStorage.FazerUploadFotoPlantaAsync(bytes, nomeArquivo, usuarioId);
        }
        catch
        {
            return null;
        }
    }
}
