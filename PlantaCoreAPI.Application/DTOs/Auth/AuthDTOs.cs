using System.ComponentModel.DataAnnotations;

namespace PlantaCoreAPI.Application.DTOs.Auth;

public class RegistroDTOEntrada
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Nome deve ter entre 2 e 100 caracteres")]
    public string Nome { get; set; } = null!;

    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Senha é obrigatória")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "Senha deve ter entre 8 e 128 caracteres")]
    public string Senha { get; set; } = null!;

    [Required(ErrorMessage = "Confirmação de senha é obrigatória")]
    [Compare("Senha", ErrorMessage = "Senhas não coincidem")]
    public string ConfirmacaoSenha { get; set; } = null!;
}

public class LoginDTOEntrada
{
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Senha é obrigatória")]
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
    [Required(ErrorMessage = "Token de refresh é obrigatório")]
    public string TokenRefresh { get; set; } = null!;
}

public class ConfirmarEmailDTOEntrada
{
    [Required(ErrorMessage = "ID do usuário é obrigatório")]
    public Guid UsuarioId { get; set; }

    [Required(ErrorMessage = "Token é obrigatório")]
    public string Token { get; set; } = null!;
}

public class ResetarSenhaDTOEntrada
{
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = null!;
}

public class NovaSenhaDTOEntrada
{
    [Required(ErrorMessage = "ID do usuário é obrigatório")]
    public Guid UsuarioId { get; set; }

    [Required(ErrorMessage = "Token é obrigatório")]
    public string Token { get; set; } = null!;

    [Required(ErrorMessage = "Nova senha é obrigatória")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "Senha deve ter entre 8 e 128 caracteres")]
    public string NovaSenha { get; set; } = null!;

    [Required(ErrorMessage = "Confirmação de senha é obrigatória")]
    [Compare("NovaSenha", ErrorMessage = "Senhas não coincidem")]
    public string ConfirmacaoSenha { get; set; } = null!;
}

public class TrocarSenhaDTOEntrada
{
    [Required(ErrorMessage = "Senha atual é obrigatória")]
    public string SenhaAtual { get; set; } = null!;

    [Required(ErrorMessage = "Nova senha é obrigatória")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "Senha deve ter entre 8 e 128 caracteres")]
    public string NovaSenha { get; set; } = null!;

    [Required(ErrorMessage = "Confirmação de senha é obrigatória")]
    [Compare("NovaSenha", ErrorMessage = "Senhas não coincidem")]
    public string ConfirmacaoSenha { get; set; } = null!;
}
public class LoginGoogleDTOEntrada
{
    [Required(ErrorMessage = "O token do Google é obrigatório")]
    public string TokenGoogle { get; set; } = null!;
}
