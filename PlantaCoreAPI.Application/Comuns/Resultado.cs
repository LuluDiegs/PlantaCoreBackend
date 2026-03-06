namespace PlantaCoreAPI.Application.Comuns;

public class Resultado<T>
{
    public bool Sucesso { get; set; }
    public T? Dados { get; set; }
    public string? Mensagem { get; set; }
    public IEnumerable<string>? Erros { get; set; }

    public static Resultado<T> Ok(T dados, string? mensagem = null)
    {
        return new Resultado<T>
        {
            Sucesso = true,
            Dados = dados,
            Mensagem = mensagem
        };
    }

    public static Resultado<T> Erro(string mensagem, IEnumerable<string>? erros = null)
    {
        return new Resultado<T>
        {
            Sucesso = false,
            Mensagem = mensagem,
            Erros = erros
        };
    }
}

public class Resultado
{
    public bool Sucesso { get; set; }
    public string? Mensagem { get; set; }
    public IEnumerable<string>? Erros { get; set; }

    public static Resultado Ok(string? mensagem = null)
    {
        return new Resultado
        {
            Sucesso = true,
            Mensagem = mensagem
        };
    }

    public static Resultado Erro(string mensagem, IEnumerable<string>? erros = null)
    {
        return new Resultado
        {
            Sucesso = false,
            Mensagem = mensagem,
            Erros = erros
        };
    }
}
