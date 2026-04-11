using Microsoft.Extensions.Logging;
using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Domain.Interfaces;

namespace PlantaCoreAPI.Application.Services;

public class AccountDeletionService : IAccountDeletionService
{
    private readonly IRepositorioUsuario _repositorioUsuario;
    private readonly IRepositorioNotificacao _repositorioNotificacao;
    private readonly IRepositorioPlanta _repositorioPlanta;
    private readonly IFileStorageService _fileStorageService;
    private readonly IRepositorioExclusaoConta _repositorioExclusaoConta;
    private readonly ILogger<AccountDeletionService> _logger;

    public AccountDeletionService(
        IRepositorioUsuario repositorioUsuario,
        IRepositorioNotificacao repositorioNotificacao,
        IRepositorioPlanta repositorioPlanta,
        IFileStorageService fileStorageService,
        IRepositorioExclusaoConta repositorioExclusaoConta,
        ILogger<AccountDeletionService> logger)
    {
        _repositorioUsuario = repositorioUsuario;
        _repositorioNotificacao = repositorioNotificacao;
        _repositorioPlanta = repositorioPlanta;
        _fileStorageService = fileStorageService;
        _repositorioExclusaoConta = repositorioExclusaoConta;
        _logger = logger;
    }

    public async Task<Resultado> ExcluirContaCompleteAsync(Guid usuarioId)
    {
        try
        {
            var usuario = await _repositorioUsuario.ObterPorIdAsync(usuarioId);
            if (usuario == null)
                return Resultado.Erro("Usuário não encontrado");

            await DeletarFotoPerfilAsync(usuario.FotoPerfil, usuarioId);
            await DeletarFotosPlantasAsync(usuarioId);
            await _repositorioNotificacao.DeletarTodasDoUsuarioAsync(usuarioId);
            await _repositorioExclusaoConta.DeletarSeguidoresAsync(usuarioId);
            await _repositorioExclusaoConta.DeletarCurtidasAsync(usuarioId);
            await _repositorioExclusaoConta.DeletarComentariosAsync(usuarioId);
            await _repositorioExclusaoConta.DeletarPostsAsync(usuarioId);
            await _repositorioExclusaoConta.DeletarPlantasAsync(usuarioId);
            await _repositorioExclusaoConta.DeletarTokensRefreshAsync(usuarioId);

            usuario.Excluir();
            await _repositorioUsuario.AtualizarAsync(usuario);
            await _repositorioUsuario.SalvarMudancasAsync();

            return Resultado.Ok("Conta e todos os dados associados foram deletados com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir conta do usuário {UsuarioId}", usuarioId);
            return Resultado.Erro("Erro ao excluir conta. Tente novamente.");
        }
    }

    private async Task DeletarFotoPerfilAsync(string? fotoPerfil, Guid usuarioId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fotoPerfil))
                return;
            await _fileStorageService.DeletarFotoPerfilAsync(fotoPerfil, usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao deletar foto de perfil do usuário {UsuarioId}", usuarioId);
        }
    }

    private async Task DeletarFotosPlantasAsync(Guid usuarioId)
    {
        try
        {
            var fotos = await _repositorioExclusaoConta.ObterFotosDasPlantasAsync(usuarioId);
            foreach (var foto in fotos)
            {
                if (!string.IsNullOrWhiteSpace(foto))
                {
                    await _fileStorageService.DeletarFotoPlantaAsync(foto, usuarioId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao deletar fotos de plantas do usuário {UsuarioId}", usuarioId);
        }
    }
}
