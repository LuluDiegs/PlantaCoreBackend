namespace PlantaCoreAPI.Domain.Interfaces;

public interface IRepositorioSolicitacaoSeguir : IRepositorio<Entities.SolicitacaoSeguir>
{
    Task<Entities.SolicitacaoSeguir?> ObterPendentePorParAsync(Guid solicitanteId, Guid alvoId);
    Task<IEnumerable<Entities.SolicitacaoSeguir>> ObterPendentesPorAlvoAsync(Guid alvoId);
    Task<IEnumerable<Entities.SolicitacaoSeguir>> ObterPendentesPorSolicitanteAsync(Guid solicitanteId);
    Task<bool> ExisteSolicitacaoPendenteAsync(Guid solicitanteId, Guid alvoId);
}
