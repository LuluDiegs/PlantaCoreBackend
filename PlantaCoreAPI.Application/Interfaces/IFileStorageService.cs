namespace PlantaCoreAPI.Application.Interfaces;

public interface IFileStorageService
{
    Task<List<string>> ListarTodosArquivosAsync();
    Task<string> FazerUploadAsync(byte[] bytes, string nomeArquivo, string tipoConteudo);
    Task<string> FazerUploadFotoPerfilAsync(byte[] bytes, string nomeArquivo, Guid usuarioId);
    Task<string> FazerUploadFotoPlantaAsync(byte[] bytes, string nomeArquivo, Guid usuarioId);
    Task<bool> ExcluirArquivoAsync(string nomeArquivo);
    Task<bool> DeletarFotoPerfilAsync(string urlFoto, Guid usuarioId);
    Task<bool> DeletarFotoPlantaAsync(string urlFoto, Guid usuarioId);
    Task<int> ExcluirTodosArquivosAsync();
}
