using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.DTOs.Post;
using PlantaCoreAPI.Application.DTOs.Comentario;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Domain.Comuns;
using PlantaCoreAPI.Domain.Entities;
using PlantaCoreAPI.Domain.Interfaces;

namespace PlantaCoreAPI.Infrastructure.Services;

public class PostService : IPostService
{
    private readonly IRepositorioPost _repositorioPost;
    private readonly IRepositorioUsuario _repositorioUsuario;
    private readonly IRepositorioPlanta _repositorioPlanta;
    private readonly IRepositorioNotificacao _repositorioNotificacao;

    public PostService(
        IRepositorioPost repositorioPost,
        IRepositorioUsuario repositorioUsuario,
        IRepositorioPlanta repositorioPlanta,
        IRepositorioNotificacao repositorioNotificacao)
    {
        _repositorioPost = repositorioPost;
        _repositorioUsuario = repositorioUsuario;
        _repositorioPlanta = repositorioPlanta;
        _repositorioNotificacao = repositorioNotificacao;
    }

    public async Task<Resultado<PostDTOSaida>> CriarPostAsync(Guid usuarioId, CriarPostDTOEntrada entrada)
    {
        try
        {
            var usuario = await _repositorioUsuario.ObterPorIdAsync(usuarioId);
            var planta = await _repositorioPlanta.ObterPorIdAsync(entrada.PlantaId);

            if (usuario == null || planta == null)
                return Resultado<PostDTOSaida>.Erro("Usuário ou planta não encontrado");

            var post = Post.Criar(usuarioId, entrada.PlantaId, entrada.Conteudo);
            await _repositorioPost.AdicionarAsync(post);
            await _repositorioPost.SalvarMudancasAsync();

            return Resultado<PostDTOSaida>.Ok(MapearPostPara(post, usuario, planta, false));
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
            var planta = await _repositorioPlanta.ObterPorIdAsync(post.PlantaId);
            return Resultado<PostDTOSaida>.Ok(MapearPostPara(post, usuario!, planta!, false));
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
            var planta = await _repositorioPlanta.ObterPorIdAsync(post.PlantaId);
            var curtiu = post.Curtidas.Any(c => c.UsuarioId == usuarioId);

            return Resultado<PostDTOSaida>.Ok(MapearPostPara(post, usuario!, planta!, curtiu));
        }
        catch (Exception ex)
        {
            return Resultado<PostDTOSaida>.Erro($"Erro ao obter post: {ex.Message}");
        }
    }

    public async Task<Resultado<IEnumerable<PostDTOSaida>>> ObterFeedAsync(Guid usuarioId, int pagina = 1, int tamanho = 10)
    {
        try
        {
            var posts = await _repositorioPost.ObterFeedAsync(usuarioId, pagina, tamanho);

            var dtos = new List<PostDTOSaida>();
            foreach (var post in posts.Where(p => p.Usuario != null))
            {
                var planta = await _repositorioPlanta.ObterPorIdAsync(post.PlantaId);
                var curtiu = post.Curtidas.Any(c => c.UsuarioId == usuarioId);
                dtos.Add(MapearPostPara(post, post.Usuario!, planta!, curtiu));
            }

            return Resultado<IEnumerable<PostDTOSaida>>.Ok(dtos);
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
            var post = await _repositorioPost.ObterPorIdAsync(postId);
            if (post == null)
                return Resultado.Erro("Post não encontrado");

            // Validar se o usuário está tentando curtir seu próprio post
            if (post.UsuarioId == usuarioId)
                return Resultado.Erro("Você não pode curtir seu próprio post");

            var usuario = await _repositorioUsuario.ObterPorIdAsync(usuarioId);
            if (usuario == null)
                return Resultado.Erro("Usuário não encontrado");

            post.AdicionarCurtida(usuario);
            await _repositorioPost.AtualizarAsync(post);
            await _repositorioPost.SalvarMudancasAsync();

            var notificacao = Notificacao.Criar(
                post.UsuarioId,
                Domain.Enums.TipoNotificacao.Curtida,
                $"{usuario.Nome} curtiu seu post",
                usuarioId,
                null,
                postId);

            await _repositorioNotificacao.AdicionarAsync(notificacao);
            await _repositorioNotificacao.SalvarMudancasAsync();

            return Resultado.Ok("Post curtido com sucesso");
        }
        catch (Exception ex)
        {
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

            var comentario = Comentario.Criar(entrada.PostId, usuarioId, entrada.Conteudo);
            post.AdicionarComentario(comentario);

            await _repositorioPost.AtualizarAsync(post);
            await _repositorioPost.SalvarMudancasAsync();

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

    public async Task<Resultado<IEnumerable<ComentarioDTOSaida>>> ListarComentariosPostAsync(Guid postId, Guid usuarioAutenticadoId, int pagina = 1, int tamanho = 20)
    {
        try
        {
            var post = await _repositorioPost.ObterPorIdAsync(postId);
            if (post == null)
                return Resultado<IEnumerable<ComentarioDTOSaida>>.Erro("Post não encontrado");

            var comentarios = post.Comentarios
                .Where(c => c.Ativo)
                .OrderByDescending(c => c.DataCriacao)
                .Skip((pagina - 1) * tamanho)
                .Take(tamanho)
                .ToList();

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
                .Select(c => MapearComentarioPara(c, usuarios[c.UsuarioId], usuarioAutenticadoId))
                .ToList();

            return Resultado<IEnumerable<ComentarioDTOSaida>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            return Resultado<IEnumerable<ComentarioDTOSaida>>.Erro($"Erro ao listar comentários: {ex.Message}");
        }
    }

    public async Task<Resultado<PaginaResultado<PostDTOSaida>>> ListarPostsUsuarioAsync(Guid usuarioId, Guid usuarioAutenticadoId, int pagina, int tamanho)
    {
        try
        {
            var paginaPosts = await _repositorioPost.ObterPorUsuarioPaginadoAsync(usuarioId, pagina, tamanho);

            var itens = new List<PostDTOSaida>();
            foreach (var post in paginaPosts.Itens.Where(p => p.Usuario != null))
            {
                var planta = await _repositorioPlanta.ObterPorIdAsync(post.PlantaId);
                itens.Add(MapearPostPara(post, post.Usuario!, planta!, post.Curtidas.Any(c => c.UsuarioId == usuarioAutenticadoId)));
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

    public async Task<Resultado<PaginaResultado<PostDTOSaida>>> ObterExploradorAsync(Guid usuarioAutenticadoId, int pagina, int tamanho)
    {
        try
        {
            var paginaPosts = await _repositorioPost.ObterExploradorAsync(pagina, tamanho);

            var itens = new List<PostDTOSaida>();
            foreach (var post in paginaPosts.Itens.Where(p => p.Usuario != null))
            {
                var planta = await _repositorioPlanta.ObterPorIdAsync(post.PlantaId);
                itens.Add(MapearPostPara(post, post.Usuario!, planta!, post.Curtidas.Any(c => c.UsuarioId == usuarioAutenticadoId)));
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
                var planta = await _repositorioPlanta.ObterPorIdAsync(post.PlantaId);
                dtos.Add(MapearPostPara(post, post.Usuario!, planta!, post.Curtidas.Any(c => c.UsuarioId == usuarioAutenticadoId)));
            }

            return Resultado<IEnumerable<PostDTOSaida>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            return Resultado<IEnumerable<PostDTOSaida>>.Erro($"Erro ao listar posts curtidos: {ex.Message}");
        }
    }

    private PostDTOSaida MapearPostPara(Post post, Usuario usuario, Planta planta, bool curtiu)
    {
        return new PostDTOSaida
        {
            Id = post.Id,
            PlantaId = post.PlantaId,
            UsuarioId = post.UsuarioId,
            NomeUsuario = usuario.Nome,
            FotoUsuario = usuario.FotoPerfil,
            NomePlanta = planta.NomeComum ?? planta.NomeCientifico,
            FotoPlanta = planta.FotoPlanta,
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
}
