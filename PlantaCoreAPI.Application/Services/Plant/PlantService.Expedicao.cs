using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.DTOs.Planta;
using PlantaCoreAPI.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace PlantaCoreAPI.Application.Services;

public sealed partial class PlantService
{
    private const int AlcanceCapturaMetros = 150;

    public async Task<Resultado<ExpedicaoPlantasProximasDTOSaida>> BuscarPlantasProximasAsync(Guid usuarioId, BuscarPlantasProximasDTOEntrada entrada)
    {
        try
        {
            if (!CoordenadasValidas(entrada.Latitude, entrada.Longitude))
                return Resultado<ExpedicaoPlantasProximasDTOSaida>.Erro("Coordenadas invalidas.");

            if (entrada.RaioKm <= 0 || entrada.RaioKm > 15)
                return Resultado<ExpedicaoPlantasProximasDTOSaida>.Erro("O raio de busca deve estar entre 0.1 km e 15 km.");

            var limite = Math.Clamp(entrada.Limite, 1, 100);
            var plantasDoUsuario = await _repositorioPlanta.ObterPorUsuarioAsync(usuarioId);
            var especiesJaColecionadas = plantasDoUsuario
                .Where(p => !string.IsNullOrWhiteSpace(p.NomeCientifico))
                .Select(p => p.NomeCientifico)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var candidatas = await _repositorioPlanta.ObterPlantasCompartilhadasAsync(usuarioId);
            var raioMetros = entrada.RaioKm * 1000d;

            var proximas = candidatas
                .Select(planta => new
                {
                    Planta = planta,
                    DistanciaMetros = CalcularDistanciaMetros(
                        entrada.Latitude,
                        entrada.Longitude,
                        planta.Latitude!.Value,
                        planta.Longitude!.Value)
                })
                .Where(x => x.DistanciaMetros <= raioMetros)
                .OrderBy(x => x.DistanciaMetros)
                .ToList();

            var frequenciaPorEspecie = proximas
                .GroupBy(x => x.Planta.NomeCientifico, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

            var plantas = proximas
                .Take(limite)
                .Select(x =>
                {
                    var jaColecionada = especiesJaColecionadas.Contains(x.Planta.NomeCientifico);
                    return new PlantaProximaDTOSaida
                    {
                        Id = x.Planta.Id,
                        NomeCientifico = x.Planta.NomeCientifico,
                        NomeComum = x.Planta.NomeComum,
                        FotoPlanta = x.Planta.FotoPlanta,
                        Familia = x.Planta.Familia,
                        Genero = x.Planta.Genero,
                        DonoNome = x.Planta.Usuario?.Nome,
                        Latitude = x.Planta.Latitude!.Value,
                        Longitude = x.Planta.Longitude!.Value,
                        DistanciaMetros = Math.Round(x.DistanciaMetros, 0),
                        Raridade = CalcularRaridade(frequenciaPorEspecie[x.Planta.NomeCientifico]),
                        JaColecionada = jaColecionada,
                        DisponivelParaCaptura = !jaColecionada && x.DistanciaMetros <= AlcanceCapturaMetros
                    };
                })
                .ToList();

            return Resultado<ExpedicaoPlantasProximasDTOSaida>.Ok(new ExpedicaoPlantasProximasDTOSaida
            {
                Plantas = plantas,
                Total = plantas.Count,
                NovasEspecies = plantas.Count(p => !p.JaColecionada),
                JaColecionadas = plantas.Count(p => p.JaColecionada),
                AlcanceCapturaMetros = AlcanceCapturaMetros,
                RaioKm = entrada.RaioKm
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar plantas proximas para o usuario {UsuarioId}", usuarioId);
            return Resultado<ExpedicaoPlantasProximasDTOSaida>.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado<CapturaExpedicaoDTOSaida>> CapturarPlantaProximaAsync(Guid usuarioId, CapturarPlantaProximaDTOEntrada entrada)
    {
        try
        {
            if (!CoordenadasValidas(entrada.Latitude, entrada.Longitude))
                return Resultado<CapturaExpedicaoDTOSaida>.Erro("Coordenadas invalidas.");

            var plantaOrigem = await _repositorioPlanta.ObterPorIdAsync(entrada.PlantaId);
            if (plantaOrigem is null)
                return Resultado<CapturaExpedicaoDTOSaida>.Erro("Planta nao encontrada.");

            if (plantaOrigem.UsuarioId == usuarioId)
                return Resultado<CapturaExpedicaoDTOSaida>.Erro("Voce nao pode capturar uma planta sua.");

            if (!plantaOrigem.CompartilharLocalizacao || plantaOrigem.Latitude is null || plantaOrigem.Longitude is null)
                return Resultado<CapturaExpedicaoDTOSaida>.Erro("Essa planta nao esta disponivel na expedicao.");

            var jaExisteNaColecao = await _repositorioPlanta.ObterPorNomeCientificoEUsuarioAsync(plantaOrigem.NomeCientifico, usuarioId);
            if (jaExisteNaColecao is not null)
                return Resultado<CapturaExpedicaoDTOSaida>.Erro("Essa especie ja faz parte da sua colecao.");

            var distanciaMetros = CalcularDistanciaMetros(
                entrada.Latitude,
                entrada.Longitude,
                plantaOrigem.Latitude.Value,
                plantaOrigem.Longitude.Value);

            if (distanciaMetros > AlcanceCapturaMetros)
                return Resultado<CapturaExpedicaoDTOSaida>.Erro($"Aproxime-se mais da planta para capturar. Alcance maximo: {AlcanceCapturaMetros} m.");

            var novaPlanta = Planta.Criar(
                usuarioId,
                plantaOrigem.NomeCientifico,
                plantaOrigem.NomeComum,
                plantaOrigem.Familia,
                plantaOrigem.Genero,
                plantaOrigem.Toxica,
                plantaOrigem.DescricaoToxicidade,
                plantaOrigem.ToxicaAnimais,
                plantaOrigem.DescricaoToxicidadeAnimais,
                plantaOrigem.ToxicaCriancas,
                plantaOrigem.DescricaoToxicidadeCriancas,
                plantaOrigem.RequisitosLuz,
                plantaOrigem.RequisitosAgua,
                plantaOrigem.RequisitosTemperatura,
                plantaOrigem.Cuidados,
                plantaOrigem.FotoPlanta,
                false,
                null,
                null);

            await _repositorioPlanta.AdicionarAsync(novaPlanta);
            await _repositorioPlanta.SalvarMudancasAsync();

            var plantasCompartilhadas = await _repositorioPlanta.ObterPlantasCompartilhadasAsync(usuarioId);
            var ocorrenciasEspecie = plantasCompartilhadas.Count(p =>
                string.Equals(p.NomeCientifico, plantaOrigem.NomeCientifico, StringComparison.OrdinalIgnoreCase));

            return Resultado<CapturaExpedicaoDTOSaida>.Ok(new CapturaExpedicaoDTOSaida
            {
                PlantaCapturada = MapearPlantaPara(novaPlanta),
                DistanciaMetros = Math.Round(distanciaMetros, 0),
                Raridade = CalcularRaridade(Math.Max(1, ocorrenciasEspecie)),
                AlcanceCapturaMetros = AlcanceCapturaMetros
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao capturar planta na expedicao para o usuario {UsuarioId}", usuarioId);
            return Resultado<CapturaExpedicaoDTOSaida>.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    private static bool CoordenadasValidas(float latitude, float longitude) =>
        latitude >= -90 && latitude <= 90 && longitude >= -180 && longitude <= 180;

    private static double CalcularDistanciaMetros(float origemLat, float origemLng, float destinoLat, float destinoLng)
    {
        const double raioTerraMetros = 6371000d;
        var lat1 = GrausParaRadianos(origemLat);
        var lat2 = GrausParaRadianos(destinoLat);
        var deltaLat = GrausParaRadianos(destinoLat - origemLat);
        var deltaLng = GrausParaRadianos(destinoLng - origemLng);

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(deltaLng / 2) * Math.Sin(deltaLng / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return raioTerraMetros * c;
    }

    private static double GrausParaRadianos(double graus) => graus * (Math.PI / 180d);

    private static string CalcularRaridade(int ocorrenciasDaEspecie)
    {
        if (ocorrenciasDaEspecie <= 1)
            return "Rara";
        if (ocorrenciasDaEspecie <= 3)
            return "Incomum";
        return "Comum";
    }
}
