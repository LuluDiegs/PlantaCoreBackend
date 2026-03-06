namespace PlantaCoreAPI.Domain.Entities;

public class Comentario
{
    public Guid Id { get; private set; }
    public Guid PostId { get; private set; }
    public Post? Post { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Usuario? Usuario { get; private set; }
    public string Conteudo { get; private set; } = null!;
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }
    public DateTime? DataExclusao { get; private set; }
    public bool Ativo { get; private set; } = true;
    public int PontuacaoTotal { get; private set; }

    private List<Curtida> _curtidas = new();
    public IReadOnlyList<Curtida> Curtidas => _curtidas.AsReadOnly();

    private Comentario() { }

    public static Comentario Criar(Guid postId, Guid usuarioId, string conteudo)
    {
        if (string.IsNullOrWhiteSpace(conteudo))
            throw new Exceptions.DomainException("Conteúdo não pode estar vazio");

        return new Comentario
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            UsuarioId = usuarioId,
            Conteudo = conteudo.Trim(),
            DataCriacao = DateTime.UtcNow,
            Ativo = true
        };
    }

    public void Atualizar(string novoConteudo)
    {
        if (string.IsNullOrWhiteSpace(novoConteudo))
            throw new Exceptions.DomainException("Conteúdo não pode estar vazio");

        Conteudo = novoConteudo.Trim();
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Excluir()
    {
        DataExclusao = DateTime.UtcNow;
        Ativo = false;
    }

    public void AdicionarCurtida(Guid usuarioId)
    {
        if (_curtidas.Any(c => c.UsuarioId == usuarioId))
            throw new Exceptions.DomainException("Você já curtiu este comentário");

        _curtidas.Add(Curtida.CriarParaComentario(Id, usuarioId));
        PontuacaoTotal += 1;
    }

    public void RemoverCurtida(Guid usuarioId)
    {
        var curtida = _curtidas.FirstOrDefault(c => c.UsuarioId == usuarioId);
        if (curtida != null)
        {
            _curtidas.Remove(curtida);
            PontuacaoTotal -= 1;
        }
    }
}
