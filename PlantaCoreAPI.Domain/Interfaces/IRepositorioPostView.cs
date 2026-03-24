using PlantaCoreAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlantaCoreAPI.Domain.Interfaces;

public interface IRepositorioPostView
{
    Task AdicionarAsync(PostView postView);
    Task RemoverAsync(Guid usuarioId, Guid postId);
    Task<bool> ExisteAsync(Guid usuarioId, Guid postId);
    Task<List<PostView>> ListarPorUsuarioAsync(Guid usuarioId);
}
