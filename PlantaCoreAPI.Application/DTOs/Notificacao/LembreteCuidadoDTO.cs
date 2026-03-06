namespace PlantaCoreAPI.Application.DTOs.Notificacao;

public class LembreteCuidadoDTOSaida
{
    public Guid Id { get; set; }
    public Guid PlantaId { get; set; }
    public string NomePlanta { get; set; } = null!;
    public DateTime DataCriacao { get; set; }
    public bool Lida { get; set; }
    public LembreteCuidadoDetalhesDTOSaida Detalhes { get; set; } = null!;
}

public class LembreteCuidadoDetalhesDTOSaida
{
    public string? Rega { get; set; }
    public string? Luz { get; set; }
    public string? Temperatura { get; set; }
    public string? Cuidados { get; set; }
}

public class ListarNotificacoesComLembretesDTOSaida
{
    public List<NotificacaoDTOSaida> NotificacoesSociais { get; set; } = new();
    public List<LembreteCuidadoDTOSaida> Lembretes { get; set; } = new();
    public int TotalNaoLidas { get; set; }
}
