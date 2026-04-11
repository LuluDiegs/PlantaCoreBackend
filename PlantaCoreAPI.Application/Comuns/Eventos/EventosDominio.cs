using System;

namespace PlantaCoreAPI.Application.Comuns.Eventos;

public interface IEventoDominio { }
public class UsuarioSeguidoEvento : IEventoDominio
{
    public Guid SeguidorId { get; set; }
    public Guid SeguidoId { get; set; }
}

public class PostCurtidoEvento : IEventoDominio
{
    public Guid UsuarioId { get; set; }
    public Guid PostId { get; set; }
}

public class ComentarioCriadoEvento : IEventoDominio
{
    public Guid UsuarioId { get; set; }
    public Guid PostId { get; set; }
    public Guid ComentarioId { get; set; }
}

public class PlantaIdentificadaEvento : IEventoDominio
{
    public Guid UsuarioId { get; set; }
    public Guid PlantaId { get; set; }
}

public class LembreteCuidadoCriadoEvento : IEventoDominio
{
    public Guid UsuarioId { get; set; }
    public Guid PlantaId { get; set; }
}
