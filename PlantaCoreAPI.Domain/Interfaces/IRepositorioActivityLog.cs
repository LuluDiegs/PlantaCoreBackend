using PlantaCoreAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlantaCoreAPI.Domain.Interfaces;

public interface IRepositorioActivityLog
{
    Task AdicionarAsync(ActivityLog log);
    Task<List<ActivityLog>> ListarPorUsuarioAsync(Guid usuarioId, int pagina = 1, int tamanho = 20);
}
