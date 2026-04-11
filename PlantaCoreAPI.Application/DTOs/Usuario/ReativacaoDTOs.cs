namespace PlantaCoreAPI.Application.DTOs.Usuario;

public class SolicitarReativacaoDTOEntrada
{
    public string Email { get; set; } = null!;
}

public class ReativarComTokenDTOEntrada
{
    public string Email { get; set; } = null!;
    public string Token { get; set; } = null!;
    public string NovaSenha { get; set; } = null!;
}

public class VerificarTokenReativacaoDTOEntrada
{
    public string Email { get; set; } = null!;
    public string Token { get; set; } = null!;
}
