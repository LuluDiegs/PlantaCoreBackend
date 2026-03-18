namespace PlantaCoreAPI.Domain.Entities;

public class Post
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Usuario? Usuario { get; private set; }
    public Guid? PlantaId { get; private set; }
    public Planta? Planta { get; private set; }
    public Guid? ComunidadeId { get; private set; }
    public Comunidade? Comunidade { get; private set; }
    public string Conteudo { get; private set; } = null!;
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }
    public DateTime? DataExclusao { get; private set; }
    public bool Ativo { get; private set; } = true;
    public int PontuacaoTotal { get; private set; }

    private List<Curtida> _curtidas = new();
    private List<Comentario> _comentarios = new();

    public IReadOnlyList<Curtida> Curtidas => _curtidas.AsReadOnly();
    public IReadOnlyList<Comentario> Comentarios => _comentarios.AsReadOnly();

    private Post() { }

    public static Post Criar(Guid usuarioId, string conteudo, Guid? plantaId = null, Guid? comunidadeId = null)
    {
        if (string.IsNullOrWhiteSpace(conteudo))
            throw new Exceptions.DomainException("Conteúdo não pode estar vazio");

        return new Post
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            PlantaId = plantaId,
            ComunidadeId = comunidadeId,
            Conteudo = conteudo.Trim(),
            DataCriacao = DateTime.UtcNow,
            Ativo = true,
            PontuacaoTotal = 0
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

    public void AdicionarCurtida(Usuario usuario)
    {
        if (_curtidas.Any(c => c.UsuarioId == usuario.Id))
            throw new Exceptions.DomainException("Você já curtiu este post");

        _curtidas.Add(Curtida.Criar(Id, usuario.Id));
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

    public void AdicionarComentario(Comentario comentario)
    {
        if (comentario == null)
            throw new Exceptions.DomainException("Comentário não pode ser nulo");

        _comentarios.Add(comentario);
    }
}
