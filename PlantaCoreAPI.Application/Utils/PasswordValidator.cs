using System.Text.RegularExpressions;

namespace PlantaCoreAPI.Application.Utils;

public static class PasswordValidator
{
    private static readonly Regex RegexMinuscula = new("[a-z]", RegexOptions.Compiled);
    private static readonly Regex RegexMaiuscula = new("[A-Z]", RegexOptions.Compiled);
    private static readonly Regex RegexNumero = new("[0-9]", RegexOptions.Compiled);
    private static readonly Regex RegexEspecial = new("[!@#$%^&*()_+\\-=\\[\\]{};':\"\\\\|,.<>\\/?]", RegexOptions.Compiled);
    public static bool ValidarComplexidade(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha) || senha.Length < 8)
            return false;
        return RegexMinuscula.IsMatch(senha)
            && RegexMaiuscula.IsMatch(senha)
            && RegexNumero.IsMatch(senha)
            && RegexEspecial.IsMatch(senha);
    }

    public static string ObterMensagemErro(string senha)
    {
        var erros = new List<string>();
        if (string.IsNullOrWhiteSpace(senha))
            return "Senha não pode estar vazia";
        if (senha.Length < 8)
            erros.Add("no mínimo 8 caracteres");
        if (!RegexMinuscula.IsMatch(senha))
            erros.Add("letra minúscula");
        if (!RegexMaiuscula.IsMatch(senha))
            erros.Add("letra maiúscula");
        if (!RegexNumero.IsMatch(senha))
            erros.Add("número");
        if (!RegexEspecial.IsMatch(senha))
            erros.Add("caractere especial");
        if (erros.Count == 0)
            return string.Empty;
        return $"Senha deve conter: {string.Join(", ", erros)}";
    }
}
