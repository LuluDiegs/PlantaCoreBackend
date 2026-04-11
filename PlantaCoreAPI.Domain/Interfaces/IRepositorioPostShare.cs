using PlantaCoreAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlantaCoreAPI.Domain.Interfaces;

public interface IRepositorioPostShare
{
    Task AdicionarAsync(PostShare postShare);
    Task RemoverAsync(Guid usuarioId, Guid postId);
    Task<bool> ExisteAsync(Guid usuarioId, Guid postId);
    Task<List<PostShare>> ListarPorUsuarioAsync(Guid usuarioId);
    Task<bool> SalvarMudancasAsync();
}
