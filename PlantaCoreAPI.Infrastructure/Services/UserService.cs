using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.DTOs.Usuario;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Domain.Comuns;
using PlantaCoreAPI.Domain.Entities;
using PlantaCoreAPI.Domain.Interfaces;

namespace PlantaCoreAPI.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IRepositorioUsuario _repositorioUsuario;
    private readonly IRepositorioPost _repositorioPost;
    private readonly IRepositorioNotificacao _repositorioNotificacao;
    private readonly IFileStorageService _fileStorageService;
    private readonly IAccountDeletionService _accountDeletionService;
    private readonly IAccountReactivationService _accountReactivationService;

    public UserService(
        IRepositorioUsuario repositorioUsuario,
        IRepositorioPost repositorioPost,
        IRepositorioNotificacao repositorioNotificacao,
        IFileStorageService fileStorageService,
        IAccountDeletionService accountDeletionService,
        IAccountReactivationService accountReactivationService)
    {
        _repositorioUsuario = repositorioUsuario;
        _repositorioPost = repositorioPost;
        _repositorioNotificacao = repositorioNotificacao;
        _fileStorageService = fileStorageService;
        _accountDeletionService = accountDeletionService;
        _accountReactivationService = accountReactivationService;
    }

    public async Task<Resultado<UsuarioDTOSaida>> ObterPerfilAsync(Guid usuarioId)
    {
        try
        {
            var usuario = await _repositorioUsuario.ObterComPlantasAsync(usuarioId);
            if (usuario == null)
                return Resultado<UsuarioDTOSaida>.Erro("Usuário não encontrado");

            var totalPosts = await _repositorioPost.ObterPorUsuarioAsync(usuarioId);
            var totalCurtidas = await _repositorioPost.ObterTotalCurtidasRecebidasAsync(usuarioId);

            var dto = new UsuarioDTOSaida
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Biografia = usuario.Biografia,
                FotoPerfil = usuario.FotoPerfil,
                TotalSeguidores = usuario.Seguidores.Count,
                TotalSeguindo = usuario.Seguindo.Count,
                TotalPlantas = usuario.Plantas.Count,
                TotalPosts = totalPosts.Count(),
                TotalCurtidasRecebidas = totalCurtidas,
                DataCriacao = usuario.DataCriacao
            };

            return Resultado<UsuarioDTOSaida>.Ok(dto);
        }
        catch (Exception ex)
        {
            return Resultado<UsuarioDTOSaida>.Erro($"Erro ao obter perfil: {ex.Message}");
        }
    }

    public async Task<Resultado<PerfilPublicoDTOSaida>> ObterPerfilPublicoAsync(Guid usuarioId, Guid usuarioAutenticadoId)
    {
        try
        {
            var usuario = await _repositorioUsuario.ObterComPlantasAsync(usuarioId);
            if (usuario == null)
                return Resultado<PerfilPublicoDTOSaida>.Erro("Usuário não encontrado");

            var usuarioAutenticado = await _repositorioUsuario.ObterPorIdAsync(usuarioAutenticadoId);
            var userSegueEste = usuarioAutenticado?.Seguindo.Any(u => u.Id == usuarioId) ?? false;

            var dto = new PerfilPublicoDTOSaida
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Biografia = usuario.Biografia,
                FotoPerfil = usuario.FotoPerfil,
                TotalSeguidores = usuario.Seguidores.Count,
                TotalSeguindo = usuario.Seguindo.Count,
                TotalPlantas = usuario.Plantas.Count,
                UserSegueEste = userSegueEste
            };

            return Resultado<PerfilPublicoDTOSaida>.Ok(dto);
        }
        catch (Exception ex)
        {
            return Resultado<PerfilPublicoDTOSaida>.Erro($"Erro ao obter perfil público: {ex.Message}");
        }
    }

    public async Task<Resultado> AtualizarPerfilAsync(Guid usuarioId, AtualizarPerfilDTOEntrada entrada)
    {
        try
        {
            var usuario = await _repositorioUsuario.ObterPorIdAsync(usuarioId);
            if (usuario == null)
                return Resultado.Erro("Usuário não encontrado");

            usuario.AtualizarPerfil(entrada.Nome, entrada.Biografia, entrada.UrlFotoPerfil);

            await _repositorioUsuario.AtualizarAsync(usuario);
            await _repositorioUsuario.SalvarMudancasAsync();

            return Resultado.Ok("Perfil atualizado com sucesso");
        }
        catch (Exception ex)
        {
            return Resultado.Erro($"Erro ao atualizar perfil: {ex.Message}");
        }
    }

    public async Task<Resultado> AtualizarNomeAsync(Guid usuarioId, string novoNome)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(novoNome))
                return Resultado.Erro("Nome não pode estar vazio");

            var usuario = await _repositorioUsuario.ObterPorIdAsync(usuarioId);
            if (usuario == null)
                return Resultado.Erro("Usuário não encontrado");

            usuario.AtualizarNome(novoNome);

            await _repositorioUsuario.AtualizarAsync(usuario);
            await _repositorioUsuario.SalvarMudancasAsync();

            return Resultado.Ok("Nome atualizado com sucesso");
        }
        catch (Exception ex)
        {
            return Resultado.Erro($"Erro ao atualizar nome: {ex.Message}");
        }
    }

    public async Task<Resultado> AtualizarFotoPerfilAsync(Guid usuarioId, Stream fotoStream, string nomeArquivo)
    {
        try
        {
            if (fotoStream == null || fotoStream.Length == 0)
                return Resultado.Erro("Nenhuma foto enviada");

            var usuario = await _repositorioUsuario.ObterPorIdAsync(usuarioId);
            if (usuario == null)
                return Resultado.Erro("Usuário não encontrado");

            using var ms = new MemoryStream();
            await fotoStream.CopyToAsync(ms);
            var bytes = ms.ToArray();

            var urlFoto = await _fileStorageService.FazerUploadFotoPerfilAsync(bytes, nomeArquivo, usuarioId);

            if (string.IsNullOrWhiteSpace(urlFoto))
                return Resultado.Erro("Erro ao fazer upload da foto para o servidor");

            usuario.AtualizarFotoPerfil(urlFoto);

            await _repositorioUsuario.AtualizarAsync(usuario);
            await _repositorioUsuario.SalvarMudancasAsync();

            return Resultado.Ok("Foto do perfil atualizada com sucesso");
        }
        catch (Exception ex)
        {
            return Resultado.Erro($"Erro ao atualizar foto do perfil: {ex.Message}");
        }
    }

    public async Task<Resultado> ExcluirContaAsync(Guid usuarioId)
    {
        try
        {
            return await _accountDeletionService.ExcluirContaCompleteAsync(usuarioId);
        }
        catch (Exception ex)
        {
            return Resultado.Erro($"Erro ao excluir conta: {ex.Message}");
        }
    }

    public async Task<Resultado> SolicitarReativacaoAsync(string email)
    {
        return await _accountReactivationService.SolicitarReativacaoAsync(email);
    }

    public async Task<Resultado> ReativarComTokenAsync(string email, string token, string novaSenha)
    {
        return await _accountReactivationService.ReativarComTokenAsync(email, token, novaSenha);
    }

    public async Task<Resultado> VerificarTokenReativacaoAsync(string email, string token)
    {
        return await _accountReactivationService.VerificarTokenReativacaoAsync(email, token);
    }

    public async Task<Resultado> ResetarSenhaSemTokenAsync(string email, string novaSenha)
    {
        return await _accountReactivationService.ResetarSenhaSemTokenAsync(email, novaSenha);
    }

    public async Task<Resultado> SegurUserAsync(Guid usuarioId, Guid usuarioParaSeguirId)
    {
        try
        {
            var usuario = await _repositorioUsuario.ObterPorIdAsync(usuarioId);
            var usuarioParaSeguir = await _repositorioUsuario.ObterPorIdAsync(usuarioParaSeguirId);

            if (usuario == null || usuarioParaSeguir == null)
                return Resultado.Erro("Usuário não encontrado");

            usuario.Seguir(usuarioParaSeguir);
            await _repositorioUsuario.AtualizarAsync(usuario);
            await _repositorioUsuario.SalvarMudancasAsync();

            var notificacao = Notificacao.Criar(
                usuarioParaSeguirId,
                Domain.Enums.TipoNotificacao.NovoSeguidor,
                $"{usuario.Nome} começou a seguir você",
                usuarioId);

            await _repositorioNotificacao.AdicionarAsync(notificacao);
            await _repositorioNotificacao.SalvarMudancasAsync();

            return Resultado.Ok("Usuário seguido com sucesso");
        }
        catch (Exception ex)
        {
            return Resultado.Erro($"Erro ao seguir usuário: {ex.Message}");
        }
    }

    public async Task<Resultado> DesSeguirUserAsync(Guid usuarioId, Guid usuarioParaDesSeguirId)
    {
        try
        {
            var usuario = await _repositorioUsuario.ObterPorIdAsync(usuarioId);
            var usuarioParaDesSeguir = await _repositorioUsuario.ObterPorIdAsync(usuarioParaDesSeguirId);

            if (usuario == null || usuarioParaDesSeguir == null)
                return Resultado.Erro("Usuário não encontrado");

            usuario.DeseguirDe(usuarioParaDesSeguir);
            await _repositorioUsuario.AtualizarAsync(usuario);
            await _repositorioUsuario.SalvarMudancasAsync();

            return Resultado.Ok("Usuário deseguido com sucesso");
        }
        catch (Exception ex)
        {
            return Resultado.Erro($"Erro ao deseguir usuário: {ex.Message}");
        }
    }

    public async Task<Resultado<PaginaResultado<PerfilPublicoDTOSaida>>> ListarSeguidoresAsync(Guid usuarioId, int pagina, int tamanho)
    {
        try
        {
            var pagina_ = await _repositorioUsuario.ObterSeguidoresPaginadoAsync(usuarioId, pagina, tamanho);

            return Resultado<PaginaResultado<PerfilPublicoDTOSaida>>.Ok(new PaginaResultado<PerfilPublicoDTOSaida>
            {
                Itens = pagina_.Itens.Select(MapearPerfilPublico),
                Pagina = pagina_.Pagina,
                TamanhoPagina = pagina_.TamanhoPagina,
                Total = pagina_.Total
            });
        }
        catch (Exception ex)
        {
            return Resultado<PaginaResultado<PerfilPublicoDTOSaida>>.Erro($"Erro ao listar seguidores: {ex.Message}");
        }
    }

    public async Task<Resultado<PaginaResultado<PerfilPublicoDTOSaida>>> ListarSeguindoAsync(Guid usuarioId, int pagina, int tamanho)
    {
        try
        {
            var pagina_ = await _repositorioUsuario.ObterSeguindoPaginadoAsync(usuarioId, pagina, tamanho);

            return Resultado<PaginaResultado<PerfilPublicoDTOSaida>>.Ok(new PaginaResultado<PerfilPublicoDTOSaida>
            {
                Itens = pagina_.Itens.Select(MapearPerfilPublico),
                Pagina = pagina_.Pagina,
                TamanhoPagina = pagina_.TamanhoPagina,
                Total = pagina_.Total
            });
        }
        catch (Exception ex)
        {
            return Resultado<PaginaResultado<PerfilPublicoDTOSaida>>.Erro($"Erro ao listar seguindo: {ex.Message}");
        }
    }

    private static PerfilPublicoDTOSaida MapearPerfilPublico(Domain.Entities.Usuario u) =>
        new()
        {
            Id = u.Id,
            Nome = u.Nome,
            Biografia = u.Biografia,
            FotoPerfil = u.FotoPerfil,
            TotalSeguidores = u.Seguidores.Count,
            TotalSeguindo = u.Seguindo.Count,
            TotalPlantas = u.Plantas.Count
        };
}
