using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.DTOs.Post;
using PlantaCoreAPI.Application.DTOs.Comentario;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Application.Comuns.Eventos;
using PlantaCoreAPI.Application.Comuns.Cache;
using PlantaCoreAPI.Domain.Comuns;
using PlantaCoreAPI.Domain.Entities;
using PlantaCoreAPI.Domain.Interfaces;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace PlantaCoreAPI.Infrastructure.Services;

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

    // Simulação: posts salvos em memória (ideal: persistir em banco)
    private static readonly Dictionary<Guid, HashSet<Guid>> _postsSalvos = new();
    // Simulação: compartilhamentos e visualizações em memória
    private static readonly Dictionary<Guid, HashSet<Guid>> _postsCompartilhados = new();
    private static readonly Dictionary<Guid, HashSet<Guid>> _postsVisualizados = new();

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

            // Extrair hashtags do conteúdo
            var hashtags = ExtractHashtags(entrada.Conteudo)
                .Select(h => new Hashtag { Nome = h })
                .ToList();

            // Gerar palavras-chave com base no conteúdo e na planta
            var palavrasChave = ExtractKeywords(entrada.Conteudo, planta)
                .Select(pc => new PalavraChave { Palavra = pc })
                .ToList();

            // Definir categorias com base na planta
            var categorias = new List<Categoria>();
            if (planta != null)
            {
                categorias.Add(new Categoria { Nome = planta.NomeCientifico });
                if (!string.IsNullOrWhiteSpace(planta.NomeComum))
                {
                    categorias.Add(new Categoria { Nome = planta.NomeComum });
                }
            }

            // Criar o post
            var post = new Post
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuarioId,
                Conteudo = entrada.Conteudo,
                DataCriacao = DateTime.UtcNow,
                Hashtags = hashtags,
                Categorias = categorias,
                PalavrasChave = palavrasChave,
                PlantaId = entrada.PlantaId,
                ComunidadeId = entrada.ComunidadeId // Usar somente se fornecido
            };

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
            return Resultado<PostDTOSaida>.Erro($"Erro ao criar post: {ex.Message}");
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

            post.Atualizar(entrada.Conteudo);
            await _repositorioPost.AtualizarAsync(post);
            await _repositorioPost.SalvarMudancasAsync();

            var usuario = await _repositorioUsuario.ObterPorIdAsync(post.UsuarioId);
            Planta? planta = post.PlantaId.HasValue ? await _repositorioPlanta.ObterPorIdAsync(post.PlantaId.Value) : null;

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
            return Resultado<PostDTOSaida>.Erro($"Erro ao atualizar post: {ex.Message}");
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
            return Resultado.Erro($"Erro ao deletar post: {ex.Message}");
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
            return Resultado<PostDTOSaida>.Erro($"Erro ao obter post: {ex.Message}");
        }
    }

    public async Task<Resultado<IEnumerable<PostDTOSaida>>> ObterFeedAsync(Guid usuarioId, int pagina = 1, int tamanho = 10, string? cursor = null)
    {
        var cacheKey = $"feed:{usuarioId}:{pagina}:{tamanho}:{cursor}";
        var cached = _cacheService.Get<IEnumerable<PostDTOSaida>>(cacheKey);
        if (cached != null)
            return Resultado<IEnumerable<PostDTOSaida>>.Ok(cached);

        try
        {
            var posts = await _repositorioPost.ObterFeedAsync(usuarioId, pagina, tamanho);

            // Cursor: se fornecido, filtrar posts a partir do cursor (id ou data)
            if (!string.IsNullOrEmpty(cursor) && Guid.TryParse(cursor, out var cursorId))
            {
                var cursorPost = posts.FirstOrDefault(p => p.Id == cursorId);
                if (cursorPost != null)
                {
                    posts = posts.Where(p => p.DataCriacao < cursorPost.DataCriacao).ToList();
                }
            }

            // Ranking: relevância (curtidas + comentários), engajamento, recência
            var postsOrdenados = posts
                .OrderByDescending(p => p.Curtidas.Count + p.Comentarios.Count)
                .ThenByDescending(p => p.DataCriacao)
                .ToList();

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
            return Resultado<IEnumerable<PostDTOSaida>>.Erro($"Erro ao obter feed: {ex.Message}");
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

            var usuario = await _repositorioUsuario.ObterPorIdAsync(usuarioId);
            if (usuario == null)
                return Resultado.Erro("Usuário não encontrado");

            // Remove qualquer curtida duplicada na lista em memória
            post.Curtidas.RemoveAll(c => c.UsuarioId == usuarioId && c.PostId == postId);

            // Verifica se já existe curtida no banco
            if (await _repositorioCurtida.ExisteAsync(usuarioId, postId))
                return Resultado.Erro("Você já curtiu este post");

            post.AdicionarCurtida(new Curtida
            {
                UsuarioId = usuario.Id,
                PostId = post.Id,
                DataCriacao = DateTime.UtcNow
            });
            await _repositorioPost.AtualizarAsync(post);
            await _repositorioPost.SalvarMudancasAsync();

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
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
        {
            _logger.LogWarning("Concorrência detectada ao curtir post: {UsuarioId} -> {PostId}", usuarioId, postId);
            return Resultado.Erro("Concorrência detectada. Tente novamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao curtir post {UsuarioId} -> {PostId}", usuarioId, postId);
            return Resultado.Erro($"Erro ao curtir post: {ex.Message}");
        }
    }

    public async Task<Resultado> RemoverCurtidaAsync(Guid usuarioId, Guid postId)
    {
        try
        {
            var post = await _repositorioPost.ObterPorIdAsync(postId);
            if (post == null)
                return Resultado.Erro("Post não encontrado");

            post.RemoverCurtida(usuarioId);
            await _repositorioPost.AtualizarAsync(post);
            await _repositorioPost.SalvarMudancasAsync();

            return Resultado.Ok("Curtida removida com sucesso");
        }
        catch (Exception ex)
        {
            return Resultado.Erro($"Erro ao remover curtida: {ex.Message}");
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

            // Controle otimista: recarregar post antes de comentar
            post = await _repositorioPost.ObterPorIdAsync(entrada.PostId);

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
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
        {
            return Resultado<ComentarioDTOSaida>.Erro("Concorrência detectada. Tente novamente.");
        }
        catch (Exception ex)
        {
            return Resultado<ComentarioDTOSaida>.Erro($"Erro ao criar comentário: {ex.Message}");
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
            return Resultado<ComentarioDTOSaida>.Erro($"Erro ao atualizar comentário: {ex.Message}");
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
            return Resultado.Erro($"Erro ao deletar comentário: {ex.Message}");
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

            if (ordenar == "relevantes")
                comentarios = comentarios.OrderByDescending(c => c.Curtidas.Count).ThenByDescending(c => c.DataCriacao).ToList();
            else
                comentarios = comentarios.OrderByDescending(c => c.DataCriacao).ToList();

            comentarios = comentarios.Skip((pagina - 1) * tamanho).Take(tamanho).ToList();

            var idsUsuarios = comentarios.Select(c => c.UsuarioId).Distinct().ToList();
            var usuarios = new Dictionary<Guid, Usuario>();
            foreach (var id in idsUsuarios)
            {
                var usuario = await _repositorioUsuario.ObterPorIdAsync(id);
                if (usuario != null)
                    usuarios[id] = usuario;
            }

            var dtos = comentarios
                .Where(c => usuarios.ContainsKey(c.UsuarioId))
                .Select(c => {
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
            return Resultado<IEnumerable<ComentarioDTOSaida>>.Erro($"Erro ao listar comentários: {ex.Message}");
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
            return Resultado<PaginaResultado<PostDTOSaida>>.Erro($"Erro ao listar posts: {ex.Message}");
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
            return Resultado<PaginaResultado<PostDTOSaida>>.Erro($"Erro ao obter explorador: {ex.Message}");
        }
    }

    public async Task<Resultado> CurtirComentarioAsync(Guid usuarioId, Guid comentarioId)
    {
        try
        {
            var comentario = await _repositorioPost.ObterComentarioPorIdAsync(comentarioId);
            if (comentario == null)
                return Resultado.Erro("Comentário não encontrado");

            comentario.AdicionarCurtida(usuarioId);
            await _repositorioPost.AtualizarComentarioAsync(comentario);
            await _repositorioPost.SalvarMudancasAsync();
            return Resultado.Ok("Comentário curtido com sucesso");
        }
        catch (Exception ex)
        {
            return Resultado.Erro($"Erro ao curtir comentário: {ex.Message}");
        }
    }

    public async Task<Resultado> RemoverCurtidaComentarioAsync(Guid usuarioId, Guid comentarioId)
    {
        try
        {
            var comentario = await _repositorioPost.ObterComentarioPorIdAsync(comentarioId);
            if (comentario == null)
                return Resultado.Erro("Comentário não encontrado");

            comentario.RemoverCurtida(usuarioId);
            await _repositorioPost.AtualizarComentarioAsync(comentario);
            await _repositorioPost.SalvarMudancasAsync();
            return Resultado.Ok("Curtida removida com sucesso");
        }
        catch (Exception ex)
        {
            return Resultado.Erro($"Erro ao remover curtida do comentário: {ex.Message}");
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
            return Resultado.Erro($"Erro ao excluir comentário: {ex.Message}");
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
            return Resultado<IEnumerable<PostDTOSaida>>.Erro($"Erro ao listar posts curtidos: {ex.Message}");
        }
    }

    public async Task<Resultado<PaginaResultado<PostDTOSaida>>> ListarPostsPorIdsAsync(List<Guid> postIds, Guid usuarioId)
    {
        try
        {
            var posts = await _repositorioPost.ObterPorIdsAsync(postIds);

            var itens = posts.Select(post =>
            {
                Planta? planta = post.PlantaId.HasValue ? _repositorioPlanta.ObterPorIdAsync(post.PlantaId.Value).Result : null;
                return MapearPostPara(post, post.Usuario!, planta, post.Curtidas.Any(c => c.UsuarioId == usuarioId));
            }).ToList();

            return Resultado<PaginaResultado<PostDTOSaida>>.Ok(new PaginaResultado<PostDTOSaida>
            {
                Itens = itens,
                Pagina = 1,
                TamanhoPagina = itens.Count,
                Total = itens.Count
            });
        }
        catch (Exception ex)
        {
            return Resultado<PaginaResultado<PostDTOSaida>>.Erro($"Erro ao listar posts por IDs: {ex.Message}");
        }
    }

    public async Task<Resultado<PaginaResultado<PostDTOSaida>>> ObterFeedFiltradoAsync(Guid usuarioId, string ordenacao, int pagina = 1, int tamanho = 10)
    {
        try
        {
            var posts = await _repositorioPost.ObterFeedAsync(usuarioId, pagina, tamanho);

            IEnumerable<Post> postsOrdenados = ordenacao.ToLower() switch
            {
                "recentes" => posts.OrderByDescending(p => p.DataCriacao),
                "recomendadas" => posts.OrderByDescending(p => p.Curtidas.Count),
                _ => posts
            };

            var dtos = new List<PostDTOSaida>();
            foreach (var post in postsOrdenados.Where(p => p.Usuario != null))
            {
                Planta? planta = post.PlantaId.HasValue ? await _repositorioPlanta.ObterPorIdAsync(post.PlantaId.Value) : null;
                var curtiu = post.Curtidas.Any(c => c.UsuarioId == usuarioId);

                string? nomeComunidade = null;
                if (post.ComunidadeId.HasValue)
                {
                    var comunidade = await _repositorioComunidade.ObterPorIdAsync(post.ComunidadeId.Value);
                    nomeComunidade = comunidade?.Nome;
                }

                dtos.Add(MapearPostPara(post, post.Usuario!, planta, curtiu, nomeComunidade));
            }

            return Resultado<PaginaResultado<PostDTOSaida>>.Ok(new PaginaResultado<PostDTOSaida>
            {
                Itens = dtos,
                Pagina = pagina,
                TamanhoPagina = tamanho,
                Total = dtos.Count
            });
        }
        catch (Exception ex)
        {
            return Resultado<PaginaResultado<PostDTOSaida>>.Erro($"Erro ao obter feed filtrado: {ex.Message}");
        }
    }

    public async Task<Resultado<PaginaResultado<PostDTOSaida>>> ObterFeedFiltradoPorDataAsync(Guid usuarioId, int pagina, int tamanho, DateTime? dataInicio, DateTime? dataFim)
    {
        var posts = await _repositorioPost.ObterFeedAsync(usuarioId, 1, int.MaxValue);
        if (dataInicio.HasValue)
            posts = posts.Where(p => p.DataCriacao >= dataInicio.Value).ToList();
        if (dataFim.HasValue)
            posts = posts.Where(p => p.DataCriacao <= dataFim.Value).ToList();
        var total = posts.Count();
        var paginados = posts.OrderByDescending(p => p.DataCriacao)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToList();
        var itens = new List<PostDTOSaida>();
        foreach (var post in paginados.Where(p => p.Usuario != null))
        {
            Planta? planta = post.PlantaId.HasValue ? await _repositorioPlanta.ObterPorIdAsync(post.PlantaId.Value) : null;
            var curtiu = post.Curtidas.Any(c => c.UsuarioId == usuarioId);
            string? nomeComunidade = null;
            if (post.ComunidadeId.HasValue)
            {
                var comunidade = await _repositorioComunidade.ObterPorIdAsync(post.ComunidadeId.Value);
                nomeComunidade = comunidade?.Nome;
            }
            itens.Add(MapearPostPara(post, post.Usuario!, planta, curtiu, nomeComunidade));
        }
        return Resultado<PaginaResultado<PostDTOSaida>>.Ok(new PaginaResultado<PostDTOSaida>
        {
            Itens = itens,
            Pagina = pagina,
            TamanhoPagina = tamanho,
            Total = total
        });
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

    private List<string> ExtractHashtags(string conteudo)
    {
        return Regex.Matches(conteudo, "#(\\w+)")
            .Cast<Match>()
            .Select(m => m.Value)
            .ToList();
    }

    private List<string> ExtractKeywords(string conteudo, Planta? planta)
    {
        var keywords = new List<string>();

        // Adicionar palavras do conteúdo
        keywords.AddRange(conteudo.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Adicionar palavras da planta
        if (planta != null)
        {
            keywords.Add(planta.NomeCientifico);
            if (!string.IsNullOrWhiteSpace(planta.NomeComum))
            {
                keywords.Add(planta.NomeComum);
            }
        }

        return keywords.Distinct().ToList();
    }

    public async Task<Resultado> SalvarPostAsync(Guid usuarioId, Guid postId)
    {
        if (await _repositorioPostSave.ExisteAsync(usuarioId, postId))
            return Resultado.Ok("Post já salvo");
        await _repositorioPostSave.AdicionarAsync(new PostSave { Id = Guid.NewGuid(), UsuarioId = usuarioId, PostId = postId });
        await _repositorioActivityLog.AdicionarAsync(new ActivityLog {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Tipo = "POST_SALVO",
            EntidadeId = postId,
            EntidadeTipo = "Post",
            DataCriacao = DateTime.UtcNow
        });
        return Resultado.Ok("Post salvo com sucesso");
    }

    public async Task<Resultado> CompartilharPostAsync(Guid usuarioId, Guid postId)
    {
        if (await _repositorioPostShare.ExisteAsync(usuarioId, postId))
            return Resultado.Erro("Você já compartilhou este post");
        await _repositorioPostShare.AdicionarAsync(new PostShare { Id = Guid.NewGuid(), UsuarioId = usuarioId, PostId = postId });
        await _repositorioActivityLog.AdicionarAsync(new ActivityLog {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Tipo = "POST_COMPARTILHADO",
            EntidadeId = postId,
            EntidadeTipo = "Post",
            DataCriacao = DateTime.UtcNow
        });
        return Resultado.Ok("Post compartilhado com sucesso");
    }

    public async Task<Resultado> VisualizarPostAsync(Guid usuarioId, Guid postId)
    {
        try
        {
            if (await _repositorioPostView.ExisteAsync(usuarioId, postId))
                return Resultado.Ok("Visualização já registrada");
            await _repositorioPostView.AdicionarAsync(new PostView { Id = Guid.NewGuid(), UsuarioId = usuarioId, PostId = postId });
            await _repositorioActivityLog.AdicionarAsync(new ActivityLog {
                Id = Guid.NewGuid(),
                UsuarioId = usuarioId,
                Tipo = "POST_VISUALIZADO",
                EntidadeId = postId,
                EntidadeTipo = "Post",
                DataCriacao = DateTime.UtcNow
            });
            return Resultado.Ok("Visualização registrada");
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException?.Message.Contains("ix_postview_usuario_post") == true)
        {
            // Chave duplicada: visualização já existe
            return Resultado.Ok("Visualização já registrada");
        }
        catch (Exception ex)
        {
            return Resultado.Erro($"Erro ao registrar visualização: {ex.Message}");
        }
    }

    public async Task<Resultado<ComentarioDTOSaida>> ResponderComentarioAsync(Guid usuarioId, Guid comentarioId, string conteudo)
    {
        var comentarioPai = await _repositorioPost.ObterComentarioPorIdAsync(comentarioId);
        if (comentarioPai == null)
            return Resultado<ComentarioDTOSaida>.Erro("Comentário não encontrado");
        var resposta = Comentario.Criar(comentarioPai.PostId, usuarioId, conteudo, comentarioId);
        var post = await _repositorioPost.ObterPorIdAsync(comentarioPai.PostId);
        post.AdicionarComentario(resposta);
        await _repositorioPost.AtualizarAsync(post);
        await _repositorioPost.SalvarMudancasAsync();
        var usuario = await _repositorioUsuario.ObterPorIdAsync(usuarioId);
        return Resultado<ComentarioDTOSaida>.Ok(MapearComentarioPara(resposta, usuario!));
    }

    public async Task<Resultado> RemoverPostSalvoAsync(Guid usuarioId, Guid postId)
    {
        await _repositorioPostSave.RemoverAsync(usuarioId, postId);
        return Resultado.Ok("Post removido dos salvos");
    }

    public async Task<Resultado<IEnumerable<PostDTOSaida>>> ListarPostsSalvosAsync(Guid usuarioId)
    {
        var salvos = await _repositorioPostSave.ListarPorUsuarioAsync(usuarioId);
        if (salvos.Count == 0)
            return Resultado<IEnumerable<PostDTOSaida>>.Ok(Array.Empty<PostDTOSaida>());
        var posts = await _repositorioPost.ObterPorIdsAsync(salvos.Select(s => s.PostId));
        var dtos = posts.Select(p => MapearPostPara(p, p.Usuario!, p.Planta, false)).ToList();
        return Resultado<IEnumerable<PostDTOSaida>>.Ok(dtos);
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

    public async Task<Resultado<PaginaResultado<PostDTOSaida>>> BuscarPostsAsync(string? hashtag, string? categoria, string? palavraChave, Guid? usuarioId, Guid? comunidadeId, int pagina, int tamanho)
    {
        var query = _repositorioPost.Query();
        if (!string.IsNullOrWhiteSpace(hashtag))
            query = query.Where(p => p.Hashtags.Any(h => h.Nome == hashtag));
        if (!string.IsNullOrWhiteSpace(categoria))
            query = query.Where(p => p.Categorias.Any(c => c.Nome == categoria));
        if (!string.IsNullOrWhiteSpace(palavraChave))
            query = query.Where(p => p.Conteudo.Contains(palavraChave));
        if (usuarioId.HasValue)
            query = query.Where(p => p.UsuarioId == usuarioId);
        if (comunidadeId.HasValue)
            query = query.Where(p => p.ComunidadeId == comunidadeId);
        var total = query.Count();
        var posts = query.OrderByDescending(p => p.DataCriacao)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToList();
        var itens = new List<PostDTOSaida>();
        foreach (var post in posts)
        {
            Planta? planta = post.PlantaId.HasValue ? await _repositorioPlanta.ObterPorIdAsync(post.PlantaId.Value) : null;
            var curtiu = usuarioId.HasValue && post.Curtidas.Any(c => c.UsuarioId == usuarioId.Value);
            itens.Add(MapearPostPara(post, post.Usuario!, planta, curtiu, post.Comunidade?.Nome));
        }
        return Resultado<PaginaResultado<PostDTOSaida>>.Ok(new PaginaResultado<PostDTOSaida>
        {
            Itens = itens,
            Pagina = pagina,
            TamanhoPagina = tamanho,
            Total = total
        });
    }

    public async Task<Resultado<PaginaResultado<PostDTOSaida>>> BuscarPostsPorPlantaAsync(string nomePlanta, int pagina = 1, int tamanho = 10)
    {
        var posts = await _repositorioPost.ObterTodosAsync();
        var postsFiltrados = posts
            .Where(p => p.Planta != null &&
                        (p.Planta.NomeCientifico.Contains(nomePlanta, StringComparison.OrdinalIgnoreCase) ||
                         (p.Planta.NomeComum != null && p.Planta.NomeComum.Contains(nomePlanta, StringComparison.OrdinalIgnoreCase))))
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToList();
        var dtos = postsFiltrados.Select(p => MapearPostPara(p, p.Usuario!, p.Planta, false)).ToList();
        return Resultado<PaginaResultado<PostDTOSaida>>.Ok(new PaginaResultado<PostDTOSaida>
        {
            Itens = dtos,
            Pagina = pagina,
            TamanhoPagina = tamanho,
            Total = postsFiltrados.Count
        });
    }

    public async Task<Resultado<IEnumerable<PostDTOSaida>>> BuscarPostsPorHashtagAsync(string hashtag)
    {
        var posts = await _repositorioPost.ObterPorHashtagAsync(hashtag);
        var resultado = posts.Select(p => MapearPostPara(p, p.Usuario!, p.Planta, false));
        return Resultado<IEnumerable<PostDTOSaida>>.Ok(resultado);
    }

    public async Task<Resultado<IEnumerable<PostDTOSaida>>> BuscarPostsPorCategoriaAsync(string categoria)
    {
        var posts = await _repositorioPost.ObterPorCategoriaAsync(categoria);
        var resultado = posts.Select(p => MapearPostPara(p, p.Usuario!, p.Planta, false));
        return Resultado<IEnumerable<PostDTOSaida>>.Ok(resultado);
    }

    public async Task<Resultado<IEnumerable<PostDTOSaida>>> BuscarPostsPorPalavraChaveAsync(string palavraChave)
    {
        var posts = await _repositorioPost.ObterPorPalavraChaveAsync(palavraChave);
        var resultado = posts.Select(p => MapearPostPara(p, p.Usuario!, p.Planta, false));
        return Resultado<IEnumerable<PostDTOSaida>>.Ok(resultado);
    }

    public async Task<IEnumerable<PostDTOSaida>> ObterTrendingPostsAsync(int quantidade = 10)
    {
        var posts = await _repositorioPost.ObterTodosAsync();
        var trending = posts
            .OrderByDescending(p => p.Curtidas.Count + p.Comentarios.Count)
            .ThenByDescending(p => p.DataCriacao)
            .Take(quantidade)
            .Where(p => p.Usuario != null)
            .Select(p => MapearPostPara(p, p.Usuario!, p.Planta, false))
            .ToList();
        return trending;
    }
}
