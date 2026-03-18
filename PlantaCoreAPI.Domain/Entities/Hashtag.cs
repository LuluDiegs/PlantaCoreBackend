using System;

namespace PlantaCoreAPI.Domain.Entities
{
    public class Hashtag
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = null!;
        public Guid PostId { get; set; }
        public Post Post { get; set; } = null!;
    }
}