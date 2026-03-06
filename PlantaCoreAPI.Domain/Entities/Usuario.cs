namespace PlantaCoreAPI.Domain.Entities;

public class Usuario
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string SenhaHash { get; private set; } = null!;
    public string? Biografia { get; private set; }
    public string? FotoPerfil { get; private set; }
    
    public bool EmailConfirmado { get; private set; }
    public string? TokenConfirmacaoEmail { get; private set; }
    public string? TokenResetarSenha { get; private set; }
    public DateTime? DataTokenResetarSenha { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataExclusao { get; private set; }
    public bool Ativo { get; private set; } = true;

    public List<Usuario> Seguindo { get; private set; } = new();
    public List<Usuario> Seguidores { get; private set; } = new();
    public List<Planta> Plantas { get; private set; } = new();
    public List<Post> Posts { get; private set; } = new();
    public List<Notificacao> Notificacoes { get; private set; } = new();

    private Usuario() { }

    public static Usuario Criar(string nome, string email, string senhaHash)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new Exceptions.DomainException("Nome não pode estar vazio");
        
        if (string.IsNullOrWhiteSpace(email))
            throw new Exceptions.DomainException("Email não pode estar vazio");
        
        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new Exceptions.DomainException("Senha não pode estar vazia");

        ValidarEmail(email);

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = nome.Trim(),
            Email = email.ToLower().Trim(),
            SenhaHash = senhaHash,
            DataCriacao = DateTime.UtcNow,
            EmailConfirmado = false,
            TokenConfirmacaoEmail = Guid.NewGuid().ToString()
        };

        return usuario;
    }

    private static void ValidarEmail(string email)
    {
        if (!email.Contains("@") || !email.Contains("."))
            throw new Exceptions.DomainException("Formato de email inválido");
    }

    public bool VerificarSenha(string senha, Func<string, string, bool> verificador)
    {
        try
        {
            return verificador(senha, SenhaHash);
        }
        catch
        {
            return false;
        }
    }

    public void ConfirmarEmail()
    {
        EmailConfirmado = true;
        TokenConfirmacaoEmail = null;
    }

    public void GerarTokenConfirmacaoEmail()
    {
        TokenConfirmacaoEmail = Guid.NewGuid().ToString();
    }

    public void GerarTokenResetarSenha()
    {
        TokenResetarSenha = Guid.NewGuid().ToString();
        DataTokenResetarSenha = DateTime.UtcNow.AddHours(1);
    }

    public void ResetarSenha(string novaSenhaHash)
    {
        if (DataTokenResetarSenha < DateTime.UtcNow)
            throw new Exceptions.DomainException("Token de reset expirou");

        SenhaHash = novaSenhaHash;
        TokenResetarSenha = null;
        DataTokenResetarSenha = null;
    }

    public void TrocarSenha(string novaSenhaHash)
    {
        SenhaHash = novaSenhaHash;
    }

    public void AtualizarPerfil(string? nome = null, string? biografia = null, string? fotoPerfil = null)
    {
        if (!string.IsNullOrWhiteSpace(nome))
            Nome = nome.Trim();

        if (biografia != null)
        {
            if (biografia.Length > 500)
                throw new Exceptions.DomainException("Biografia não pode ter mais de 500 caracteres");
            Biografia = biografia;
        }

        if (!string.IsNullOrWhiteSpace(fotoPerfil))
            FotoPerfil = fotoPerfil;
    }

    public void AtualizarNome(string novoNome)
    {
        if (string.IsNullOrWhiteSpace(novoNome))
            throw new Exceptions.DomainException("Nome não pode estar vazio");
        
        Nome = novoNome.Trim();
    }

    public void AtualizarFotoPerfil(string? urlFoto)
    {
        FotoPerfil = urlFoto;
    }

    public void Excluir()
    {
        DataExclusao = DateTime.UtcNow;
        Ativo = false;
    }

    public void Reativar()
    {
        Ativo = true;
        DataExclusao = null;
    }

    public void Seguir(Usuario usuario)
    {
        if (usuario.Id == Id)
            throw new Exceptions.DomainException("Você não pode seguir a si mesmo");

        if (!Seguindo.Contains(usuario))
            Seguindo.Add(usuario);
    }

    public void DeseguirDe(Usuario usuario)
    {
        Seguindo.Remove(usuario);
    }
}
