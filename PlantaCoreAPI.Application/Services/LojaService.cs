using Microsoft.Extensions.Logging;
using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.Interfaces;
using PlantaCoreAPI.Domain.Entities;
using PlantaCoreAPI.Domain.Interfaces;

namespace PlantaCoreAPI.Application.Services;

public class LojaService : ILojaService
{
    private readonly IRepositorioLoja _repositorioLoja;
    private readonly ILogger<ILojaService> _logger;

    public LojaService(
        IRepositorioLoja repositorioLoja,
        ILogger<ILojaService> logger)
    {
        _repositorioLoja = repositorioLoja;
        _logger = logger;
    }

    public IEnumerable<string> Validar(Loja loja)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(loja.Nome))
            erros.Add("Nome é obrigatório.");

        else if (loja.Nome.Length > 150)
            erros.Add("Nome deve ter no máximo 150 caracteres.");

        if (!string.IsNullOrWhiteSpace(loja.Email))
        {
            try
            {
                new System.Net.Mail.MailAddress(loja.Email);
            }
            catch
            {
                erros.Add("Email inválido.");
            }
        }

        if (!string.IsNullOrWhiteSpace(loja.Telefone) && loja.Telefone.Length > 20)
            erros.Add("Telefone deve ter no máximo 20 caracteres.");

        if (!string.IsNullOrWhiteSpace(loja.ImagemUrl) &&
            !Uri.TryCreate(loja.ImagemUrl, UriKind.Absolute, out _))
        {
            erros.Add("ImagemUrl deve ser uma URL válida.");
        }

        if (!loja.SomenteOnline)
        {
            if (string.IsNullOrWhiteSpace(loja.Cidade))
                erros.Add("Cidade é obrigatória para lojas físicas.");

            if (string.IsNullOrWhiteSpace(loja.Estado))
                erros.Add("Estado é obrigatório para lojas físicas.");

            if (string.IsNullOrWhiteSpace(loja.Endereco))
                erros.Add("Endereço é obrigatório para lojas físicas.");
        }

        return erros;
    }

    public async Task<Resultado<IEnumerable<Loja>>> ObterTodosAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<Loja> lojas = await _repositorioLoja
                .ObterTodosAsync(cancellationToken);

            return Resultado<IEnumerable<Loja>>.Ok(lojas);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return Resultado<IEnumerable<Loja>>.Erro("Erro ao buscar lojas");
        }
    }

    public async Task<Resultado<IEnumerable<Loja>>> ObterPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<Loja> lojas = await _repositorioLoja
                .ObterPorUsuarioAsync(usuarioId, cancellationToken);

            return Resultado<IEnumerable<Loja>>.Ok(lojas);   
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return Resultado<IEnumerable<Loja>>.Erro("Erro ao buscar lojas");
        }
    }

    public async Task<Resultado<Loja>> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            Loja? loja = await _repositorioLoja
                .ObterPorIdAsync(id, cancellationToken);

            if (loja is null)
            {
                return Resultado<Loja>.Erro("Loja não encontrada");
            }

            return Resultado<Loja>.Ok(loja);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return Resultado<Loja>.Erro("Erro ao buscar loja");
        }
    }

    public async Task<Resultado> AdicionarAsync(
        Loja loja,
        CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<string> mensagensErro = Validar(loja);
            if (mensagensErro.Any())
            {
                return Resultado.Erro("Loja inválida", mensagensErro);
            }

            await _repositorioLoja.AdicionarAsync(loja, cancellationToken);
            await _repositorioLoja.SalvarMudancasAsync(cancellationToken);

            return Resultado.Ok("Loja criada com sucesso");
        } 
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return Resultado.Erro("Erro ao criar loja");
        }
    }

    public async Task<Resultado> AtualizarAsync(
        Loja loja,
        CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<string> mensagensErro = Validar(loja);
            if (mensagensErro.Any())
            {
                return Resultado.Erro("Loja inválida", mensagensErro);
            }

            _repositorioLoja.Atualizar(loja);
            await _repositorioLoja.SalvarMudancasAsync(cancellationToken);

            return Resultado.Ok("Loja atualizada com sucesso");   
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return Resultado.Erro("Erro ao atualizar loja");
        }
    }

    public async Task<Resultado> RemoverAsync(
        Guid lojaId,
        Guid usuarioId,
        CancellationToken cancellationToken)
    {
        try
        {
            Resultado<Loja> resultado = await ObterPorIdAsync(lojaId, cancellationToken);
            if (!resultado.Sucesso)
            {
                return Resultado.Erro(resultado.Mensagem ?? "Erro");
            }
            if (resultado.Dados is null)
            {
                return Resultado.Erro("Dados da loja estão vazios");
            }

            Loja loja = resultado.Dados;
            if (loja.UsuarioId != usuarioId)
            {
                return Resultado.Erro("Sem permissão para remover essa loja");
            }

            _repositorioLoja.Remover(loja);
            await _repositorioLoja.SalvarMudancasAsync(cancellationToken);

            return Resultado.Ok("Loja removida com sucesso");
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return Resultado.Erro("Erro ao remover loja");
        }
    }
}