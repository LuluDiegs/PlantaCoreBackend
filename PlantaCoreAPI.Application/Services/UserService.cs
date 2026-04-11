using Microsoft.Extensions.Logging;
using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.Comuns.Cache;
using PlantaCoreAPI.Application.Comuns.Eventos;
using PlantaCoreAPI.Application.DTOs.Planta;
using PlantaCoreAPI.Application.DTOs.Post;
using PlantaCoreAPI.Application.DTOs.Usuario;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Application.Utils;
using PlantaCoreAPI.Domain.Comuns;
using PlantaCoreAPI.Domain.Entities;
using PlantaCoreAPI.Domain.Interfaces;

namespace PlantaCoreAPI.Application.Services;

public class UserService : IUserService
{
    private readonly IRepositorioUsuario _repositorioUsuario;
    private readonly IRepositorioPost _repositorioPost;
    private readonly IRepositorioPlanta _repositorioPlanta;
    private readonly IRepositorioNotificacao _repositorioNotificacao;
    private readonly IRepositorioSolicitacaoSeguir _repositorioSolicitacao;
    private readonly IFileStorageService _fileStorageService;
    private readonly IAccountDeletionService _accountDeletionService;
    private readonly IAccountReactivationService _accountReactivationService;
    private readonly IEventoDispatcher _eventoDispatcher;
    private readonly ICacheService _cacheService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IRepositorioUsuario repositorioUsuario,
        IRepositorioPost repositorioPost,
        IRepositorioPlanta repositorioPlanta,
        IRepositorioNotificacao repositorioNotificacao,
        IRepositorioSolicitacaoSeguir repositorioSolicitacao,
        IFileStorageService fileStorageService,
        IAccountDeletionService accountDeletionService,
        IAccountReactivationService accountReactivationService,
        IEventoDispatcher eventoDispatcher,
        ICacheService cacheService,
        ILogger<UserService> logger)
    {
        _repositorioUsuario = repositorioUsuario;
        _repositorioPost = repositorioPost;
        _repositorioPlanta = repositorioPlanta;
        _repositorioNotificacao = repositorioNotificacao;
        _repositorioSolicitacao = repositorioSolicitacao;
        _fileStorageService = fileStorageService;
        _accountDeletionService = accountDeletionService;
        _accountReactivationService = accountReactivationService;
        _eventoDispatcher = eventoDispatcher;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Resultado<UsuarioDTOSaida>> ObterPerfilAsync(Guid usuarioId)
    {
        var cacheKey = $"perfil:{usuarioId}";
        var cached = _cacheService.Get<UsuarioDTOSaida>(cacheKey);
        if (cached != null)
            return Resultado<UsuarioDTOSaida>.Ok(cached);
        try
        {
            var usuario = await _repositorioUsuario.ObterComPlantasAsync(usuarioId);
            if (usuario == null)
                return Resultado<UsuarioDTOSaida>.Erro("Usuário não encontrado");
            var totalPosts = await _repositorioPost.ContarPorUsuarioAsync(usuarioId);
            var totalCurtidas = await _repositorioPost.ObterTotalCurtidasRecebidasAsync(usuarioId);
            var result = Resultado<UsuarioDTOSaida>.Ok(new UsuarioDTOSaida
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Biografia = usuario.Biografia,
                FotoPerfil = usuario.FotoPerfil,
                PerfilPrivado = usuario.PerfilPrivado,
                TotalSeguidores = usuario.Seguidores.Count,
                TotalSeguindo = usuario.Seguindo.Count,
                TotalPlantas = usuario.Plantas.Count,
                TotalPosts = totalPosts,
                TotalCurtidasRecebidas = totalCurtidas,
                DataCriacao = usuario.DataCriacao
            });
            _cacheService.Set(cacheKey, result.Dados, TimeSpan.FromMinutes(2));
            return result;
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            return Resultado<UsuarioDTOSaida>.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado<PerfilPublicoDTOSaida>> ObterPerfilPublicoAsync(Guid usuarioId, Guid usuarioAutenticadoId)
    {
        try
        {
            var usuario = await _repositorioUsuario.ObterComPlantasAsync(usuarioId);
            if (usuario == null)
                return Resultado<PerfilPublicoDTOSaida>.Erro("Usuário não encontrado");
            var userSegueEste = false;
            var solicitacaoPendente = false;
            if (usuarioAutenticadoId != Guid.Empty)
            {
                var usuarioAutenticado = await _repositorioUsuario.ObterComPlantasAsync(usuarioAutenticadoId);
                userSegueEste = usuarioAutenticado?.Seguindo.Any(u => u.Id == usuarioId) ?? false;
                if (!userSegueEste && usuario.PerfilPrivado)
                    solicitacaoPendente = await _repositorioSolicitacao.ExisteSolicitacaoPendenteAsync(usuarioAutenticadoId, usuarioId);
            }

            var totalPosts = await _repositorioPost.ContarPorUsuarioAsync(usuarioId);
            return Resultado<PerfilPublicoDTOSaida>.Ok(new PerfilPublicoDTOSaida
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Biografia = usuario.Biografia,
                FotoPerfil = usuario.FotoPerfil,
                PerfilPrivado = usuario.PerfilPrivado,
                TotalSeguidores = usuario.Seguidores.Count,
                TotalSeguindo = usuario.Seguindo.Count,
                TotalPlantas = usuario.Plantas.Count,
                TotalPosts = totalPosts,
                UserSegueEste = userSegueEste,
                SolicitacaoPendente = solicitacaoPendente
            });
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao obter perfil público do usuário {UsuarioId}", usuarioId);
            return Resultado<PerfilPublicoDTOSaida>.Erro("Ocorreu um erro interno. Tente novamente.");
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
            _cacheService.Remove($"perfil:{usuarioId}");
            return Resultado.Ok("Perfil atualizado com sucesso");
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao atualizar perfil do usuário {UsuarioId}", usuarioId);
            return Resultado.Erro("Ocorreu um erro interno. Tente novamente.");
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
            _cacheService.Remove($"perfil:{usuarioId}");
            return Resultado.Ok("Nome atualizado com sucesso");
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao atualizar nome do usuário {UsuarioId}", usuarioId);
            return Resultado.Erro("Ocorreu um erro interno. Tente novamente.");
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
            _cacheService.Remove($"perfil:{usuarioId}");
            return Resultado.Ok("Foto do perfil atualizada com sucesso");
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao atualizar foto de perfil do usuário {UsuarioId}", usuarioId);
            return Resultado.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado> AlterarPrivacidadePerfilAsync(Guid usuarioId, bool privado)
    {
        try
        {
            var usuario = await _repositorioUsuario.ObterPorIdAsync(usuarioId);
            if (usuario == null)
                return Resultado.Erro("Usuário não encontrado");
            usuario.AlterarPrivacidadePerfil(privado);
            await _repositorioUsuario.AtualizarAsync(usuario);
            await _repositorioUsuario.SalvarMudancasAsync();
            _cacheService.Remove($"perfil:{usuarioId}");
            return Resultado.Ok(privado ? "Perfil alterado para privado" : "Perfil alterado para público");
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao alterar privacidade do perfil do usuário {UsuarioId}", usuarioId);
            return Resultado.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado> ExcluirContaAsync(Guid usuarioId)
    {
        try
        {
            var usuario = await _repositorioUsuario.ObterPorIdAsync(usuarioId);
            if (usuario == null)
                return Resultado.Erro("Usuário não encontrado");
            usuario.Excluir();
            await _repositorioUsuario.AtualizarAsync(usuario);
            await _repositorioUsuario.SalvarMudancasAsync();
            _cacheService.Remove($"perfil:{usuarioId}");
            return Resultado.Ok("Conta marcada como excluída (soft delete)");
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao excluir conta do usuário {UsuarioId}", usuarioId);
            return Resultado.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado> SolicitarReativacaoAsync(string email) =>
        await _accountReactivationService.SolicitarReativacaoAsync(email);

    public async Task<Resultado> ReativarComTokenAsync(string email, string token, string novaSenha) =>
        await _accountReactivationService.ReativarComTokenAsync(email, token, novaSenha);

    public async Task<Resultado> VerificarTokenReativacaoAsync(string email, string token) =>
        await _accountReactivationService.VerificarTokenReativacaoAsync(email, token);

    public async Task<Resultado> SegurUserAsync(Guid usuarioId, Guid usuarioParaSeguirId)
    {
        try
        {
            _logger.LogInformation("Usuário {UsuarioId} tentando seguir {SeguidoId}", usuarioId, usuarioParaSeguirId);
            if (usuarioId == usuarioParaSeguirId)
                return Resultado.Erro("Você não pode seguir a si mesmo");
            var usuario = await _repositorioUsuario.ObterComSeguindoESeguidoresAsync(usuarioId);
            var usuarioParaSeguir = await _repositorioUsuario.ObterComSeguindoESeguidoresAsync(usuarioParaSeguirId);
            if (usuario == null || usuarioParaSeguir == null)
                return Resultado.Erro("Usuário não encontrado");
            var jaSegue = usuario.Seguindo.Any(u => u.Id == usuarioParaSeguirId);
            if (jaSegue)
                return Resultado.Erro("Você já segue este usuário");
            if (usuarioParaSeguir.PerfilPrivado)
                return Resultado.Erro("Este perfil é privado. Use enviar solicitação de seguir.");
            usuario.Seguir(usuarioParaSeguir);
            await _repositorioUsuario.SalvarMudancasAsync();
            await _eventoDispatcher.PublicarAsync(new UsuarioSeguidoEvento { SeguidorId = usuarioId, SeguidoId = usuarioParaSeguirId });
            var notificacao = Notificacao.Criar(
                usuarioParaSeguirId,
                Domain.Enums.TipoNotificacao.NovoSeguidor,
                $"{usuario.Nome} começou a seguir você",
                usuarioId);
            await _repositorioNotificacao.AdicionarAsync(notificacao);
            await _repositorioNotificacao.SalvarMudancasAsync();
            _logger.LogInformation("Usuário {UsuarioId} seguiu {SeguidoId}", usuarioId, usuarioParaSeguirId);
            return Resultado.Ok("Usuário seguido com sucesso");
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao seguir usuário {UsuarioId} -> {SeguidoId}", usuarioId, usuarioParaSeguirId);
            return Resultado.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado> DesSeguirUserAsync(Guid usuarioId, Guid usuarioParaDesSeguirId)
    {
        try
        {
            var usuario = await _repositorioUsuario.ObterComSeguindoESeguidoresAsync(usuarioId);
            var usuarioParaDesSeguir = await _repositorioUsuario.ObterComSeguindoESeguidoresAsync(usuarioParaDesSeguirId);
            if (usuario == null || usuarioParaDesSeguir == null)
                return Resultado.Erro("Usuário não encontrado");
            var jaSegue = usuario.Seguindo.Any(u => u.Id == usuarioParaDesSeguirId);
            if (!jaSegue)
                return Resultado.Erro("Você não segue este usuário");
            usuario.DeseguirDe(usuarioParaDesSeguir);
            await _repositorioUsuario.SalvarMudancasAsync();
            return Resultado.Ok("Usuário deseguido com sucesso");
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao deseguir usuário {UsuarioId} -> {DeseguidoId}", usuarioId, usuarioParaDesSeguirId);
            return Resultado.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado> EnviarSolicitacaoSeguirAsync(Guid solicitanteId, Guid alvoId)
    {
        try
        {
            if (solicitanteId == alvoId)
                return Resultado.Erro("Você não pode enviar solicitação para si mesmo");
            var solicitante = await _repositorioUsuario.ObterPorIdAsync(solicitanteId);
            var alvo = await _repositorioUsuario.ObterPorIdAsync(alvoId);
            if (solicitante == null || alvo == null)
                return Resultado.Erro("Usuário não encontrado");
            if (!alvo.PerfilPrivado)
                return Resultado.Erro("Este perfil é público. Use seguir diretamente.");
            var jaSegue = await _repositorioUsuario.UsuarioSegueAsync(solicitanteId, alvoId);
            if (jaSegue)
                return Resultado.Erro("Você já segue este usuário");
            var jaExiste = await _repositorioSolicitacao.ExisteSolicitacaoPendenteAsync(solicitanteId, alvoId);
            if (jaExiste)
                return Resultado.Erro("Já existe uma solicitação pendente");
            var solicitacao = SolicitacaoSeguir.Criar(solicitanteId, alvoId);
            await _repositorioSolicitacao.AdicionarAsync(solicitacao);
            await _repositorioSolicitacao.SalvarMudancasAsync();
            var notificacao = Notificacao.Criar(
                alvoId,
                Domain.Enums.TipoNotificacao.PedidoSeguir,
                $"{solicitante.Nome} quer seguir você",
                solicitanteId);
            await _repositorioNotificacao.AdicionarAsync(notificacao);
            await _repositorioNotificacao.SalvarMudancasAsync();
            return Resultado.Ok("Solicitação de seguir enviada com sucesso");
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao enviar solicitação de seguir {SolicitanteId} -> {AlvoId}", solicitanteId, alvoId);
            return Resultado.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado> AceitarSolicitacaoSeguirAsync(Guid alvoId, Guid solicitacaoId)
    {
        try
        {
            var solicitacao = await _repositorioSolicitacao.ObterPorIdAsync(solicitacaoId);
            if (solicitacao == null || !solicitacao.Pendente)
                return Resultado.Erro("Solicitação não encontrada ou já processada");
            if (solicitacao.AlvoId != alvoId)
                return Resultado.Erro("Sem permissão para aceitar esta solicitação");
            solicitacao.Aceitar();
            var solicitante = await _repositorioUsuario.ObterComSeguindoESeguidoresAsync(solicitacao.SolicitanteId);
            var alvoUsuario = await _repositorioUsuario.ObterComSeguindoESeguidoresAsync(alvoId);
            if (solicitante != null && alvoUsuario != null)
            {
                solicitante.Seguir(alvoUsuario);
                await _eventoDispatcher.PublicarAsync(new UsuarioSeguidoEvento { SeguidorId = solicitacao.SolicitanteId, SeguidoId = alvoId });
                var notificacao = Notificacao.Criar(
                    solicitacao.SolicitanteId,
                    Domain.Enums.TipoNotificacao.PedidoSeguirAceito,
                    $"{alvoUsuario.Nome} aceitou seu pedido de seguir",
                    alvoId);
                await _repositorioNotificacao.AdicionarAsync(notificacao);
            }

            await _repositorioSolicitacao.SalvarMudancasAsync();
            return Resultado.Ok("Solicitação aceita com sucesso");
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao aceitar solicitação {SolicitacaoId}", solicitacaoId);
            return Resultado.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado> RejeitarSolicitacaoSeguirAsync(Guid alvoId, Guid solicitacaoId)
    {
        try
        {
            var solicitacao = await _repositorioSolicitacao.ObterPorIdAsync(solicitacaoId);
            if (solicitacao == null || !solicitacao.Pendente)
                return Resultado.Erro("Solicitação não encontrada ou já processada");
            if (solicitacao.AlvoId != alvoId)
                return Resultado.Erro("Sem permissão para rejeitar esta solicitação");
            solicitacao.Rejeitar();
            await _repositorioSolicitacao.AtualizarAsync(solicitacao);
            await _repositorioSolicitacao.SalvarMudancasAsync();
            return Resultado.Ok("Solicitação rejeitada");
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao rejeitar solicitação {SolicitacaoId}", solicitacaoId);
            return Resultado.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado<IEnumerable<SolicitacaoSeguirDTOSaida>>> ListarSolicitacoesPendentesAsync(Guid usuarioId)
    {
        try
        {
            var solicitacoes = await _repositorioSolicitacao.ObterPendentesPorAlvoAsync(usuarioId);
            var dtos = solicitacoes.Select(s => new SolicitacaoSeguirDTOSaida
            {
                Id = s.Id,
                SolicitanteId = s.SolicitanteId,
                NomeSolicitante = s.Solicitante?.Nome ?? string.Empty,
                FotoSolicitante = s.Solicitante?.FotoPerfil,
                DataSolicitacao = s.DataSolicitacao
            });
            return Resultado<IEnumerable<SolicitacaoSeguirDTOSaida>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao listar solicitações pendentes do usuário {UsuarioId}", usuarioId);
            return Resultado<IEnumerable<SolicitacaoSeguirDTOSaida>>.Erro("Ocorreu um erro interno. Tente novamente.");
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
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao listar seguidores do usuário {UsuarioId}", usuarioId);
            return Resultado<PaginaResultado<PerfilPublicoDTOSaida>>.Erro("Ocorreu um erro interno. Tente novamente.");
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
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao listar seguindo do usuário {UsuarioId}", usuarioId);
            return Resultado<PaginaResultado<PerfilPublicoDTOSaida>>.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado<PaginaResultado<PlantaDTOSaida>>> ListarPlantasUsuarioAsync(Guid usuarioId, Guid usuarioAutenticadoId, int pagina, int tamanho)
    {
        try
        {
            var alvo = await _repositorioUsuario.ObterComPlantasAsync(usuarioId);
            if (alvo == null)
                return Resultado<PaginaResultado<PlantaDTOSaida>>.Erro("Usuário não encontrado");
            if (alvo.PerfilPrivado && usuarioAutenticadoId != usuarioId)
            {
                var segue = alvo.Seguidores.Any(s => s.Id == usuarioAutenticadoId);
                if (!segue)
                    return Resultado<PaginaResultado<PlantaDTOSaida>>.Erro("Este perfil é privado");
            }

            var paginaPlantas = await _repositorioPlanta.ObterPorUsuarioPaginadoAsync(usuarioId, pagina, tamanho);
            var itens = paginaPlantas.Itens.Select(MapearPlanta).ToList();
            return Resultado<PaginaResultado<PlantaDTOSaida>>.Ok(new PaginaResultado<PlantaDTOSaida>
            {
                Itens = itens,
                Pagina = paginaPlantas.Pagina,
                TamanhoPagina = paginaPlantas.TamanhoPagina,
                Total = paginaPlantas.Total
            });
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao listar plantas do usuário {UsuarioId}", usuarioId);
            return Resultado<PaginaResultado<PlantaDTOSaida>>.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado<PaginaResultado<PostDTOSaida>>> ListarPostsPerfilAsync(Guid usuarioId, Guid usuarioAutenticadoId, int pagina, int tamanho)
    {
        try
        {
            var alvo = await _repositorioUsuario.ObterComPlantasAsync(usuarioId);
            if (alvo == null)
                return Resultado<PaginaResultado<PostDTOSaida>>.Erro("Usuário não encontrado");
            if (alvo.PerfilPrivado && usuarioAutenticadoId != usuarioId)
            {
                var segue = alvo.Seguidores.Any(s => s.Id == usuarioAutenticadoId);
                if (!segue)
                    return Resultado<PaginaResultado<PostDTOSaida>>.Erro("Este perfil é privado");
            }

            var paginaPosts = await _repositorioPost.ObterPorUsuarioPaginadoAsync(usuarioId, pagina, tamanho, null);
            var itens = paginaPosts.Itens
                .Where(p => p.Usuario != null)
                .Select(p => new PostDTOSaida
                {
                    Id = p.Id,
                    PlantaId = p.PlantaId,
                    ComunidadeId = p.ComunidadeId,
                    UsuarioId = p.UsuarioId,
                    NomeUsuario = p.Usuario!.Nome,
                    FotoUsuario = p.Usuario.FotoPerfil,
                    Conteudo = p.Conteudo,
                    TotalCurtidas = p.Curtidas.Count,
                    TotalComentarios = p.Comentarios.Count,
                    CurtiuUsuario = p.Curtidas.Any(c => c.UsuarioId == usuarioAutenticadoId),
                    DataCriacao = p.DataCriacao,
                    DataAtualizacao = p.DataAtualizacao
                })
                .ToList();
            return Resultado<PaginaResultado<PostDTOSaida>>.Ok(new PaginaResultado<PostDTOSaida>
            {
                Itens = itens,
                Pagina = paginaPosts.Pagina,
                TamanhoPagina = paginaPosts.TamanhoPagina,
                Total = paginaPosts.Total
            });
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao listar posts do perfil {UsuarioId}", usuarioId);
            return Resultado<PaginaResultado<PostDTOSaida>>.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado<RelacaoUsuarioDTOSaida>> ObterRelacaoUsuarioAsync(Guid usuarioId, Guid usuarioAlvoId)
    {
        try
        {
            if (usuarioId == usuarioAlvoId)
                return Resultado<RelacaoUsuarioDTOSaida>.Ok(new RelacaoUsuarioDTOSaida
                {
                    Seguindo = false,
                    SegueVoce = false,
                    SolicitacaoPendente = false
                });
            var usuario = await _repositorioUsuario.ObterComPlantasAsync(usuarioId);
            var alvo = await _repositorioUsuario.ObterComPlantasAsync(usuarioAlvoId);
            if (usuario == null || alvo == null)
                return Resultado<RelacaoUsuarioDTOSaida>.Erro("Usuário não encontrado");
            var seguir = usuario.Seguindo.Any(u => u.Id == usuarioAlvoId);
            var segueVoce = usuario.Seguidores.Any(u => u.Id == usuarioAlvoId);
            var solicitacaoPendente = await _repositorioSolicitacao.ExisteSolicitacaoPendenteAsync(usuarioId, usuarioAlvoId);
            return Resultado<RelacaoUsuarioDTOSaida>.Ok(new RelacaoUsuarioDTOSaida
            {
                Seguindo = seguir,
                SegueVoce = segueVoce,
                SolicitacaoPendente = solicitacaoPendente
            });
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao obter relação entre {UsuarioId} e {AlvoId}", usuarioId, usuarioAlvoId);
            return Resultado<RelacaoUsuarioDTOSaida>.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado<PaginaResultado<UsuarioListaDTOSaida>>> ListarSeguidoresListaAsync(Guid usuarioId, Guid usuarioAutenticadoId, int pagina, int tamanho)
    {
        try
        {
            var pagina_ = await _repositorioUsuario.ObterSeguidoresPaginadoAsync(usuarioId, pagina, tamanho);
            var usuarioAutenticado = await _repositorioUsuario.ObterComSeguindoAsync(usuarioAutenticadoId);
            var seguindoIds = usuarioAutenticado?.Seguindo.Select(u => u.Id).ToHashSet() ?? new HashSet<Guid>();
            var itens = pagina_.Itens.Select(u => new UsuarioListaDTOSaida
            {
                Id = u.Id,
                Nome = u.Nome,
                Seguindo = seguindoIds.Contains(u.Id)
            }).ToList();
            return Resultado<PaginaResultado<UsuarioListaDTOSaida>>.Ok(new PaginaResultado<UsuarioListaDTOSaida>
            {
                Itens = itens,
                Pagina = pagina_.Pagina,
                TamanhoPagina = pagina_.TamanhoPagina,
                Total = pagina_.Total
            });
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao listar seguidores do usuário {UsuarioId}", usuarioId);
            return Resultado<PaginaResultado<UsuarioListaDTOSaida>>.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado<PaginaResultado<UsuarioListaDTOSaida>>> ListarSeguindoListaAsync(Guid usuarioId, Guid usuarioAutenticadoId, int pagina, int tamanho)
    {
        try
        {
            var pagina_ = await _repositorioUsuario.ObterSeguindoPaginadoAsync(usuarioId, pagina, tamanho);
            var usuarioAutenticado = await _repositorioUsuario.ObterComSeguindoAsync(usuarioAutenticadoId);
            var seguindoIds = usuarioAutenticado?.Seguindo.Select(u => u.Id).ToHashSet() ?? new HashSet<Guid>();
            var itens = pagina_.Itens.Select(u => new UsuarioListaDTOSaida
            {
                Id = u.Id,
                Nome = u.Nome,
                Seguindo = seguindoIds.Contains(u.Id)
            }).ToList();
            return Resultado<PaginaResultado<UsuarioListaDTOSaida>>.Ok(new PaginaResultado<UsuarioListaDTOSaida>
            {
                Itens = itens,
                Pagina = pagina_.Pagina,
                TamanhoPagina = pagina_.TamanhoPagina,
                Total = pagina_.Total
            });
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao listar seguindo do usuário {UsuarioId}", usuarioId);
            return Resultado<PaginaResultado<UsuarioListaDTOSaida>>.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<IEnumerable<UsuarioListaDTOSaida>> SugerirUsuariosParaSeguirAsync(Guid usuarioId, int quantidade = 10)
    {
        try
        {
            var usuario = await _repositorioUsuario.ObterComPlantasAsync(usuarioId);
            if (usuario == null)
                return Enumerable.Empty<UsuarioListaDTOSaida>();
            var todos = await _repositorioUsuario.ObterTodosAsync();
            var seguindoIds = usuario.Seguindo.Select(u => u.Id).ToHashSet();
            seguindoIds.Add(usuarioId);
            var sugestoes = todos
                .Where(u => !seguindoIds.Contains(u.Id))
                .OrderByDescending(u => u.Seguidores.Count(s => seguindoIds.Contains(s.Id)))
                .ThenByDescending(u => u.Seguidores.Count)
                .Take(quantidade)
                .Select(u => new UsuarioListaDTOSaida
                {
                    Id = u.Id,
                    Nome = u.Nome,
                    Seguindo = false
                });
            return sugestoes;
        }
        catch
        {
            return Enumerable.Empty<UsuarioListaDTOSaida>();
        }
    }

    public async Task<IEnumerable<UsuarioListaDTOSaida>> BuscarUsuariosPorNomeAsync(string termo)
    {
        var todos = await _repositorioUsuario.ObterTodosAsync();
        return todos.Where(u => u.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase))
            .Select(u => new UsuarioListaDTOSaida { Id = u.Id, Nome = u.Nome, Seguindo = false });
    }

    private static PerfilPublicoDTOSaida MapearPerfilPublico(Usuario u) =>
        new()
        {
            Id = u.Id,
            Nome = u.Nome,
            Biografia = u.Biografia,
            FotoPerfil = u.FotoPerfil,
            PerfilPrivado = u.PerfilPrivado,
            TotalSeguidores = u.Seguidores.Count,
            TotalSeguindo = u.Seguindo.Count,
            TotalPlantas = u.Plantas.Count
        };

    private static PlantaDTOSaida MapearPlanta(Domain.Entities.Planta planta) => new()
    {
        Id = planta.Id,
        NomeCientifico = planta.NomeCientifico,
        NomeComum = planta.NomeComum,
        Familia = planta.Familia,
        Genero = planta.Genero,
        Toxica = planta.Toxica,
        DescricaoToxicidade = planta.DescricaoToxicidade,
        ToxicaAnimais = planta.ToxicaAnimais,
        DescricaoToxicidadeAnimais = planta.DescricaoToxicidadeAnimais,
        ToxicaCriancas = planta.ToxicaCriancas,
        DescricaoToxicidadeCriancas = planta.DescricaoToxicidadeCriancas,
        RequisitosLuz = planta.RequisitosLuz,
        RequisitosAgua = planta.RequisitosAgua,
        RequisitosTemperatura = planta.RequisitosTemperatura,
        Cuidados = planta.Cuidados,
        FotoPlanta = planta.FotoPlanta,
        DataIdentificacao = planta.DataIdentificacao
    };
}
