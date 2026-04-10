namespace PlantaCoreAPI.Application.Utils;

public static class ExcecaoTransienteHelper
{
    private static readonly string[] MensagensTransientes =
    [
        "transient failure",
        "timeout",
        "connection reset",
        "broken pipe",
        "exception while reading from stream",
        "an exception has been raised"
    ];

    public static bool EhTransiente(Exception ex)
    {
        var msg = ex.Message.ToLowerInvariant();
        if (MensagensTransientes.Any(t => msg.Contains(t))) return true;
        if (ex.InnerException != null) return EhTransiente(ex.InnerException);
        return false;
    }

    public static void RelancaSeFoiTransiente(Exception ex)
    {
        if (EhTransiente(ex))
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
    }
}
