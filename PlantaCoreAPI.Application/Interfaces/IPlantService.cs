using PlantaCoreAPI.Application.Comuns;
using PlantaCoreAPI.Application.DTOs.Identificacao;
using PlantaCoreAPI.Application.DTOs.Planta;
using PlantaCoreAPI.Application.DTOs.Post;
using PlantaCoreAPI.Domain.Comuns;

namespace PlantaCoreAPI.Application.Interfaces;

public interface IPlantService
{
    Task<Resultado<PlantaDTOSaida>> IdentificarPlantaAsync(Guid usuarioId, IdentificacaoDTOEntrada entrada);
    Task<Resultado<PlantaDTOSaida>> BuscarPlantaAsync(Guid usuarioId, BuscaPlantaDTOEntrada entrada);
    Task<Resultado<ResultadoBuscaPlantaDTOSaida>> BuscarPlantasTrefleAsync(string nomePlanta, int pagina);
    Task<Resultado<PlantaDTOSaida>> AdicionarPlantaDoTrefleAsync(Guid usuarioId, int plantaTrefleId, string? nomeCientifico, string? urlImagem);
    Task<Resultado<IEnumerable<PlantaDTOSaida>>> ListarPlantasUsuarioAsync(Guid usuarioId);
    Task<Resultado<PaginaResultado<PlantaDTOSaida>>> ListarPlantasUsuarioPaginadoAsync(Guid usuarioId, int pagina, int tamanho);
    Task<Resultado<PlantaDTOSaida>> ObterPlantaAsync(Guid plantaId);
    Task<Resultado<bool>> ExcluirPlantaAsync(Guid plantaId, Guid usuarioId);
    Task<Resultado<PaginaResultado<PlantaDTOSaida>>> BuscarPlantasUsuarioAsync(Guid usuarioId, string termo, int pagina, int tamanho);
    Task<Resultado<PostDTOSaida>> PostarFotoIdentificacaoAsync(Guid usuarioId, Guid plantaId, string conteudo);
    Task<IEnumerable<PlantaCoreAPI.Application.DTOs.Planta.PlantaDTOSaida>> BuscarPlantasPorNomeAsync(string termo);
    Task<IEnumerable<PostDTOSaida>> ListarPostsDaPlantaAsync(Guid plantaId);
}
