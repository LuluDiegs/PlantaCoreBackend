using PlantaCoreAPI.Application.Interfaces;

namespace PlantaCoreAPI.Infrastructure.Services;

public sealed partial class PlantService
{
    private DadosPlantaEnriquecidos ExtrairDadosDoGemini(string? descricaoGemini, string nomeCientifico, PlantaTrefle? plantaTrefle)
    {
        var resultado = new DadosPlantaEnriquecidos
        {
            NomeCientifico = nomeCientifico,
            NomeComum = plantaTrefle?.NomeComum,
            Familia = plantaTrefle?.Familia,
            Genero = plantaTrefle?.Genero,
            FotoPlanta = plantaTrefle?.UrlImagem
        };

        if (!string.IsNullOrWhiteSpace(descricaoGemini))
        {
            foreach (var linhaOriginal in descricaoGemini.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (string.IsNullOrWhiteSpace(linhaOriginal)) continue;

                var linhaNormalizada = NormalizarLinha(linhaOriginal);
                var linhaLower = linhaNormalizada.ToLowerInvariant();
                var valor = ExtrairValor(linhaNormalizada);

                if (string.IsNullOrWhiteSpace(valor)) continue;

                valor = LimparMarkdown(valor);

                if (string.IsNullOrWhiteSpace(valor)) continue;

                if (linhaLower.StartsWith("nome comum:") && string.IsNullOrWhiteSpace(resultado.NomeComum))
                    resultado.NomeComum = valor;
                else if (linhaLower.StartsWith("nome científico:") || linhaLower.StartsWith("nome cientifico:"))
                {
                    if (!string.IsNullOrWhiteSpace(valor))
                        resultado.NomeCientifico = valor;
                }

                else if (linhaLower.StartsWith("família:") || linhaLower.StartsWith("familia:"))
                    resultado.Familia = valor;
                else if ((linhaLower.StartsWith("gênero:") || linhaLower.StartsWith("genero:")) && string.IsNullOrWhiteSpace(resultado.Genero))
                    resultado.Genero = valor;
                else if (linhaLower.StartsWith("toxicidade para humanos:"))
                {
                    resultado.DescricaoToxicidade = valor;
                    resultado.Toxica = ClassificarToxicidade(valor);
                }
                else if (linhaLower.StartsWith("toxicidade para animais domésticos:") ||
                         linhaLower.StartsWith("toxicidade para animais:") ||
                         linhaLower.StartsWith("toxicidade animais:"))
                {
                    resultado.DescricaoToxicidadeAnimais = valor;
                    resultado.ToxicaAnimais = ClassificarToxicidade(valor);
                }
                else if (linhaLower.StartsWith("toxicidade para crianças:") ||
                         linhaLower.StartsWith("toxicidade em crianças:") ||
                         linhaLower.StartsWith("toxicidade criancas:") ||
                         linhaLower.StartsWith("toxicidade para criancas:"))
                {
                    resultado.DescricaoToxicidadeCriancas = valor;
                    resultado.ToxicaCriancas = ClassificarToxicidade(valor);
                }
                else if (linhaLower.StartsWith("luz:") && string.IsNullOrWhiteSpace(resultado.RequisitosLuz))
                    resultado.RequisitosLuz = valor;
                else if ((linhaLower.StartsWith("água:") || linhaLower.StartsWith("agua:")) && string.IsNullOrWhiteSpace(resultado.RequisitosAgua))
                    resultado.RequisitosAgua = valor;
                else if (linhaLower.StartsWith("temperatura ideal:") && string.IsNullOrWhiteSpace(resultado.RequisitosTemperatura))
                    resultado.RequisitosTemperatura = valor;
                else if ((linhaLower.StartsWith("observações:") || linhaLower.StartsWith("observacoes:")) && string.IsNullOrWhiteSpace(resultado.Cuidados))
                    resultado.Cuidados = valor;
                else if (linhaLower.StartsWith("guia de cuidado") && string.IsNullOrWhiteSpace(resultado.Cuidados))
                    resultado.Cuidados = valor;
            }
        }

        resultado.GarantirConsistenciaToxicidade();
        AplicarRegrasConsistenciaToxicidade(resultado);
        return resultado;
    }

    private static string NormalizarLinha(string linha)
    {
        var normalizada = System.Text.RegularExpressions.Regex.Replace(linha, @"\*{1,3}", "");
        normalizada = System.Text.RegularExpressions.Regex.Replace(normalizada, @"^#+\s*", "");
        normalizada = System.Text.RegularExpressions.Regex.Replace(normalizada, @"^[-•]\s+", "");
        return normalizada.Trim();
    }

    private static string LimparMarkdown(string valor)
    {
        var limpo = System.Text.RegularExpressions.Regex.Replace(valor, @"\*{1,3}", "");
        limpo = System.Text.RegularExpressions.Regex.Replace(limpo, @"`", "");
        return limpo.Trim();
    }

    /// <summary>
    /// Classifica se um texto descreve toxicidade QUÍMICA/BIOLÓGICA real.
    /// Riscos mecânicos (engasgamento, asfixia) NÃO são considerados toxicidade.
    /// </summary>
    private static string ClassificarToxicidade(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return "Não";

        var v = valor.ToLowerInvariant();

        if (v.StartsWith("não") || v.StartsWith("nao") ||
            v.Contains("não é tóxica") || v.Contains("nao e toxica") ||
            v.Contains("não tóxica") || v.Contains("nao toxica") ||
            v.Contains("não é considerada tóxica") ||
            v.Contains("segura para consumo") || v.Contains("seguro para consumo") ||
            v.Contains("não apresenta toxicidade") || v.Contains("sem toxicidade conhecida"))
            return "Não";

        if (v.Contains("tóxica") || v.Contains("toxica") ||
            v.Contains("veneno") || v.Contains("venenosa") ||
            v.Contains("intoxicação") || v.Contains("intoxicacao") ||
            v.Contains("envenenamento") ||
            v.Contains("substância tóxica") || v.Contains("composto tóxico") ||
            v.Contains("alcaloide") || v.Contains("glicosídeo") || v.Contains("oxalato") ||
            v.Contains("saponina") || v.Contains("tanino") ||
            v.Contains("irritação química") || v.Contains("irritacao quimica") ||
            v.Contains("ingestão perigosa") ||
            v.Contains("reação alérgica grave") ||
            v.Contains("fatal se ingerido") || v.Contains("fatal se ingerida"))
            return "Sim";

        return "Não";
    }

    private static void AplicarRegrasConsistenciaToxicidade(DadosPlantaEnriquecidos dados)
    {
        if (dados.Toxica == "Sim")
        {
            if (dados.DescricaoToxicidade?.ToLowerInvariant().StartsWith("não") ?? false)
                dados.DescricaoToxicidade = "Sim. " + dados.DescricaoToxicidade;

            if ((dados.DescricaoToxicidadeAnimais?.ToLowerInvariant().Contains("tóxica") ?? false) ||
                (dados.DescricaoToxicidadeAnimais?.ToLowerInvariant().Contains("toxica") ?? false) ||
                (dados.DescricaoToxicidadeAnimais?.ToLowerInvariant().Contains("veneno") ?? false) ||
                (dados.DescricaoToxicidadeAnimais?.ToLowerInvariant().Contains("intoxicação") ?? false) ||
                (dados.DescricaoToxicidadeAnimais?.ToLowerInvariant().Contains("fatal") ?? false))
                dados.ToxicaAnimais = "Sim";

            dados.ToxicaCriancas = "Sim";

            if (!string.IsNullOrWhiteSpace(dados.DescricaoToxicidadeAnimais) &&
                !dados.DescricaoToxicidadeAnimais.ToLowerInvariant().StartsWith("sim"))
                dados.DescricaoToxicidadeAnimais = "Sim. " + dados.DescricaoToxicidadeAnimais;
        }

        if (dados.ToxicaAnimais == "Sim" && (dados.DescricaoToxicidadeAnimais?.ToLowerInvariant().StartsWith("não") ?? false))
            dados.DescricaoToxicidadeAnimais = "Sim. " + dados.DescricaoToxicidadeAnimais;

        if (dados.ToxicaCriancas == "Sim" && (dados.DescricaoToxicidadeCriancas?.ToLowerInvariant().StartsWith("não") ?? false))
            dados.DescricaoToxicidadeCriancas = "Sim. " + dados.DescricaoToxicidadeCriancas;
    }

    private static string? ExtrairValor(string linha)
    {
        var indice = linha.IndexOf(':');
        return indice >= 0 && indice < linha.Length - 1
            ? linha[(indice + 1)..].Trim()
            : null;
    }
}
