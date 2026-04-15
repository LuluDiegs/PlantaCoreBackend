using Microsoft.Extensions.Logging;
using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.Comuns.Cache;
using PlantaCoreAPI.Application.Comuns.Eventos;
using PlantaCoreAPI.Application.DTOs.Comentario;
using PlantaCoreAPI.Application.DTOs.Post;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Application.Utils;
using PlantaCoreAPI.Domain.Comuns;
using PlantaCoreAPI.Domain.Entities;
using PlantaCoreAPI.Domain.Interfaces;

namespace PlantaCoreAPI.Application.Services;

public class PostService : IPostService
{
    private readonly IRepositorioPost _repositorioPost;
    private readonly IRepositorioUsuario _repositorioUsuario;
    private readonly IRepositorioPlanta _repositorioPlanta;
    private readonly IRepositorioNotificacao _repositorioNotificacao;
    private readonly IRepositorioComunidade _repositorioComunidade;
    private readonly IEventoDispatcher _eventoDispatcher;
    private readonly ICacheService _cacheService;
    private readonly ILogger<PostService> _logger;
    private readonly IRepositorioPostSave _repositorioPostSave;
    private readonly IRepositorioPostShare _repositorioPostShare;
    private readonly IRepositorioPostView _repositorioPostView;
    private readonly IRepositorioActivityLog _repositorioActivityLog;
    private readonly IRepositorioCurtida _repositorioCurtida;

    public PostService(
        IRepositorioPost repositorioPost,
        IRepositorioUsuario repositorioUsuario,
        IRepositorioPlanta repositorioPlanta,
        IRepositorioNotificacao repositorioNotificacao,
        IRepositorioComunidade repositorioComunidade,
        IEventoDispatcher eventoDispatcher,
        ICacheService cacheService,
        ILogger<PostService> logger,
        IRepositorioPostSave repositorioPostSave,
        IRepositorioPostShare repositorioPostShare,
        IRepositorioPostView repositorioPostView,
        IRepositorioActivityLog repositorioActivityLog,
        IRepositorioCurtida repositorioCurtida)
    {
        _repositorioPost = repositorioPost;
        _repositorioUsuario = repositorioUsuario;
        _repositorioPlanta = repositorioPlanta;
        _repositorioNotificacao = repositorioNotificacao;
        _repositorioComunidade = repositorioComunidade;
        _eventoDispatcher = eventoDispatcher;
        _cacheService = cacheService;
        _logger = logger;
        _repositorioPostSave = repositorioPostSave;
        _repositorioPostShare = repositorioPostShare;
        _repositorioPostView = repositorioPostView;
        _repositorioActivityLog = repositorioActivityLog;
        _repositorioCurtida = repositorioCurtida;
    }

    public async Task<Resultado<PostDTOSaida>> CriarPostAsync(Guid usuarioId, CriarPostDTOEntrada entrada)
    {
        try
        {
            var usuario = await _repositorioUsuario.ObterPorIdAsync(usuarioId);
            if (usuario == null)
                return Resultado<PostDTOSaida>.Erro("Usuário não encontrado");

            Planta? planta = null;
            if (entrada.PlantaId.HasValue)
            {
                planta = await _repositorioPlanta.ObterPorIdAsync(entrada.PlantaId.Value);
                if (planta == null)
                    return Resultado<PostDTOSaida>.Erro("Planta não encontrada");
                if (planta.UsuarioId != usuarioId)
                    return Resultado<PostDTOSaida>.Erro("Você só pode postar sobre suas próprias plantas");
            }

            if (entrada.ComunidadeId.HasValue)
            {
                var ehMembro = await _repositorioComunidade.UsuarioEhMembroAsync(entrada.ComunidadeId.Value, usuarioId);
                if (!ehMembro)
                    return Resultado<PostDTOSaida>.Erro("Você precisa ser membro da comunidade para postar nela");
            }

            var post = Post.Criar(usuarioId, entrada.Conteudo, entrada.PlantaId, entrada.ComunidadeId);
            AplicarMetadadosPost(post, entrada.Conteudo, entrada.Hashtags, entrada.Categorias, entrada.PalavrasChave, planta);

            await _repositorioPost.AdicionarAsync(post);
            await _repositorioPost.SalvarMudancasAsync();

            string? nomeComunidade = null;
            if (entrada.ComunidadeId.HasValue)
            {
                var comunidade = await _repositorioComunidade.ObterPorIdAsync(entrada.ComunidadeId.Value);
                nomeComunidade = comunidade?.Nome;
            }

            return Resultado<PostDTOSaida>.Ok(MapearPostPara(post, usuario, planta, false, nomeComunidade));
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao criar post para usuário {UsuarioId}", usuarioId);
            return Resultado<PostDTOSaida>.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado<PostDTOSaida>> AtualizarPostAsync(Guid usuarioId, Guid postId, AtualizarPostDTOEntrada entrada)
    {
        try
        {
            var post = await _repositorioPost.ObterPorIdAsync(postId);
            if (post == null)
                return Resultado<PostDTOSaida>.Erro("Post não encontrado");
            if (post.UsuarioId != usuarioId)
                return Resultado<PostDTOSaida>.Erro("Sem permissão para atualizar este post");

            Planta? planta = post.PlantaId.HasValue ? await _repositorioPlanta.ObterPorIdAsync(post.PlantaId.Value) : null;

            post.Atualizar(entrada.Conteudo);
            AplicarMetadadosPost(post, entrada.Conteudo, entrada.Hashtags, entrada.Categorias, entrada.PalavrasChave, planta);
            await _repositorioPost.AtualizarAsync(post);
            await _repositorioPost.SalvarMudancasAsync();

            var usuario = await _repositorioUsuario.ObterPorIdAsync(post.UsuarioId);

            string? nomeComunidade = null;
            if (post.ComunidadeId.HasValue)
            {
                var comunidade = await _repositorioComunidade.ObterPorIdAsync(post.ComunidadeId.Value);
                nomeComunidade = comunidade?.Nome;
            }

            return Resultado<PostDTOSaida>.Ok(MapearPostPara(post, usuario!, planta, false, nomeComunidade));
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao atualizar post {PostId} para usuário {UsuarioId}", postId, usuarioId);
            return Resultado<PostDTOSaida>.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado> ExcluirPostAsync(Guid usuarioId, Guid postId)
    {
        try
        {
            var post = await _repositorioPost.ObterPorIdAsync(postId);
            if (post == null)
                return Resultado.Erro("Post não encontrado");
            if (post.UsuarioId != usuarioId)
                return Resultado.Erro("Sem permissão para deletar este post");

            post.Excluir();
            await _repositorioPost.AtualizarAsync(post);
            await _repositorioPost.SalvarMudancasAsync();
            return Resultado.Ok("Post deletado com sucesso");
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao excluir post {PostId} para usuário {UsuarioId}", postId, usuarioId);
            return Resultado.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado<PostDTOSaida>> ObterPostAsync(Guid postId, Guid usuarioId)
    {
        try
        {
            var post = await _repositorioPost.ObterPorIdAsync(postId);
            if (post == null)
                return Resultado<PostDTOSaida>.Erro("Post não encontrado");

            var usuario = await _repositorioUsuario.ObterPorIdAsync(post.UsuarioId);
            Planta? planta = post.PlantaId.HasValue ? await _repositorioPlanta.ObterPorIdAsync(post.PlantaId.Value) : null;
            var curtiu = post.Curtidas.Any(c => c.UsuarioId == usuarioId);

            string? nomeComunidade = null;
            if (post.ComunidadeId.HasValue)
            {
                var comunidade = await _repositorioComunidade.ObterPorIdAsync(post.ComunidadeId.Value);
                nomeComunidade = comunidade?.Nome;
            }

            return Resultado<PostDTOSaida>.Ok(MapearPostPara(post, usuario!, planta, curtiu, nomeComunidade));
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            return Resultado<PostDTOSaida>.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado<IEnumerable<PostDTOSaida>>> ObterFeedAsync(Guid usuarioId, int pagina = 1, int tamanho = 10, string? ordenarPor = null)
    {
        var cacheKey = $"feed:{usuarioId}:{pagina}:{tamanho}:{ordenarPor}";
        var cached = _cacheService.Get<IEnumerable<PostDTOSaida>>(cacheKey);
        if (cached != null)
            return Resultado<IEnumerable<PostDTOSaida>>.Ok(cached);

        try
        {
            var posts = await _repositorioPost.ObterFeedAsync(usuarioId, pagina, tamanho, ordenarPor);
            var postsOrdenados = posts.ToList();

            var dtos = new List<PostDTOSaida>();
            foreach (var post in postsOrdenados.Where(p => p.Usuario != null))
            {
                Planta? planta = post.PlantaId.HasValue ? await _repositorioPlanta.ObterPorIdAsync(post.PlantaId.Value) : null;
                var curtiu = post.Curtidas.Any(c => c.UsuarioId == usuarioId);
                var comentou = post.Comentarios.Any(c => c.UsuarioId == usuarioId);

                string? nomeComunidade = null;
                if (post.ComunidadeId.HasValue)
                {
                    var comunidade = await _repositorioComunidade.ObterPorIdAsync(post.ComunidadeId.Value);
                    nomeComunidade = comunidade?.Nome;
                }

                var dto = MapearPostPara(post, post.Usuario!, planta, curtiu, nomeComunidade);
                dto.ComentadoPorMim = comentou;
                dtos.Add(dto);
            }

            var result = Resultado<IEnumerable<PostDTOSaida>>.Ok(dtos);
            _cacheService.Set(cacheKey, dtos, TimeSpan.FromMinutes(2));
            return result;
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao obter feed do usuário {UsuarioId}", usuarioId);
            return Resultado<IEnumerable<PostDTOSaida>>.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado> CurtirPostAsync(Guid usuarioId, Guid postId)
    {
        try
        {
            _logger.LogInformation("Usuário {UsuarioId} tentando curtir post {PostId}", usuarioId, postId);

            var post = await _repositorioPost.ObterPorIdAsync(postId);
            if (post == null)
                return Resultado.Erro("Post não encontrado");
            if (post.UsuarioId == usuarioId)
                return Resultado.Erro("Você não pode curtir seu próprio post");
            if (await _repositorioCurtida.ExisteAsync(usuarioId, postId))
                return Resultado.Erro("Você já curtiu este post");
            var usuario = await _repositorioUsuario.ObterPorIdAsync(usuarioId);
            if (usuario == null)
                return Resultado.Erro("Usuário não encontrado");

            var curtida = Curtida.CriarParaPost(postId, usuarioId);
            await _repositorioCurtida.AdicionarAsync(curtida);
            await _repositorioCurtida.SalvarMudancasAsync();
            await _eventoDispatcher.PublicarAsync(new PostCurtidoEvento { UsuarioId = usuarioId, PostId = postId });

            var notificacao = Notificacao.Criar(
                post.UsuarioId,
                Domain.Enums.TipoNotificacao.Curtida,
                $"{usuario.Nome} curtiu seu post",
                usuarioId,
                null,
                postId);
            await _repositorioNotificacao.AdicionarAsync(notificacao);
            await _repositorioNotificacao.SalvarMudancasAsync();

            _logger.LogInformation("Usuário {UsuarioId} curtiu post {PostId}", usuarioId, postId);
            return Resultado.Ok("Post curtido com sucesso");
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao curtir post {UsuarioId} -> {PostId}", usuarioId, postId);
            return Resultado.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado> RemoverCurtidaAsync(Guid usuarioId, Guid postId)
    {
        try
        {
            var curtida = await _repositorioCurtida.ObterPorUsuarioEPostAsync(usuarioId, postId);
            if (curtida == null)
                return Resultado.Erro("Curtida não encontrada");

            await _repositorioCurtida.RemoverAsync(curtida);
            await _repositorioCurtida.SalvarMudancasAsync();
            return Resultado.Ok("Curtida removida com sucesso");
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao remover curtida {UsuarioId} -> {PostId}", usuarioId, postId);
            return Resultado.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado<ComentarioDTOSaida>> CriarComentarioAsync(Guid usuarioId, CriarComentarioDTOEntrada entrada)
    {
        try
        {
            var post = await _repositorioPost.ObterPorIdAsync(entrada.PostId);
            var usuario = await _repositorioUsuario.ObterPorIdAsync(usuarioId);
            if (post == null || usuario == null)
                return Resultado<ComentarioDTOSaida>.Erro("Post ou usuário não encontrado");

            var comentario = Comentario.Criar(entrada.PostId, usuarioId, entrada.Conteudo);
            post.AdicionarComentario(comentario);
            await _repositorioPost.AtualizarAsync(post);
            await _repositorioPost.SalvarMudancasAsync();
            await _eventoDispatcher.PublicarAsync(new ComentarioCriadoEvento { UsuarioId = usuarioId, PostId = entrada.PostId, ComentarioId = comentario.Id });

            if (post.UsuarioId != usuarioId)
            {
                var notificacao = Notificacao.Criar(
                    post.UsuarioId,
                    Domain.Enums.TipoNotificacao.Comentario,
                    $"{usuario.Nome} comentou seu post",
                    usuarioId,
                    null,
                    entrada.PostId);
                await _repositorioNotificacao.AdicionarAsync(notificacao);
                await _repositorioNotificacao.SalvarMudancasAsync();
            }

            return Resultado<ComentarioDTOSaida>.Ok(MapearComentarioPara(comentario, usuario));
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            return Resultado<ComentarioDTOSaida>.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado<ComentarioDTOSaida>> AtualizarComentarioAsync(Guid usuarioId, Guid comentarioId, AtualizarComentarioDTOEntrada entrada)
    {
        try
        {
            var post = await _repositorioPost.ObterPorComentarioIdAsync(comentarioId);
            if (post == null)
                return Resultado<ComentarioDTOSaida>.Erro("Comentário não encontrado");

            var comentario = post.Comentarios.FirstOrDefault(c => c.Id == comentarioId);
            if (comentario == null || comentario.UsuarioId != usuarioId)
                return Resultado<ComentarioDTOSaida>.Erro("Sem permissão para atualizar este comentário");
            comentario.Atualizar(entrada.Conteudo);
            await _repositorioPost.AtualizarAsync(post);
            await _repositorioPost.SalvarMudancasAsync();

            var usuario = await _repositorioUsuario.ObterPorIdAsync(usuarioId);
            return Resultado<ComentarioDTOSaida>.Ok(MapearComentarioPara(comentario, usuario!));
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            return Resultado<ComentarioDTOSaida>.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado> ExcluirComentarioAsync(Guid usuarioId, Guid comentarioId)
    {
        try
        {
            var post = await _repositorioPost.ObterPorComentarioIdAsync(comentarioId);
            if (post == null)
                return Resultado.Erro("Comentário não encontrado");

            var comentario = post.Comentarios.FirstOrDefault(c => c.Id == comentarioId);
            if (comentario == null || comentario.UsuarioId != usuarioId)
                return Resultado.Erro("Sem permissão para deletar este comentário");
            comentario.Excluir();
            await _repositorioPost.AtualizarAsync(post);
            await _repositorioPost.SalvarMudancasAsync();
            return Resultado.Ok("Comentário deletado com sucesso");
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            return Resultado.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado<IEnumerable<ComentarioDTOSaida>>> ListarComentariosPostAsync(Guid postId, Guid usuarioAutenticadoId, int pagina = 1, int tamanho = 20, string? ordenar = null)
    {
        try
        {
            var post = await _repositorioPost.ObterPorIdAsync(postId);
            if (post == null)
                return Resultado<IEnumerable<ComentarioDTOSaida>>.Erro("Post não encontrado");

            var comentarios = post.Comentarios
                .Where(c => c.Ativo && c.ComentarioPaiId == null)
                .ToList();

            comentarios = ordenar == "relevantes"
                ? comentarios.OrderByDescending(c => c.Curtidas.Count).ThenByDescending(c => c.DataCriacao).ToList()
                : comentarios.OrderByDescending(c => c.DataCriacao).ToList();

            comentarios = comentarios.Skip((pagina - 1) * tamanho).Take(tamanho).ToList();

            var idsUsuarios = comentarios.Select(c => c.UsuarioId).Distinct().ToList();
            var usuariosList = await _repositorioUsuario.ObterPorIdsAsync(idsUsuarios);
            var usuarios = usuariosList.ToDictionary(u => u.Id);

            var dtos = comentarios
                .Where(c => usuarios.ContainsKey(c.UsuarioId))
                .Select(c =>
                {
                    var totalRespostas = post.Comentarios.Count(r => r.ComentarioPaiId == c.Id && r.Ativo);
                    var dto = MapearComentarioPara(c, usuarios[c.UsuarioId], usuarioAutenticadoId);
                    dto.TotalRespostas = totalRespostas;
                    return dto;
                })
                .ToList();

            return Resultado<IEnumerable<ComentarioDTOSaida>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            return Resultado<IEnumerable<ComentarioDTOSaida>>.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado<PaginaResultado<PostDTOSaida>>> ListarPostsUsuarioAsync(Guid usuarioId, Guid usuarioAutenticadoId, int pagina, int tamanho, string? ordenarPor)
    {
        try
        {
            var paginaPosts = await _repositorioPost.ObterPorUsuarioPaginadoAsync(usuarioId, pagina, tamanho, ordenarPor);
            var itens = new List<PostDTOSaida>();
            foreach (var post in paginaPosts.Itens.Where(p => p.Usuario != null))
            {
                Planta? planta = post.PlantaId.HasValue ? await _repositorioPlanta.ObterPorIdAsync(post.PlantaId.Value) : null;
                itens.Add(MapearPostPara(post, post.Usuario!, planta, post.Curtidas.Any(c => c.UsuarioId == usuarioAutenticadoId)));
            }

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
            _logger.LogError(ex, "Erro ao listar posts do usuário {UsuarioId}", usuarioId);
            return Resultado<PaginaResultado<PostDTOSaida>>.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado<PaginaResultado<PostDTOSaida>>> ObterExploradorAsync(Guid usuarioAutenticadoId, int pagina, int tamanho, string? ordenarPor)
    {
        try
        {
            var paginaPosts = await _repositorioPost.ObterExploradorAsync(pagina, tamanho, ordenarPor);
            var itens = new List<PostDTOSaida>();
            foreach (var post in paginaPosts.Itens.Where(p => p.Usuario != null))
            {
                Planta? planta = post.PlantaId.HasValue ? await _repositorioPlanta.ObterPorIdAsync(post.PlantaId.Value) : null;
                itens.Add(MapearPostPara(post, post.Usuario!, planta, post.Curtidas.Any(c => c.UsuarioId == usuarioAutenticadoId)));
            }

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
            _logger.LogError(ex, "Erro ao obter explorador");
            return Resultado<PaginaResultado<PostDTOSaida>>.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado> CurtirComentarioAsync(Guid usuarioId, Guid comentarioId)
    {
        try
        {
            var comentario = await _repositorioPost.ObterComentarioPorIdAsync(comentarioId);
            if (comentario == null)
                return Resultado.Erro("Comentário não encontrado");
            if (comentario.Curtidas.Any(c => c.UsuarioId == usuarioId))
                return Resultado.Erro("Você já curtiu este comentário");

            comentario.AdicionarCurtida(usuarioId);
            await _repositorioPost.AtualizarComentarioAsync(comentario);
            await _repositorioPost.SalvarMudancasAsync();
            return Resultado.Ok("Comentário curtido com sucesso");
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao curtir comentário {ComentarioId}", comentarioId);
            return Resultado.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado> RemoverCurtidaComentarioAsync(Guid usuarioId, Guid comentarioId)
    {
        try
        {
            var comentario = await _repositorioPost.ObterComentarioPorIdAsync(comentarioId);
            if (comentario == null)
                return Resultado.Erro("Comentário não encontrado");
            if (!comentario.Curtidas.Any(c => c.UsuarioId == usuarioId))
                return Resultado.Erro("Você não curtiu este comentário");

            comentario.RemoverCurtida(usuarioId);
            await _repositorioPost.AtualizarComentarioAsync(comentario);
            await _repositorioPost.SalvarMudancasAsync();
            return Resultado.Ok("Curtida removida com sucesso");
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao remover curtida do comentário {ComentarioId}", comentarioId);
            return Resultado.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado> ExcluirComentarioComoDonoPostAsync(Guid donoPostId, Guid comentarioId)
    {
        try
        {
            var post = await _repositorioPost.ObterPorComentarioIdAsync(comentarioId);
            if (post == null)
                return Resultado.Erro("Comentário não encontrado");
            if (post.UsuarioId != donoPostId)
                return Resultado.Erro("Sem permissão para excluir este comentário");

            var comentario = post.Comentarios.FirstOrDefault(c => c.Id == comentarioId);
            if (comentario == null)
                return Resultado.Erro("Comentário não encontrado");
            comentario.Excluir();
            await _repositorioPost.AtualizarAsync(post);
            await _repositorioPost.SalvarMudancasAsync();
            return Resultado.Ok("Comentário excluído com sucesso");
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            return Resultado.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado<IEnumerable<PostDTOSaida>>> ListarPostsCurtidosAsync(Guid usuarioId, Guid usuarioAutenticadoId)
    {
        try
        {
            var posts = await _repositorioPost.ObterPostsCurtidosPorUsuarioAsync(usuarioId);
            var dtos = new List<PostDTOSaida>();
            foreach (var post in posts.Where(p => p.Usuario != null))
            {
                Planta? planta = post.PlantaId.HasValue ? await _repositorioPlanta.ObterPorIdAsync(post.PlantaId.Value) : null;
                dtos.Add(MapearPostPara(post, post.Usuario!, planta, post.Curtidas.Any(c => c.UsuarioId == usuarioAutenticadoId)));
            }

            return Resultado<IEnumerable<PostDTOSaida>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao listar posts curtidos do usuário {UsuarioId}", usuarioId);
            return Resultado<IEnumerable<PostDTOSaida>>.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado> SalvarPostAsync(Guid usuarioId, Guid postId)
    {
        try
        {
            if (await _repositorioPostSave.ExisteAsync(usuarioId, postId))
                return Resultado.Ok("Post já salvo");

            await _repositorioPostSave.AdicionarAsync(PostSave.Criar(usuarioId, postId));
            await _repositorioPostSave.SalvarMudancasAsync();
            await _repositorioActivityLog.AdicionarAsync(ActivityLog.Criar(usuarioId, "POST_SALVO", postId, "Post"));
            await _repositorioActivityLog.SalvarMudancasAsync();
            return Resultado.Ok("Post salvo com sucesso");
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao salvar post {PostId} para usuário {UsuarioId}", postId, usuarioId);
            return Resultado.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado> RemoverPostSalvoAsync(Guid usuarioId, Guid postId)
    {
        try
        {
            await _repositorioPostSave.RemoverAsync(usuarioId, postId);
            await _repositorioPostSave.SalvarMudancasAsync();
            return Resultado.Ok("Post removido dos salvos");
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao remover post salvo {PostId} para usuário {UsuarioId}", postId, usuarioId);
            return Resultado.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado<IEnumerable<PostDTOSaida>>> ListarPostsSalvosAsync(Guid usuarioId)
    {
        try
        {
            var salvos = await _repositorioPostSave.ListarPorUsuarioAsync(usuarioId);
            if (salvos.Count == 0)
                return Resultado<IEnumerable<PostDTOSaida>>.Ok(Array.Empty<PostDTOSaida>());
            var posts = await _repositorioPost.ObterPorIdsAsync(salvos.Select(s => s.PostId));
            var dtos = posts.Select(p => MapearPostPara(p, p.Usuario!, p.Planta, false)).ToList();
            return Resultado<IEnumerable<PostDTOSaida>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao listar posts salvos do usuário {UsuarioId}", usuarioId);
            return Resultado<IEnumerable<PostDTOSaida>>.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado> CompartilharPostAsync(Guid usuarioId, Guid postId)
    {
        try
        {
            if (await _repositorioPostShare.ExisteAsync(usuarioId, postId))
                return Resultado.Erro("Você já compartilhou este post");

            await _repositorioPostShare.AdicionarAsync(PostShare.Criar(usuarioId, postId));
            await _repositorioPostShare.SalvarMudancasAsync();
            await _repositorioActivityLog.AdicionarAsync(ActivityLog.Criar(usuarioId, "POST_COMPARTILHADO", postId, "Post"));
            await _repositorioActivityLog.SalvarMudancasAsync();
            return Resultado.Ok("Post compartilhado com sucesso");
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao compartilhar post {PostId} para usuário {UsuarioId}", postId, usuarioId);
            return Resultado.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado> VisualizarPostAsync(Guid usuarioId, Guid postId)
    {
        try
        {
            if (await _repositorioPostView.ExisteAsync(usuarioId, postId))
                return Resultado.Ok("Visualização já registrada");

            await _repositorioPostView.AdicionarAsync(PostView.Criar(usuarioId, postId));
            await _repositorioPostView.SalvarMudancasAsync();
            await _repositorioActivityLog.AdicionarAsync(ActivityLog.Criar(usuarioId, "POST_VISUALIZADO", postId, "Post"));
            await _repositorioActivityLog.SalvarMudancasAsync();
            return Resultado.Ok("Visualização registrada");
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            return Resultado.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado<ComentarioDTOSaida>> ResponderComentarioAsync(Guid usuarioId, Guid comentarioId, string conteudo)
    {
        try
        {
            var comentarioPai = await _repositorioPost.ObterComentarioPorIdAsync(comentarioId);
            if (comentarioPai == null)
                return Resultado<ComentarioDTOSaida>.Erro("Comentário não encontrado");

            var post = await _repositorioPost.ObterPorIdAsync(comentarioPai.PostId);
            if (post == null)
                return Resultado<ComentarioDTOSaida>.Erro("Post não encontrado");
            var resposta = Comentario.Criar(comentarioPai.PostId, usuarioId, conteudo, comentarioId);
            post.AdicionarComentario(resposta);
            await _repositorioPost.AtualizarAsync(post);
            await _repositorioPost.SalvarMudancasAsync();

            var usuario = await _repositorioUsuario.ObterPorIdAsync(usuarioId);
            return Resultado<ComentarioDTOSaida>.Ok(MapearComentarioPara(resposta, usuario!));
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao responder comentário {ComentarioId} por usuário {UsuarioId}", comentarioId, usuarioId);
            return Resultado<ComentarioDTOSaida>.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<Resultado<PaginaResultado<PostDTOSaida>>> ListarPostsComunidadeAsync(Guid comunidadeId, Guid usuarioId, int pagina, int tamanho, string? ordenarPor)
    {
        var paginaPosts = await _repositorioPost.ObterPorComunidadeAsync(comunidadeId, pagina, tamanho, ordenarPor);
        var comunidade = await _repositorioComunidade.ObterPorIdAsync(comunidadeId);
        var itens = new List<PostDTOSaida>();
        foreach (var post in paginaPosts.Itens)
        {
            Planta? planta = post.PlantaId.HasValue ? await _repositorioPlanta.ObterPorIdAsync(post.PlantaId.Value) : null;
            var curtiu = post.Curtidas.Any(c => c.UsuarioId == usuarioId);
            itens.Add(MapearPostPara(post, post.Usuario!, planta, curtiu, comunidade?.Nome));
        }
        return Resultado<PaginaResultado<PostDTOSaida>>.Ok(new PaginaResultado<PostDTOSaida>
        {
            Itens = itens,
            Pagina = paginaPosts.Pagina,
            TamanhoPagina = paginaPosts.TamanhoPagina,
            Total = paginaPosts.Total
        });
    }

    public async Task<Resultado<PaginaResultado<PostDTOSaida>>> BuscarPostsAsync(string? q, int pagina, int tamanho)
    {
        var paginaResult = await _repositorioPost.BuscarPostsAsync(q, pagina, tamanho);
        var itens = new List<PostDTOSaida>();
        foreach (var post in paginaResult.Itens)
        {
            Planta? planta = post.PlantaId.HasValue ? await _repositorioPlanta.ObterPorIdAsync(post.PlantaId.Value) : null;
            itens.Add(MapearPostPara(post, post.Usuario!, planta, false, post.Comunidade?.Nome));
        }
        return Resultado<PaginaResultado<PostDTOSaida>>.Ok(new PaginaResultado<PostDTOSaida>
        {
            Itens = itens,
            Pagina = paginaResult.Pagina,
            TamanhoPagina = paginaResult.TamanhoPagina,
            Total = paginaResult.Total
        });
    }

    public async Task<Resultado<PaginaResultado<PostDTOSaida>>> BuscarPostsPorPlantaAsync(string nomePlanta, int pagina = 1, int tamanho = 10)
    {
        try
        {
            var paginaResult = await _repositorioPost.BuscarPostsPorPlantaAsync(nomePlanta, pagina, tamanho);
            var dtos = paginaResult.Itens.Select(p => MapearPostPara(p, p.Usuario!, p.Planta, false)).ToList();
            return Resultado<PaginaResultado<PostDTOSaida>>.Ok(new PaginaResultado<PostDTOSaida>
            {
                Itens = dtos,
                Pagina = paginaResult.Pagina,
                TamanhoPagina = paginaResult.TamanhoPagina,
                Total = paginaResult.Total
            });
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao buscar posts por planta {NomePlanta}", nomePlanta);
            return Resultado<PaginaResultado<PostDTOSaida>>.Erro("Ocorreu um erro interno. Tente novamente.");
        }
    }

    public async Task<IEnumerable<PostDTOSaida>> ObterTrendingPostsAsync(int quantidade = 10)
    {
        try
        {
            var posts = await _repositorioPost.ObterTrendingPostsAsync(quantidade);
            return posts.Select(p => MapearPostPara(p, p.Usuario!, p.Planta, false)).ToList();
        }
        catch (Exception ex)
        {
            ExcecaoTransienteHelper.RelancaSeFoiTransiente(ex);
            _logger.LogError(ex, "Erro ao obter trending posts");
            return Enumerable.Empty<PostDTOSaida>();
        }
    }

    private static PostDTOSaida MapearPostPara(Post post, Usuario usuario, Planta? planta, bool curtiu, string? nomeComunidade = null)
    {
        return new PostDTOSaida
        {
            Id = post.Id,
            PlantaId = post.PlantaId,
            ComunidadeId = post.ComunidadeId,
            NomeComunidade = nomeComunidade,
            UsuarioId = post.UsuarioId,
            NomeUsuario = usuario.Nome,
            FotoUsuario = usuario.FotoPerfil,
            NomePlanta = planta != null ? (planta.NomeComum ?? planta.NomeCientifico) : null,
            FotoPlanta = planta?.FotoPlanta,
            Conteudo = post.Conteudo,
            Hashtags = post.Hashtags.Select(h => h.Nome).ToList(),
            Categorias = post.Categorias.Select(c => c.Nome).ToList(),
            PalavrasChave = post.PalavrasChave.Select(pc => pc.Palavra).ToList(),
            TotalCurtidas = post.Curtidas.Count,
            TotalComentarios = post.Comentarios.Count,
            CurtiuUsuario = curtiu,
            DataCriacao = post.DataCriacao,
            DataAtualizacao = post.DataAtualizacao
        };
    }

    private static ComentarioDTOSaida MapearComentarioPara(Comentario comentario, Usuario usuario, Guid usuarioAutenticadoId = default)
    {
        return new ComentarioDTOSaida
        {
            Id = comentario.Id,
            PostId = comentario.PostId,
            UsuarioId = comentario.UsuarioId,
            NomeUsuario = usuario.Nome,
            FotoUsuario = usuario.FotoPerfil,
            Conteudo = comentario.Conteudo,
            TotalCurtidas = comentario.Curtidas.Count,
            CurtiuUsuario = usuarioAutenticadoId != default && comentario.Curtidas.Any(c => c.UsuarioId == usuarioAutenticadoId),
            DataCriacao = comentario.DataCriacao,
            DataAtualizacao = comentario.DataAtualizacao
        };
    }

    private static readonly System.Text.RegularExpressions.Regex RegexHashtag =
        new(@"#(\w+)", System.Text.RegularExpressions.RegexOptions.Compiled);

    private static void AplicarMetadadosPost(
        Post post,
        string conteudo,
        IEnumerable<string>? hashtagsEntrada,
        IEnumerable<string>? categoriasEntrada,
        IEnumerable<string>? palavrasChaveEntrada,
        Planta? planta)
    {
        var hashtags = ExtractHashtags(conteudo)
            .Concat(NormalizarHashtags(hashtagsEntrada))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var categorias = planta == null
            ? new List<string>()
            : NormalizarLista(new[] { planta.NomeCientifico, planta.NomeComum }.Where(s => s != null).Select(s => s!));

        var palavrasChave = ExtractPalavrasChave(conteudo)
            .Concat(NormalizarLista(palavrasChaveEntrada))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        post.Hashtags.Clear();
        post.Hashtags.AddRange(hashtags.Select(h => new Hashtag
        {
            Id = Guid.NewGuid(),
            Nome = h,
            PostId = post.Id
        }));

        post.Categorias.Clear();
        post.Categorias.AddRange(categorias.Select(c => new Categoria
        {
            Id = Guid.NewGuid(),
            Nome = c,
            PostId = post.Id
        }));

        post.PalavrasChave.Clear();
        post.PalavrasChave.AddRange(palavrasChave.Select(pc => new PalavraChave
        {
            Id = Guid.NewGuid(),
            Palavra = pc,
            PostId = post.Id
        }));
    }

    private static List<string> ExtractHashtags(string conteudo)
    {
        return RegexHashtag.Matches(conteudo)
            .Cast<System.Text.RegularExpressions.Match>()
            .Select(m => m.Value.Trim())
            .Where(h => !string.IsNullOrWhiteSpace(h))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<string> ExtractPalavrasChave(string conteudo)
    {
        return System.Text.RegularExpressions.Regex.Split(conteudo ?? string.Empty, @"\s+")
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<string> NormalizarHashtags(IEnumerable<string>? valores)
    {
        return (valores ?? Enumerable.Empty<string>())
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v.Trim())
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v.StartsWith('#') ? v : $"#{v}")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<string> NormalizarLista(IEnumerable<string>? valores, bool removerPrefixoHashtag = false)
    {
        return (valores ?? Enumerable.Empty<string>())
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => removerPrefixoHashtag ? v.Trim().TrimStart('#') : v.Trim())
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
