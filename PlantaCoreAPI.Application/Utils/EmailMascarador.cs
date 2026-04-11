namespace PlantaCoreAPI.Application.Utils;

public static class EmailMascarador
{
    public static string Mascarar(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return "***";
        var arroba = email.IndexOf('@');
        if (arroba <= 0)
            return "***";
        var local = email[..arroba];
        var dominio = email[(arroba + 1)..];
        var localMascarado = local.Length <= 2
            ? new string('*', local.Length)
            : local[..2] + new string('*', local.Length - 2);
        var ponto = dominio.LastIndexOf('.');
        if (ponto <= 0)
            return $"{localMascarado}@***";
        var nomeDominio = dominio[..ponto];
        var tld = dominio[ponto..];
        var dominioMascarado = nomeDominio.Length <= 1
            ? new string('*', nomeDominio.Length)
            : nomeDominio[..1] + new string('*', nomeDominio.Length - 1);
        return $"{localMascarado}@{dominioMascarado}{tld}";
    }
}
