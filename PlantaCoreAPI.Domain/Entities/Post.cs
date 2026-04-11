namespace PlantaCoreAPI.Domain.Entities;

public class Post
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;
    public string Conteudo { get; private set; } = null!;
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }
    public bool Ativo { get; private set; } = true;
    public DateTime? DataExclusao { get; private set; }
    public List<Hashtag> Hashtags { get; set; } = new();
    public List<Categoria> Categorias { get; set; } = new();
    public List<PalavraChave> PalavrasChave { get; set; } = new();
    public Guid? PlantaId { get; private set; }
    public Planta? Planta { get; private set; }
    public Guid? ComunidadeId { get; private set; }
    public Comunidade? Comunidade { get; private set; }
    public List<Comentario> Comentarios { get; set; } = new();
    public List<Curtida> Curtidas { get; set; } = new();
    public int PontuacaoTotal => Curtidas.Count;
    private Post() { }
    public static Post Criar(Guid usuarioId, string conteudo, Guid? plantaId, Guid? comunidadeId)
    {
        if (string.IsNullOrWhiteSpace(conteudo))
            throw new Exceptions.DomainException("Conteúdo do post não pode estar vazio");
        return new Post
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Conteudo = conteudo.Trim(),
            PlantaId = plantaId,
            ComunidadeId = comunidadeId,
            DataCriacao = DateTime.UtcNow,
            Ativo = true
        };
    }

    public void Atualizar(string conteudo)
    {
        if (string.IsNullOrWhiteSpace(conteudo))
            throw new Exceptions.DomainException("Conteúdo do post não pode estar vazio");
        Conteudo = conteudo.Trim();
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Excluir()
    {
        Ativo = false;
        DataExclusao = DateTime.UtcNow;
    }

    public void AdicionarComentario(Comentario comentario)
    {
        Comentarios.Add(comentario);
    }
}
