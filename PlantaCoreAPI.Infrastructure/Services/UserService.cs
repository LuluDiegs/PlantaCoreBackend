using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.DTOs.Planta;
using PlantaCoreAPI.Application.DTOs.Post;
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
    private readonly IRepositorioPlanta _repositorioPlanta;
    private readonly IRepositorioNotificacao _repositorioNotificacao;
    private readonly IRepositorioSolicitacaoSeguir _repositorioSolicitacao;
    private readonly IFileStorageService _fileStorageService;
    private readonly IAccountDeletionService _accountDeletionService;
    private readonly IAccountReactivationService _accountReactivationService;

    public UserService(
        IRepositorioUsuario repositorioUsuario,
        IRepositorioPost repositorioPost,
        IRepositorioPlanta repositorioPlanta,
        IRepositorioNotificacao repositorioNotificacao,
        IRepositorioSolicitacaoSeguir repositorioSolicitacao,
        IFileStorageService fileStorageService,
        IAccountDeletionService accountDeletionService,
        IAccountReactivationService accountReactivationService)
    {
        _repositorioUsuario = repositorioUsuario;
        _repositorioPost = repositorioPost;
        _repositorioPlanta = repositorioPlanta;
        _repositorioNotificacao = repositorioNotificacao;
        _repositorioSolicitacao = repositorioSolicitacao;
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

            return Resultado<UsuarioDTOSaida>.Ok(new UsuarioDTOSaida
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
                TotalPosts = totalPosts.Count(),
                TotalCurtidasRecebidas = totalCurtidas,
                DataCriacao = usuario.DataCriacao
            });
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

            var userSegueEste = false;
            var solicitacaoPendente = false;

            if (usuarioAutenticadoId != Guid.Empty)
            {
                var usuarioAutenticado = await _repositorioUsuario.ObterComPlantasAsync(usuarioAutenticadoId);
                userSegueEste = usuarioAutenticado?.Seguindo.Any(u => u.Id == usuarioId) ?? false;

                if (!userSegueEste && usuario.PerfilPrivado)
                    solicitacaoPendente = await _repositorioSolicitacao.ExisteSolicitacaoPendenteAsync(usuarioAutenticadoId, usuarioId);
            }

            var totalPosts = await _repositorioPost.ObterPorUsuarioAsync(usuarioId);

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
                TotalPosts = totalPosts.Count(),
                UserSegueEste = userSegueEste,
                SolicitacaoPendente = solicitacaoPendente
            });
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

            return Resultado.Ok(privado ? "Perfil alterado para privado" : "Perfil alterado para público");
        }
        catch (Exception ex)
        {
            return Resultado.Erro($"Erro ao alterar privacidade do perfil: {ex.Message}");
        }
    }

    public async Task<Resultado> ExcluirContaAsync(Guid usuarioId)
    {
        try { return await _accountDeletionService.ExcluirContaCompleteAsync(usuarioId); }
        catch (Exception ex) { return Resultado.Erro($"Erro ao excluir conta: {ex.Message}"); }
    }

    public async Task<Resultado> SolicitarReativacaoAsync(string email) =>
        await _accountReactivationService.SolicitarReativacaoAsync(email);

    public async Task<Resultado> ReativarComTokenAsync(string email, string token, string novaSenha) =>
        await _accountReactivationService.ReativarComTokenAsync(email, token, novaSenha);

    public async Task<Resultado> VerificarTokenReativacaoAsync(string email, string token) =>
        await _accountReactivationService.VerificarTokenReativacaoAsync(email, token);

    public async Task<Resultado> ResetarSenhaSemTokenAsync(string email, string novaSenha) =>
        await _accountReactivationService.ResetarSenhaSemTokenAsync(email, novaSenha);

    public async Task<Resultado> SegurUserAsync(Guid usuarioId, Guid usuarioParaSeguirId)
    {
        try
        {
            var usuario = await _repositorioUsuario.ObterPorIdAsync(usuarioId);
            var usuarioParaSeguir = await _repositorioUsuario.ObterPorIdAsync(usuarioParaSeguirId);

            if (usuario == null || usuarioParaSeguir == null)
                return Resultado.Erro("Usuário não encontrado");

            if (usuarioParaSeguir.PerfilPrivado)
                return Resultado.Erro("Este perfil é privado. Use enviar solicitação de seguir.");

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

            var jaSegue = solicitante.Seguindo.Any(u => u.Id == alvoId);
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
            return Resultado.Erro($"Erro ao enviar solicitação: {ex.Message}");
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
            await _repositorioSolicitacao.AtualizarAsync(solicitacao);

            var solicitante = await _repositorioUsuario.ObterPorIdAsync(solicitacao.SolicitanteId);
            var alvoUsuario = await _repositorioUsuario.ObterPorIdAsync(alvoId);

            if (solicitante != null && alvoUsuario != null)
            {
                solicitante.Seguir(alvoUsuario);
                await _repositorioUsuario.AtualizarAsync(solicitante);

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
            return Resultado.Erro($"Erro ao aceitar solicitação: {ex.Message}");
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
            return Resultado.Erro($"Erro ao rejeitar solicitação: {ex.Message}");
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
            return Resultado<IEnumerable<SolicitacaoSeguirDTOSaida>>.Erro($"Erro ao listar solicitações: {ex.Message}");
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
            return Resultado<PaginaResultado<PlantaDTOSaida>>.Erro($"Erro ao listar plantas: {ex.Message}");
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

            var paginaPosts = await _repositorioPost.ObterPorUsuarioPaginadoAsync(usuarioId, pagina, tamanho);
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
            return Resultado<PaginaResultado<PostDTOSaida>>.Erro($"Erro ao listar posts do perfil: {ex.Message}");
        }
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
        Toxica = planta.Toxica ? "Sim" : "Não",
        DescricaoToxicidade = planta.DescricaoToxicidade,
        ToxicaAnimais = planta.ToxicaAnimais ? "Sim" : "Não",
        DescricaoToxicidadeAnimais = planta.DescricaoToxicidadeAnimais,
        ToxicaCriancas = planta.ToxicaCriancas ? "Sim" : "Não",
        DescricaoToxicidadeCriancas = planta.DescricaoToxicidadeCriancas,
        RequisitosLuz = planta.RequisitosLuz,
        RequisitosAgua = planta.RequisitosAgua,
        RequisitosTemperatura = planta.RequisitosTemperatura,
        Cuidados = planta.Cuidados,
        FotoPlanta = planta.FotoPlanta,
        DataIdentificacao = planta.DataIdentificacao
    };
}
