using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.DTOs.Planta;
using PlantaCoreAPI.Application.DTOs.Post;
using PlantaCoreAPI.Application.DTOs.Usuario;
using PlantaCoreAPI.Domain.Comuns;

namespace PlantaCoreAPI.Application.Interfaces;

public interface IUserService
{
    Task<Resultado<UsuarioDTOSaida>> ObterPerfilAsync(Guid usuarioId);
    Task<Resultado<PerfilPublicoDTOSaida>> ObterPerfilPublicoAsync(Guid usuarioId, Guid usuarioAutenticadoId);
    Task<Resultado> AtualizarPerfilAsync(Guid usuarioId, AtualizarPerfilDTOEntrada entrada);
    Task<Resultado> AtualizarNomeAsync(Guid usuarioId, string novoNome);
    Task<Resultado> AtualizarFotoPerfilAsync(Guid usuarioId, Stream fotoStream, string nomeArquivo);
    Task<Resultado> AlterarPrivacidadePerfilAsync(Guid usuarioId, bool privado);
    Task<Resultado> ExcluirContaAsync(Guid usuarioId);
    Task<Resultado> SolicitarReativacaoAsync(string email);
    Task<Resultado> ReativarComTokenAsync(string email, string token, string novaSenha);
    Task<Resultado> VerificarTokenReativacaoAsync(string email, string token);
    Task<Resultado> ResetarSenhaSemTokenAsync(string email, string novaSenha);
    Task<Resultado> SegurUserAsync(Guid usuarioId, Guid usuarioParaSeguirId);
    Task<Resultado> DesSeguirUserAsync(Guid usuarioId, Guid usuarioParaDesSeguirId);
    Task<Resultado> EnviarSolicitacaoSeguirAsync(Guid solicitanteId, Guid alvoId);
    Task<Resultado> AceitarSolicitacaoSeguirAsync(Guid alvoId, Guid solicitacaoId);
    Task<Resultado> RejeitarSolicitacaoSeguirAsync(Guid alvoId, Guid solicitacaoId);
    Task<Resultado<IEnumerable<SolicitacaoSeguirDTOSaida>>> ListarSolicitacoesPendentesAsync(Guid usuarioId);
    Task<Resultado<PaginaResultado<PerfilPublicoDTOSaida>>> ListarSeguidoresAsync(Guid usuarioId, int pagina, int tamanho);
    Task<Resultado<PaginaResultado<PerfilPublicoDTOSaida>>> ListarSeguindoAsync(Guid usuarioId, int pagina, int tamanho);
    Task<Resultado<PaginaResultado<PlantaDTOSaida>>> ListarPlantasUsuarioAsync(Guid usuarioId, Guid usuarioAutenticadoId, int pagina, int tamanho);
    Task<Resultado<PaginaResultado<PostDTOSaida>>> ListarPostsPerfilAsync(Guid usuarioId, Guid usuarioAutenticadoId, int pagina, int tamanho);
}
