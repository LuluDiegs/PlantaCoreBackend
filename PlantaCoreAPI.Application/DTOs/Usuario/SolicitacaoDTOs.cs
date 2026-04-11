namespace PlantaCoreAPI.Application.DTOs.Usuario;

public class SolicitacaoSeguirDTOSaida
{
    public Guid Id { get; set; }
    public Guid SolicitanteId { get; set; }
    public string NomeSolicitante { get; set; } = null!;
    public string? FotoSolicitante { get; set; }
    public DateTime DataSolicitacao { get; set; }
}
