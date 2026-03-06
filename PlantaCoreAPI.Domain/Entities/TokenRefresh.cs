namespace PlantaCoreAPI.Domain.Entities;

public class TokenRefresh
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Usuario? Usuario { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime DataExpiracao { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public bool Revogado { get; private set; }
    public DateTime? DataRevogacao { get; private set; }

    private TokenRefresh() { }

    public static TokenRefresh Criar(Guid usuarioId, string token, int diasValidade = 7)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new Exceptions.DomainException("Token não pode estar vazio");

        var tokenRefresh = new TokenRefresh
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Token = token,
            DataCriacao = DateTime.UtcNow,
            DataExpiracao = DateTime.UtcNow.AddDays(diasValidade),
            Revogado = false
        };

        return tokenRefresh;
    }

    public void Revogar()
    {
        Revogado = true;
        DataRevogacao = DateTime.UtcNow;
    }

    public bool EstaValido()
    {
        return !Revogado && DataExpiracao > DateTime.UtcNow;
    }
}
