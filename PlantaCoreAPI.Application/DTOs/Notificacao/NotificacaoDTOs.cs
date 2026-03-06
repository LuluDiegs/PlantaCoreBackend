namespace PlantaCoreAPI.Application.DTOs.Notificacao;

public class NotificacaoDTOSaida
{
    public Guid Id { get; set; }
    public string Tipo { get; set; } = null!;
    public string Mensagem { get; set; } = null!;
    public bool Lida { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataLeitura { get; set; }
    public Guid? UsuarioOrigemId { get; set; }
    public string? UsuarioOrigemNome { get; set; }
    public string? FotoUsuarioOrigem { get; set; }
    public Guid? PostId { get; set; }
    public Guid? PlantaId { get; set; }
}

public class ListarNotificacoesDTOSaida
{
    public List<NotificacaoDTOSaida> Notificacoes { get; set; } = new();
    public int TotalNaoLidas { get; set; }
}
