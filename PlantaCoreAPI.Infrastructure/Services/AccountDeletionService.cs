using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using PlantaCoreAPI.Infrastructure.Dados;

namespace PlantaCoreAPI.Infrastructure.Services;

public class AccountDeletionService : IAccountDeletionService
{
    private readonly IRepositorioUsuario _repositorioUsuario;
    private readonly IRepositorioPost _repositorioPost;
    private readonly IRepositorioCurtida _repositorioCurtida;
    private readonly IRepositorioComentario _repositorioComentario;
    private readonly IRepositorioNotificacao _repositorioNotificacao;
    private readonly IRepositorioPlanta _repositorioPlanta;
    private readonly IFileStorageService _fileStorageService;
    private readonly PlantaCoreDbContext _contexto;

    public AccountDeletionService(
        IRepositorioUsuario repositorioUsuario,
        IRepositorioPost repositorioPost,
        IRepositorioCurtida repositorioCurtida,
        IRepositorioComentario repositorioComentario,
        IRepositorioNotificacao repositorioNotificacao,
        IRepositorioPlanta repositorioPlanta,
        IFileStorageService fileStorageService,
        PlantaCoreDbContext contexto)
    {
        _repositorioUsuario = repositorioUsuario;
        _repositorioPost = repositorioPost;
        _repositorioCurtida = repositorioCurtida;
        _repositorioComentario = repositorioComentario;
        _repositorioNotificacao = repositorioNotificacao;
        _repositorioPlanta = repositorioPlanta;
        _fileStorageService = fileStorageService;
        _contexto = contexto;
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

            await DeletarSeguidoresAsync(usuarioId);

            await DeletarCurtidasUsuarioAsync(usuarioId);

            await DeletarComentariosUsuarioAsync(usuarioId);

            await DeletarPostsUsuarioAsync(usuarioId);

            await DeletarPlantasUsuarioAsync(usuarioId);

            await DeletarTokensRefreshAsync(usuarioId);

            usuario.Excluir();
            await _repositorioUsuario.AtualizarAsync(usuario);
            await _repositorioUsuario.SalvarMudancasAsync();

            return Resultado.Ok("Conta e todos os dados associados foram deletados com sucesso");
        }
        catch (Exception ex)
        {
            return Resultado.Erro($"Erro ao excluir conta: {ex.Message}");
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
            Console.WriteLine($"Erro ao deletar foto de perfil: {ex.Message}");
        }
    }

    private async Task DeletarFotosPlantasAsync(Guid usuarioId)
    {
        try
        {
            var plantas = await _contexto.Plantas
                .Where(p => p.UsuarioId == usuarioId)
                .ToListAsync();

            foreach (var planta in plantas)
            {
                if (!string.IsNullOrWhiteSpace(planta.FotoPlanta))
                {
                    await _fileStorageService.DeletarFotoPlantaAsync(planta.FotoPlanta, usuarioId);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao deletar fotos de plantas: {ex.Message}");
        }
    }

    private async Task DeletarSeguidoresAsync(Guid usuarioId)
    {
        try
        {
            var usuario = await _contexto.Usuarios
                .Include(u => u.Seguidores)
                .Include(u => u.Seguindo)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario != null)
            {
                usuario.Seguidores.Clear();
                usuario.Seguindo.Clear();
                await _repositorioUsuario.AtualizarAsync(usuario);
                await _repositorioUsuario.SalvarMudancasAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao deletar seguidores: {ex.Message}");
        }
    }

    private async Task DeletarCurtidasUsuarioAsync(Guid usuarioId)
    {
        try
        {
            var curtidas = await _contexto.Curtidas
                .Where(c => c.UsuarioId == usuarioId)
                .ToListAsync();

            foreach (var curtida in curtidas)
            {
                await _repositorioCurtida.RemoverAsync(curtida);
            }

            await _repositorioCurtida.SalvarMudancasAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao deletar curtidas: {ex.Message}");
        }
    }

    private async Task DeletarComentariosUsuarioAsync(Guid usuarioId)
    {
        try
        {
            var comentarios = await _contexto.Comentarios
                .Where(c => c.UsuarioId == usuarioId)
                .ToListAsync();

            foreach (var comentario in comentarios)
            {
                await _repositorioComentario.RemoverAsync(comentario);
            }

            await _repositorioComentario.SalvarMudancasAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao deletar comentários: {ex.Message}");
        }
    }

    private async Task DeletarPostsUsuarioAsync(Guid usuarioId)
    {
        try
        {
            var posts = await _contexto.Posts
                .Where(p => p.UsuarioId == usuarioId)
                .ToListAsync();

            foreach (var post in posts)
            {
                await _repositorioPost.RemoverAsync(post);
            }

            await _repositorioPost.SalvarMudancasAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao deletar posts: {ex.Message}");
        }
    }

    private async Task DeletarPlantasUsuarioAsync(Guid usuarioId)
    {
        try
        {
            var plantas = await _contexto.Plantas
                .Where(p => p.UsuarioId == usuarioId)
                .ToListAsync();

            foreach (var planta in plantas)
            {
                await _repositorioPlanta.RemoverAsync(planta);
            }

            await _repositorioPlanta.SalvarMudancasAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao deletar plantas: {ex.Message}");
        }
    }

    private async Task DeletarTokensRefreshAsync(Guid usuarioId)
    {
        try
        {
            var tokens = await _contexto.TokensRefresh
                .Where(t => t.UsuarioId == usuarioId)
                .ToListAsync();

            foreach (var token in tokens)
            {
                _contexto.TokensRefresh.Remove(token);
            }

            await _contexto.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao deletar tokens: {ex.Message}");
        }
    }
}
