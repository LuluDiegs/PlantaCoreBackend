using System;

namespace PlantaCoreAPI.Domain.Entities
{
    public class PalavraChave
    {
        public Guid Id { get; set; }
        public string Palavra { get; set; } = null!;
        public Guid PostId { get; set; }
        public Post Post { get; set; } = null!;
    }
}
