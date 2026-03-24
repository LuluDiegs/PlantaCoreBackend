namespace PlantaCoreAPI.Domain.Entities;

public class Comunidade
{
    public Guid Id { get; private set; }
    public Guid CriadorId { get; private set; }
    public Usuario? Criador { get; private set; }
    public string Nome { get; private set; } = null!;
    public string? Descricao { get; private set; }
    public string? FotoComunidade { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public bool Ativa { get; private set; } = true;
    public bool Privada { get; private set; } = false;

    public List<MembroComunidade> Membros { get; private set; } = new();
    public List<Post> Posts { get; private set; } = new();

    private Comunidade() { }

    public static Comunidade Criar(Guid criadorId, string nome, string? descricao = null, bool privada = false)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new Exceptions.DomainException("Nome da comunidade não pode estar vazio");

        if (nome.Length > 100)
            throw new Exceptions.DomainException("Nome da comunidade não pode ter mais de 100 caracteres");

        return new Comunidade
        {
            Id = Guid.NewGuid(),
            CriadorId = criadorId,
            Nome = nome.Trim(),
            Descricao = descricao?.Trim(),
            DataCriacao = DateTime.UtcNow,
            Ativa = true,
            Privada = privada
        };
    }

    public void Atualizar(string? nome = null, string? descricao = null, string? fotoComunidade = null, bool? privada = null)
    {
        if (!string.IsNullOrWhiteSpace(nome))
        {
            if (nome.Length > 100)
                throw new Exceptions.DomainException("Nome da comunidade não pode ter mais de 100 caracteres");
            Nome = nome.Trim();
        }

        if (descricao != null)
            Descricao = descricao.Trim();

        if (!string.IsNullOrWhiteSpace(fotoComunidade))
            FotoComunidade = fotoComunidade;

        if (privada.HasValue)
            Privada = privada.Value;
    }

    public void Desativar()
    {
        Ativa = false;
    }

    public void TransferirAdmin(Guid novoAdminId)
    {
        CriadorId = novoAdminId;
    }
}
