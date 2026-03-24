using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.DTOs.Comunidade;
using PlantaCoreAPI.Application.DTOs.Post;
using PlantaCoreAPI.Application.DTOs.Usuario;
using PlantaCoreAPI.Domain.Comuns;

namespace PlantaCoreAPI.Application.Interfaces;

public interface IComunidadeService
{
    Task<Resultado<ComunidadeDTOSaida>> CriarComunidadeAsync(Guid usuarioId, CriarComunidadeDTOEntrada entrada);
    Task<Resultado<ComunidadeDTOSaida>> AtualizarComunidadeAsync(Guid usuarioId, Guid comunidadeId, AtualizarComunidadeDTOEntrada entrada);
    Task<Resultado> EntrarNaComunidadeAsync(Guid usuarioId, Guid comunidadeId);
    Task<Resultado> SairDaComunidadeAsync(Guid usuarioId, Guid comunidadeId);
    Task<Resultado<ComunidadeDTOSaida>> ObterComunidadeAsync(Guid comunidadeId, Guid usuarioId);
    Task<Resultado<PaginaResultado<ComunidadeDTOSaida>>> ListarComunidadesAsync(int pagina, int tamanho, Guid usuarioId);
    Task<Resultado<IEnumerable<ComunidadeDTOSaida>>> BuscarComunidadesAsync(string termo, Guid usuarioId);
    Task<Resultado<PaginaResultado<ComunidadeDTOSaida>>> ListarComunidadesDoUsuarioAsync(Guid usuarioId, int pagina, int tamanho);
    Task<Resultado<PaginaResultado<PostDTOSaida>>> ObterPostsComunidadeAsync(Guid comunidadeId, Guid usuarioId, int pagina, int tamanho);
    Task<Resultado> ExpulsarUsuarioAsync(Guid adminId, Guid comunidadeId, Guid usuarioId);
    Task<Resultado> ExcluirComunidadeAsync(Guid adminId, Guid comunidadeId);
    Task<Resultado> TransferirAdminAsync(Guid adminId, Guid comunidadeId, Guid novoAdminId);
    Task<IEnumerable<UsuarioListaDTOSaida>> ListarMembrosAsync(Guid comunidadeId);
    Task<IEnumerable<UsuarioListaDTOSaida>> ListarAdminsAsync(Guid comunidadeId);
    Task<bool> SouMembroAsync(Guid comunidadeId, Guid usuarioId);
    Task<Resultado> SolicitarEntradaAsync(Guid comunidadeId, Guid usuarioId);
    Task<IEnumerable<UsuarioListaDTOSaida>> ListarSolicitacoesAsync(Guid comunidadeId);
    Task<Resultado> AprovarSolicitacaoAsync(Guid comunidadeId, Guid usuarioId, Guid adminId);
}
