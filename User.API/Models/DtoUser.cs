using System.Text.Json.Serialization;

namespace User.API.Models
{
    public class DtoUser
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }

        public ICollection<int>? GamesList { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}
