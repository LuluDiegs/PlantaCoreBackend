using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.DTOs.Comunidade;
using PlantaCoreAPI.Application.DTOs.Post;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Domain.Comuns;
using PlantaCoreAPI.Domain.Entities;
using PlantaCoreAPI.Domain.Interfaces;

namespace PlantaCoreAPI.Infrastructure.Services;

public class ComunidadeService : IComunidadeService
{
    private readonly IRepositorioComunidade _repositorioComunidade;
    private readonly IRepositorioUsuario _repositorioUsuario;
    private readonly IRepositorioPost _repositorioPost;

    public ComunidadeService(
        IRepositorioComunidade repositorioComunidade,
        IRepositorioUsuario repositorioUsuario,
        IRepositorioPost repositorioPost)
    {
        _repositorioComunidade = repositorioComunidade;
        _repositorioUsuario = repositorioUsuario;
        _repositorioPost = repositorioPost;
    }

    public async Task<Resultado<ComunidadeDTOSaida>> CriarComunidadeAsync(Guid usuarioId, CriarComunidadeDTOEntrada entrada)
    {
        try
        {
            var usuario = await _repositorioUsuario.ObterPorIdAsync(usuarioId);
            if (usuario == null)
                return Resultado<ComunidadeDTOSaida>.Erro("Usuário não encontrado");

            var comunidade = Comunidade.Criar(usuarioId, entrada.Nome, entrada.Descricao);
            await _repositorioComunidade.AdicionarAsync(comunidade);

            var membro = MembroComunidade.Criar(comunidade.Id, usuarioId, ehAdmin: true);
            await _repositorioComunidade.AdicionarMembroAsync(membro);
            await _repositorioComunidade.SalvarMudancasAsync();

            return Resultado<ComunidadeDTOSaida>.Ok(new ComunidadeDTOSaida
            {
                Id = comunidade.Id,
                CriadorId = comunidade.CriadorId,
                NomeCriador = usuario.Nome,
                Nome = comunidade.Nome,
                Descricao = comunidade.Descricao,
                FotoComunidade = comunidade.FotoComunidade,
                TotalMembros = 1,
                UsuarioEhMembro = true,
                UsuarioEhAdmin = true,
                DataCriacao = comunidade.DataCriacao
            });
        }
        catch (Exception ex)
        {
            return Resultado<ComunidadeDTOSaida>.Erro($"Erro ao criar comunidade: {ex.Message}");
        }
    }

    public async Task<Resultado<ComunidadeDTOSaida>> AtualizarComunidadeAsync(Guid usuarioId, Guid comunidadeId, AtualizarComunidadeDTOEntrada entrada)
    {
        try
        {
            var comunidade = await _repositorioComunidade.ObterComMembrosAsync(comunidadeId);
            if (comunidade == null)
                return Resultado<ComunidadeDTOSaida>.Erro("Comunidade não encontrada");

            var membro = comunidade.Membros.FirstOrDefault(m => m.UsuarioId == usuarioId);
            if (membro == null || !membro.EhAdmin)
                return Resultado<ComunidadeDTOSaida>.Erro("Sem permissão para atualizar esta comunidade");

            comunidade.Atualizar(entrada.Nome, entrada.Descricao);
            await _repositorioComunidade.AtualizarAsync(comunidade);
            await _repositorioComunidade.SalvarMudancasAsync();

            return Resultado<ComunidadeDTOSaida>.Ok(MapearComunidade(comunidade, usuarioId));
        }
        catch (Exception ex)
        {
            return Resultado<ComunidadeDTOSaida>.Erro($"Erro ao atualizar comunidade: {ex.Message}");
        }
    }

    public async Task<Resultado> EntrarNaComunidadeAsync(Guid usuarioId, Guid comunidadeId)
    {
        try
        {
            var comunidade = await _repositorioComunidade.ObterPorIdAsync(comunidadeId);
            if (comunidade == null)
                return Resultado.Erro("Comunidade não encontrada");

            if (await _repositorioComunidade.UsuarioEhMembroAsync(comunidadeId, usuarioId))
                return Resultado.Erro("Você já é membro desta comunidade");

            var membro = MembroComunidade.Criar(comunidadeId, usuarioId);
            await _repositorioComunidade.AdicionarMembroAsync(membro);
            await _repositorioComunidade.SalvarMudancasAsync();

            return Resultado.Ok("Você entrou na comunidade com sucesso");
        }
        catch (Exception ex)
        {
            return Resultado.Erro($"Erro ao entrar na comunidade: {ex.Message}");
        }
    }

    public async Task<Resultado> SairDaComunidadeAsync(Guid usuarioId, Guid comunidadeId)
    {
        try
        {
            var comunidade = await _repositorioComunidade.ObterPorIdAsync(comunidadeId);
            if (comunidade == null)
                return Resultado.Erro("Comunidade não encontrada");

            if (comunidade.CriadorId == usuarioId)
                return Resultado.Erro("O criador não pode sair da comunidade. Transfira a administração antes de sair.");

            var membro = await _repositorioComunidade.ObterMembroAsync(comunidadeId, usuarioId);
            if (membro == null)
                return Resultado.Erro("Você não é membro desta comunidade");

            await _repositorioComunidade.RemoverMembroAsync(membro);
            await _repositorioComunidade.SalvarMudancasAsync();

            return Resultado.Ok("Você saiu da comunidade com sucesso");
        }
        catch (Exception ex)
        {
            return Resultado.Erro($"Erro ao sair da comunidade: {ex.Message}");
        }
    }

    public async Task<Resultado<ComunidadeDTOSaida>> ObterComunidadeAsync(Guid comunidadeId, Guid usuarioId)
    {
        try
        {
            var comunidade = await _repositorioComunidade.ObterComMembrosAsync(comunidadeId);
            if (comunidade == null)
                return Resultado<ComunidadeDTOSaida>.Erro("Comunidade não encontrada");

            return Resultado<ComunidadeDTOSaida>.Ok(MapearComunidade(comunidade, usuarioId));
        }
        catch (Exception ex)
        {
            return Resultado<ComunidadeDTOSaida>.Erro($"Erro ao obter comunidade: {ex.Message}");
        }
    }

    public async Task<Resultado<PaginaResultado<ComunidadeDTOSaida>>> ListarComunidadesAsync(int pagina, int tamanho, Guid usuarioId)
    {
        try
        {
            var paginaResult = await _repositorioComunidade.ListarPaginadoAsync(pagina, tamanho);

            return Resultado<PaginaResultado<ComunidadeDTOSaida>>.Ok(new PaginaResultado<ComunidadeDTOSaida>
            {
                Itens = paginaResult.Itens.Select(c => MapearComunidade(c, usuarioId)),
                Pagina = paginaResult.Pagina,
                TamanhoPagina = paginaResult.TamanhoPagina,
                Total = paginaResult.Total
            });
        }
        catch (Exception ex)
        {
            return Resultado<PaginaResultado<ComunidadeDTOSaida>>.Erro($"Erro ao listar comunidades: {ex.Message}");
        }
    }

    public async Task<Resultado<IEnumerable<ComunidadeDTOSaida>>> BuscarComunidadesAsync(string termo, Guid usuarioId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(termo))
                return Resultado<IEnumerable<ComunidadeDTOSaida>>.Erro("Termo de busca não pode estar vazio");

            var comunidades = await _repositorioComunidade.BuscarPorNomeAsync(termo);
            return Resultado<IEnumerable<ComunidadeDTOSaida>>.Ok(comunidades.Select(c => MapearComunidade(c, usuarioId)));
        }
        catch (Exception ex)
        {
            return Resultado<IEnumerable<ComunidadeDTOSaida>>.Erro($"Erro ao buscar comunidades: {ex.Message}");
        }
    }

    public async Task<Resultado<PaginaResultado<ComunidadeDTOSaida>>> ListarComunidadesDoUsuarioAsync(Guid usuarioId, int pagina, int tamanho)
    {
        try
        {
            var paginaResult = await _repositorioComunidade.ObterComunidadesDoUsuarioAsync(usuarioId, pagina, tamanho);

            return Resultado<PaginaResultado<ComunidadeDTOSaida>>.Ok(new PaginaResultado<ComunidadeDTOSaida>
            {
                Itens = paginaResult.Itens.Select(c => MapearComunidade(c, usuarioId)),
                Pagina = paginaResult.Pagina,
                TamanhoPagina = paginaResult.TamanhoPagina,
                Total = paginaResult.Total
            });
        }
        catch (Exception ex)
        {
            return Resultado<PaginaResultado<ComunidadeDTOSaida>>.Erro($"Erro ao listar comunidades do usuário: {ex.Message}");
        }
    }

    public async Task<Resultado<PaginaResultado<PostDTOSaida>>> ObterPostsComunidadeAsync(Guid comunidadeId, Guid usuarioId, int pagina, int tamanho)
    {
        try
        {
            var comunidade = await _repositorioComunidade.ObterPorIdAsync(comunidadeId);
            if (comunidade == null)
                return Resultado<PaginaResultado<PostDTOSaida>>.Erro("Comunidade não encontrada");

            var ehMembro = await _repositorioComunidade.UsuarioEhMembroAsync(comunidadeId, usuarioId);
            if (!ehMembro)
                return Resultado<PaginaResultado<PostDTOSaida>>.Erro("Você precisa ser membro da comunidade para ver os posts");

            var paginaPosts = await _repositorioPost.ObterPorComunidadeAsync(comunidadeId, pagina, tamanho);

            var itens = paginaPosts.Itens
                .Where(p => p.Usuario != null)
                .Select(p => MapearPost(p, p.Usuario!, usuarioId, comunidade.Nome))
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
            return Resultado<PaginaResultado<PostDTOSaida>>.Erro($"Erro ao obter posts da comunidade: {ex.Message}");
        }
    }

    public async Task<Resultado> ExpulsarUsuarioAsync(Guid adminId, Guid comunidadeId, Guid usuarioId)
    {
        try
        {
            var comunidade = await _repositorioComunidade.ObterComMembrosAsync(comunidadeId);
            if (comunidade == null)
                return Resultado.Erro("Comunidade não encontrada");

            var admin = comunidade.Membros.FirstOrDefault(m => m.UsuarioId == adminId);
            if (admin == null || !admin.EhAdmin)
                return Resultado.Erro("Você não tem permissão para expulsar membros desta comunidade");

            var membro = comunidade.Membros.FirstOrDefault(m => m.UsuarioId == usuarioId);
            if (membro == null)
                return Resultado.Erro("Usuário não é membro desta comunidade");

            if (membro.EhAdmin)
                return Resultado.Erro("Não é possível expulsar outro administrador");

            await _repositorioComunidade.RemoverMembroAsync(membro);
            await _repositorioComunidade.SalvarMudancasAsync();

            return Resultado.Ok("Usuário expulso com sucesso");
        }
        catch (Exception ex)
        {
            return Resultado.Erro($"Erro ao expulsar usuário: {ex.Message}");
        }
    }

    public async Task<Resultado> ExcluirComunidadeAsync(Guid adminId, Guid comunidadeId)
    {
        try
        {
            var comunidade = await _repositorioComunidade.ObterComMembrosAsync(comunidadeId);
            if (comunidade == null)
                return Resultado.Erro("Comunidade não encontrada");

            if (comunidade.CriadorId != adminId)
                return Resultado.Erro("Apenas o criador da comunidade pode excluí-la");

            await _repositorioComunidade.RemoverAsync(comunidade);
            await _repositorioComunidade.SalvarMudancasAsync();

            return Resultado.Ok("Comunidade excluída com sucesso");
        }
        catch (Exception ex)
        {
            return Resultado.Erro($"Erro ao excluir comunidade: {ex.Message}");
        }
    }

    public async Task<Resultado> TransferirAdminAsync(Guid adminId, Guid comunidadeId, Guid novoAdminId)
    {
        var comunidade = await _repositorioComunidade.ObterComMembrosAsync(comunidadeId);
        if (comunidade == null)
            return Resultado.Erro("Comunidade não encontrada");

        if (comunidade.CriadorId != adminId)
            return Resultado.Erro("Apenas o admin atual pode transferir a administração");

        var novoAdmin = comunidade.Membros.FirstOrDefault(m => m.UsuarioId == novoAdminId);
        if (novoAdmin == null)
            return Resultado.Erro("O novo admin deve ser membro da comunidade");

        var adminAtual = comunidade.Membros.FirstOrDefault(m => m.UsuarioId == adminId);
        if (adminAtual == null)
            return Resultado.Erro("Admin atual não encontrado como membro");

        adminAtual.RemoverAdmin();
        novoAdmin.PromoverAdmin();
        comunidade.GetType().GetProperty("CriadorId")?.SetValue(comunidade, novoAdminId);

        await _repositorioComunidade.AtualizarAsync(comunidade);
        await _repositorioComunidade.SalvarMudancasAsync();

        return Resultado.Ok("Admin transferido com sucesso");
    }

    private static ComunidadeDTOSaida MapearComunidade(Comunidade c, Guid usuarioId)
    {
        var membro = c.Membros.FirstOrDefault(m => m.UsuarioId == usuarioId);
        return new ComunidadeDTOSaida
        {
            Id = c.Id,
            CriadorId = c.CriadorId,
            NomeCriador = c.Criador?.Nome ?? string.Empty,
            Nome = c.Nome,
            Descricao = c.Descricao,
            FotoComunidade = c.FotoComunidade,
            TotalMembros = c.Membros.Count,
            UsuarioEhMembro = membro != null,
            UsuarioEhAdmin = membro?.EhAdmin ?? false,
            DataCriacao = c.DataCriacao
        };
    }

    private static PostDTOSaida MapearPost(Domain.Entities.Post post, Domain.Entities.Usuario usuario, Guid usuarioAutenticadoId, string? nomeComunidade)
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
            Conteudo = post.Conteudo,
            TotalCurtidas = post.Curtidas.Count,
            TotalComentarios = post.Comentarios.Count,
            CurtiuUsuario = post.Curtidas.Any(c => c.UsuarioId == usuarioAutenticadoId),
            DataCriacao = post.DataCriacao,
            DataAtualizacao = post.DataAtualizacao
        };
    }
}
