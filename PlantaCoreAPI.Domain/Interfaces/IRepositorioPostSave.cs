using PlantaCoreAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlantaCoreAPI.Domain.Interfaces;

public interface IRepositorioPostSave
{
    Task AdicionarAsync(PostSave postSave);
    Task RemoverAsync(Guid usuarioId, Guid postId);
    Task<bool> ExisteAsync(Guid usuarioId, Guid postId);
    Task<List<PostSave>> ListarPorUsuarioAsync(Guid usuarioId);
}
