namespace PlantaCoreAPI.Application.DTOs.Notificacao;

public class ConfiguracoesNotificacaoDTO
{
    public bool ReceberCurtidas { get; set; } = true;
    public bool ReceberComentarios { get; set; } = true;
    public bool ReceberNovoSeguidor { get; set; } = true;
    public bool ReceberSolicitacaoSeguir { get; set; } = true;
    public bool ReceberSolicitacaoAceita { get; set; } = true;
    public bool ReceberEvento { get; set; } = true;
    public bool ReceberPlantaCuidado { get; set; } = true;
    public bool ReceberPlantaIdentificada { get; set; } = true;
}

public class ConfiguracoesNotificacaoDTOSaida : ConfiguracoesNotificacaoDTO { }
public class ConfiguracoesNotificacaoDTOEntrada : ConfiguracoesNotificacaoDTO { }
