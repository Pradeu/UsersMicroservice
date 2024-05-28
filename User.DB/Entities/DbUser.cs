
using System.Text.Json.Serialization;

namespace User.DB.Entities
{
    public class DbUser
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        [JsonIgnore] public string Password { get; set; }

        public ICollection<DbUserGame> UserGames { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    }
}
