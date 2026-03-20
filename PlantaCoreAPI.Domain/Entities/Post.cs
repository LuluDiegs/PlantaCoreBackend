namespace PlantaCoreAPI.Domain.Entities;

public class Post
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!; // Relacionamento com o usuário
    public string Conteudo { get; set; } = null!;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataAtualizacao { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime? DataExclusao { get; set; }
    public int PontuacaoTotal { get; set; }

    // Novas propriedades para indexação
    public List<Hashtag> Hashtags { get; set; } = new();
    public List<Categoria> Categorias { get; set; } = new();
    public List<PalavraChave> PalavrasChave { get; set; } = new();

    public Guid? PlantaId { get; set; } // Relacionamento opcional com uma planta
    public Planta? Planta { get; set; }

    public Guid? ComunidadeId { get; set; } // Relacionamento opcional com uma comunidade
    public Comunidade? Comunidade { get; set; }

    public List<Comentario> Comentarios { get; set; } = new();
    public List<Curtida> Curtidas { get; set; } = new();

    // Método de fábrica para criar um post
    public static Post Criar(Guid usuarioId, string conteudo, Guid? plantaId, Guid? comunidadeId)
    {
        return new Post
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Conteudo = conteudo,
            PlantaId = plantaId,
            ComunidadeId = comunidadeId,
            DataCriacao = DateTime.UtcNow
        };
    }

    // Métodos adicionais
    public void Atualizar(string conteudo)
    {
        Conteudo = conteudo;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Excluir()
    {
        Ativo = false;
        DataExclusao = DateTime.UtcNow;
    }

    public void AdicionarCurtida(Curtida curtida)
    {
        Curtidas.Add(curtida);
        PontuacaoTotal++;
    }

    public void RemoverCurtida(Guid usuarioId)
    {
        var curtida = Curtidas.FirstOrDefault(c => c.UsuarioId == usuarioId);
        if (curtida != null)
        {
            Curtidas.Remove(curtida);
            PontuacaoTotal--;
        }
    }

    public void AdicionarComentario(Comentario comentario)
    {
        Comentarios.Add(comentario);
    }
}
