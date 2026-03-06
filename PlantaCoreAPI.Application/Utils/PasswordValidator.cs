using System.Text.RegularExpressions;

namespace PlantaCoreAPI.Application.Utils;

public static class PasswordValidator
{
    public static bool ValidarComplexidade(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha) || senha.Length < 8)
            return false;

        bool temMinuscula = Regex.IsMatch(senha, "[a-z]");
        bool temMaiuscula = Regex.IsMatch(senha, "[A-Z]");
        bool temNumero = Regex.IsMatch(senha, "[0-9]");
        bool temCaractereEspecial = Regex.IsMatch(senha, "[!@#$%^&*()_+\\-=\\[\\]{};':\"\\\\|,.<>\\/?]");

        return temMinuscula && temMaiuscula && temNumero && temCaractereEspecial;
    }

    public static string ObterMensagemErro(string senha)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(senha))
            return "Senha não pode estar vazia";

        if (senha.Length < 8)
            erros.Add("no mínimo 8 caracteres");

        if (!Regex.IsMatch(senha, "[a-z]"))
            erros.Add("letra minúscula");

        if (!Regex.IsMatch(senha, "[A-Z]"))
            erros.Add("letra maiúscula");

        if (!Regex.IsMatch(senha, "[0-9]"))
            erros.Add("número");

        if (!Regex.IsMatch(senha, "[!@#$%^&*()_+\\-=\\[\\]{};':\"\\\\|,.<>\\/?]"))
            erros.Add("caractere especial");

        if (erros.Count == 0)
            return string.Empty;

        return $"Senha deve conter: {string.Join(", ", erros)}";
    }
}
