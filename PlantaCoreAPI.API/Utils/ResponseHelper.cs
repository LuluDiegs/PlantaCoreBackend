namespace PlantaCoreAPI.API.Utils;

public static class ResponseHelper
{
    public static object Padrao<T>(bool sucesso, T dados, object? meta = null, IEnumerable<string>? erros = null)
    {
        return new
        {
            sucesso,
            dados,
            meta = meta ?? new { },
            erros = erros?.ToArray() ?? Array.Empty<string>()
        };
    }
}
