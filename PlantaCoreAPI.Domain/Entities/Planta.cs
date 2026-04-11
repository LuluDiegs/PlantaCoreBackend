namespace PlantaCoreAPI.Domain.Entities;

public class Planta
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Usuario? Usuario { get; private set; }
    public string NomeCientifico { get; private set; } = null!;
    public string? NomeComum { get; private set; }
    public string? Familia { get; private set; }
    public string? Genero { get; private set; }
    public bool Toxica { get; private set; }
    public string? DescricaoToxicidade { get; private set; }
    public bool ToxicaAnimais { get; private set; }
    public string? DescricaoToxicidadeAnimais { get; private set; }
    public bool ToxicaCriancas { get; private set; }
    public string? DescricaoToxicidadeCriancas { get; private set; }
    public string? RequisitosLuz { get; private set; }
    public string? RequisitosAgua { get; private set; }
    public string? RequisitosTemperatura { get; private set; }
    public string? Cuidados { get; private set; }
    public string? FotoPlanta { get; private set; }
    public string? DadosPlantNet { get; private set; }
    public DateTime DataIdentificacao { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public List<Post> Posts { get; private set; } = new();

    private Planta() { }

    public static Planta Criar(
        Guid usuarioId,
        string nomeCientifico,
        string? nomeComum = null,
        string? familia = null,
        string? genero = null,
        bool toxica = false,
        string? descricaoToxicidade = null,
        bool toxicaAnimais = false,
        string? descricaoToxicidadeAnimais = null,
        bool toxicaCriancas = false,
        string? descricaoToxicidadeCriancas = null,
        string? requisitosLuz = null,
        string? requisitosAgua = null,
        string? requisitosTemperatura = null,
        string? cuidados = null,
        string? fotoPlanta = null)
    {
        if (string.IsNullOrWhiteSpace(nomeCientifico))
            throw new Exceptions.DomainException("Nome científico é obrigatório");

        return new Planta
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            NomeCientifico = nomeCientifico.Trim(),
            NomeComum = nomeComum,
            Familia = familia,
            Genero = genero,
            Toxica = toxica,
            DescricaoToxicidade = descricaoToxicidade,
            ToxicaAnimais = toxicaAnimais,
            DescricaoToxicidadeAnimais = descricaoToxicidadeAnimais,
            ToxicaCriancas = toxicaCriancas,
            DescricaoToxicidadeCriancas = descricaoToxicidadeCriancas,
            RequisitosLuz = requisitosLuz,
            RequisitosAgua = requisitosAgua,
            RequisitosTemperatura = requisitosTemperatura,
            Cuidados = cuidados,
            FotoPlanta = fotoPlanta,
            DataIdentificacao = DateTime.UtcNow,
            DataCriacao = DateTime.UtcNow
        };
    }

    public void EnriquecerDados(string? nomeComum, string? familia, string? genero,
        bool toxica, string? descricaoToxicidade, bool toxicaAnimais, string? descricaoToxicidadeAnimais,
        bool toxicaCriancas, string? descricaoToxicidadeCriancas, string? requisitosLuz, string? requisitosAgua,
        string? requisitosTemperatura, string? cuidados, string? fotoPlanta = null)
    {
        NomeComum = nomeComum ?? NomeComum;
        Familia = familia ?? Familia;
        Genero = genero ?? Genero;
        Toxica = toxica;
        DescricaoToxicidade = descricaoToxicidade ?? DescricaoToxicidade;
        ToxicaAnimais = toxicaAnimais;
        DescricaoToxicidadeAnimais = descricaoToxicidadeAnimais ?? DescricaoToxicidadeAnimais;
        ToxicaCriancas = toxicaCriancas;
        DescricaoToxicidadeCriancas = descricaoToxicidadeCriancas ?? DescricaoToxicidadeCriancas;
        RequisitosLuz = requisitosLuz ?? RequisitosLuz;
        RequisitosAgua = requisitosAgua ?? RequisitosAgua;
        RequisitosTemperatura = requisitosTemperatura ?? RequisitosTemperatura;
        Cuidados = cuidados ?? Cuidados;

        if (!string.IsNullOrWhiteSpace(fotoPlanta))
            FotoPlanta = fotoPlanta;
    }
}
