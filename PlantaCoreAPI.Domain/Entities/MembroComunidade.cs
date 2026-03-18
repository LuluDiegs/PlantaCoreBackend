namespace PlantaCoreAPI.Domain.Entities;

public class MembroComunidade
{
    public Guid Id { get; private set; }
    public Guid ComunidadeId { get; private set; }
    public Comunidade? Comunidade { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Usuario? Usuario { get; private set; }
    public bool EhAdmin { get; private set; }
    public DateTime DataEntrada { get; private set; }

    private MembroComunidade() { }

    public static MembroComunidade Criar(Guid comunidadeId, Guid usuarioId, bool ehAdmin = false)
    {
        return new MembroComunidade
        {
            Id = Guid.NewGuid(),
            ComunidadeId = comunidadeId,
            UsuarioId = usuarioId,
            EhAdmin = ehAdmin,
            DataEntrada = DateTime.UtcNow
        };
    }

    public void PromoverAdmin()
    {
        EhAdmin = true;
    }
}
