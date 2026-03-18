namespace PlantaCoreAPI.Application.DTOs.Usuario;

public class UsuarioDTOSaida
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Biografia { get; set; }
    public string? FotoPerfil { get; set; }
    public bool PerfilPrivado { get; set; }
    public int TotalSeguidores { get; set; }
    public int TotalSeguindo { get; set; }
    public int TotalPlantas { get; set; }
    public int TotalPosts { get; set; }
    public int TotalCurtidasRecebidas { get; set; }
    public DateTime DataCriacao { get; set; }
}

public class AtualizarPerfilDTOEntrada
{
    public string? Nome { get; set; }
    public string? Biografia { get; set; }
    public string? UrlFotoPerfil { get; set; }
}

public class AtualizarNomeDTOEntrada
{
    public string NovoNome { get; set; } = null!;
}

public class AtualizarBiografiaDTOEntrada
{
    public string Biografia { get; set; } = null!;
}

public class AlterarPrivacidadePerfilDTOEntrada
{
    public bool Privado { get; set; }
}

public class PerfilPublicoDTOSaida
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = null!;
    public string? Biografia { get; set; }
    public string? FotoPerfil { get; set; }
    public bool PerfilPrivado { get; set; }
    public int TotalSeguidores { get; set; }
    public int TotalSeguindo { get; set; }
    public int TotalPlantas { get; set; }
    public int TotalPosts { get; set; }
    public bool UserSegueEste { get; set; }
    public bool SolicitacaoPendente { get; set; }
}
