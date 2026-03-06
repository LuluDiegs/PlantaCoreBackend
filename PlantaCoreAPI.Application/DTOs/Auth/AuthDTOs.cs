namespace PlantaCoreAPI.Application.DTOs.Auth;

public class RegistroDTOEntrada
{
    public string Nome { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Senha { get; set; } = null!;
    public string ConfirmacaoSenha { get; set; } = null!;
}

public class LoginDTOEntrada
{
    public string Email { get; set; } = null!;
    public string Senha { get; set; } = null!;
}

public class LoginDTOSaida
{
    public Guid UsuarioId { get; set; }
    public string Nome { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string TokenAcesso { get; set; } = null!;
    public string TokenRefresh { get; set; } = null!;
}

public class RefreshTokenDTOEntrada
{
    public string TokenRefresh { get; set; } = null!;
}

public class ConfirmarEmailDTOEntrada
{
    public Guid UsuarioId { get; set; }
    public string Token { get; set; } = null!;
}

public class ResetarSenhaDTOEntrada
{
    public string Email { get; set; } = null!;
}

public class NovaSenhaDTOEntrada
{
    public Guid UsuarioId { get; set; }
    public string Token { get; set; } = null!;
    public string NovaSenha { get; set; } = null!;
    public string ConfirmacaoSenha { get; set; } = null!;
}

public class TrocarSenhaDTOEntrada
{
    public string SenhaAtual { get; set; } = null!;
    public string NovaSenha { get; set; } = null!;
    public string ConfirmacaoSenha { get; set; } = null!;
}
