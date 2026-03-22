using System.Net;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using PlantaCoreAPI.Application.Interfaces;

namespace PlantaCoreAPI.Infrastructure.Services.External;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly string _modelo;
    private readonly string _baseUrl;
    private readonly List<string> _tokens;
    private int _currentTokenIndex = 0;

    public GeminiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;

        var geminiConfig = configuration.GetSection("Gemini");

        var raw = geminiConfig["ChavesApi"]
            ?? throw new InvalidOperationException("Gemini ChavesApi não configurada");

        _tokens = raw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .ToList();

        _modelo = geminiConfig["Modelo"] ?? "gemini-2.5-flash";
        _baseUrl = geminiConfig["BaseUrl"] ?? "https://generativelanguage.googleapis.com";
    }

    private string GetCurrentToken()
    {
        return _tokens[_currentTokenIndex];
    }

    private void MoveNextToken()
    {
        _currentTokenIndex = (_currentTokenIndex + 1) % _tokens.Count;
    }

    public async Task<string?> GerarDescricaoPlantaAsync(DadosPlantaParaIA dados)
    {
        // Adicionando validação para toxicidade
        if (string.IsNullOrWhiteSpace(dados.Toxicidade))
        {
            dados.Toxicidade = "Informação não disponível";
        }

        if (string.IsNullOrWhiteSpace(dados.Descricao))
        {
            dados.Descricao = "Descrição não fornecida.";
        }

        // Processamento adicional para toxicidade
        if (dados.ToxicoPets.HasValue && dados.ToxicoPets.Value)
        {
            dados.Descricao += " Atenção: Esta planta é tóxica para animais de estimação.";
        }

        // Chamada ao serviço Gemini
        return await EnviarPromptAsync(ConstruirPromptPrincipal(dados));
    }

    public async Task<string?> GerarReflexaoPlantaAsync(DadosPlantaParaIA dados, string respostaPrincipal)
    {
        // Chama o Gemini com o prompt de reflexão, usando a resposta principal
        var promptReflexao = ConstruirPromptReflexao(dados, respostaPrincipal);
        return await EnviarPromptAsync(promptReflexao);
    }

    private async Task<(bool sucesso, string? texto, HttpStatusCode statusCode)> 
        ExecutarPromptGeminiAsync(string token, string prompt)
    {
        var request = new GeminiRequestSimples
        {
            Contents = new List<GeminiContent>
            {
                new GeminiContent
                {
                    Parts = new List<GeminiPart>
                    {
                        new GeminiPart { Text = prompt }
                    }
                }
            }
        };

        var url = $"{_baseUrl}/v1beta/models/{_modelo}:generateContent?key={token}";

        var json = System.Text.Json.JsonSerializer.Serialize(request);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);
        var statusCode = response.StatusCode;

        if (!response.IsSuccessStatusCode)
            return (false, null, statusCode);

        var responseJson = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrEmpty(responseJson))
            return (false, null, statusCode);

        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var resultado = System.Text.Json.JsonSerializer.Deserialize<GeminiResponse>(responseJson, options);

        if (resultado?.Candidates?.Count > 0)
        {
            var texto = resultado.Candidates[0].Content?.Parts?[0]?.Text;
            return (true, texto, statusCode);
        }

        return (false, null, statusCode);
    }

    private async Task<string?> EnviarPromptAsync(string prompt)
    {
        int tentativas = _tokens.Count;

        for (int i = 0; i < tentativas; i++)
        {
            var token = GetCurrentToken();

            Console.WriteLine($"[Gemini] Token atual: {token}");

            var (sucesso, texto, statusCode) = await ExecutarPromptGeminiAsync(token, prompt);

            if (sucesso) return texto;

            if (statusCode is HttpStatusCode.TooManyRequests)
            {
                Console.WriteLine($"[Gemini] Token {token} exaustado. Mudando para o próximo...");

                MoveNextToken();
                continue;
            }

            break;
        }

        return null;
    }

    private string ConstruirPromptPrincipal(DadosPlantaParaIA dados)
    {
        return $@"Você é um especialista em jardinagem, botânica, toxicologia de plantas e biólogo com 20 anos de experiência.

            Planta a pesquisar: {dados.NomeCientifico}

            DEFINIÇÃO CRÍTICA DE TOXICIDADE (LEIA COM ATENÇÃO):
            Toxicidade = presença de compostos químicos ou biológicos nocivos: alcaloides, glicosídeos, oxalatos, saponinas, resinas tóxicas, etc.
            NÃO É toxicidade: risco de engasgamento, alergia leve, irritação de pele por contato físico, gordura em excesso.
            Se a planta NÃO contém compostos químicos tóxicos = responda ""Não"" em toxicidade.
            Se a planta CONTÉM compostos tóxicos = responda ""Sim"" com descrição precisa do composto e efeito.

            REGRAS DE CONSISTÊNCIA:
            - ""Sim"" em humanos → descreva o composto tóxico e o efeito (ex: oxalato de cálcio causa irritação severa)
            - ""Não"" em humanos → confirme que é segura quimicamente
            - Se tóxica para humanos, avalie separadamente para animais e crianças com a mesma rigorosidade
            - NUNCA marque ""Sim"" apenas por risco mecânico (espinho, engasgo, etc.)
            - NUNCA marque ""Sim"" apenas por ser indigesta em excesso

            Responda EXATAMENTE neste formato sem markdown, sem negrito, sem asteriscos:

            Nome científico: [nome científico correto]
            Nome comum: [nome comum em português]
            Família: [família botânica]
            Gênero: [gênero]
            Toxicidade para humanos: [Sim ou Não - descrição do composto tóxico ou confirmação de segurança]
            Toxicidade para animais domésticos: [Sim ou Não - descrição específica para cães e gatos]
            Toxicidade para crianças: [Sim ou Não - descrição específica]
            Luz: [requisitos práticos - ex: Sol pleno, Meia sombra]
            Água: [frequência e quantidade prática]
            Temperatura ideal: [faixa em °C - ex: 18-28°C]
            Observações: [curiosidades e características relevantes]
            Guia de cuidado completo: [mínimo 5 passos práticos]";
    }

    private string ConstruirPromptReflexao(DadosPlantaParaIA dados, string respostaPrincipal)
    {
        return $@"Você é um especialista em toxicologia de plantas e botânica com 20 anos de experiência.

            Planta: {dados.NomeCientifico}

            Resposta anterior:
            {respostaPrincipal}

            VALIDAÇÃO OBRIGATÓRIA:

            1. TOXICIDADE: verifique se cada campo ""Sim"" é justificado por composto químico/biológico tóxico real.
               - Risco de engasgamento NÃO é toxicidade → corrija para ""Não""
               - Gordura em excesso NÃO é toxicidade → corrija para ""Não""
               - Alergia de contato físico NÃO é toxicidade química → corrija para ""Não""
               - Alcaloide, glicosídeo, oxalato, saponina, veneno = toxicidade real → mantenha ""Sim""

            2. CONSISTÊNCIA: ""Sim"" deve ter descrição do composto tóxico. ""Não"" deve confirmar segurança.

            3. FORMATO: sem markdown, sem asteriscos, sem negrito. Texto puro apenas.

            4. Todos os campos devem estar preenchidos.

            Retorne a resposta COMPLETA corrigida no mesmo formato:

            Nome científico: [VALIDADO]
            Nome comum: [VALIDADO]
            Família: [VALIDADO]
            Gênero: [VALIDADO]
            Toxicidade para humanos: [Sim ou Não - APENAS toxicidade química/biológica real]
            Toxicidade para animais domésticos: [Sim ou Não - APENAS toxicidade química/biológica real]
            Toxicidade para crianças: [Sim ou Não - APENAS toxicidade química/biológica real]
            Luz: [VALIDADO]
            Água: [VALIDADO]
            Temperatura ideal: [VALIDADO]
            Observações: [VALIDADO]
            Guia de cuidado completo: [VALIDADO]";
    }
}

internal sealed class GeminiRequestSimples
{
    [JsonPropertyName("contents")]
    public List<GeminiContent> Contents { get; set; } = new();
}

internal sealed class GeminiContent
{
    [JsonPropertyName("parts")]
    public List<GeminiPart> Parts { get; set; } = new();

    [JsonPropertyName("role")]
    public string? Role { get; set; }
}

internal sealed class GeminiPart
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

internal sealed class GeminiResponse
{
    [JsonPropertyName("candidates")]
    public List<GeminiCandidate>? Candidates { get; set; }
}

internal sealed class GeminiCandidate
{
    [JsonPropertyName("content")]
    public GeminiContent? Content { get; set; }
}
