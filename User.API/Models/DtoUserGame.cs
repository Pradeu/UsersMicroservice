using User.DB.Entities;

namespace User.API.Models
{
    public class DtoUserGame
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int GameId { get; set; }
    }
}
