using System.Text.Json.Serialization;

namespace FreelaMatchAPI.Models
{
    public class Reviews
    {
        public int Id { get; set; }

        // Quem fez a avalia��o
        public int ReviewerId { get; set; }
        [JsonIgnore]
        public User Reviewer { get; set; }

        // Quem recebeu a avalia��o
        public int ReceiverId { get; set; }
        [JsonIgnore]
        public User Receiver { get; set; }

        // Conte�do da avalia��o
        public string ReviewText { get; set; } = string.Empty;

        // Nota (1 a 5, por exemplo)
        public int Rating { get; set; }

        // Data opcional
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ReviewCreate
    {

        // Quem fez a avalia��o
        public int ReviewerId { get; set; }

        // Quem recebeu a avalia��o
        public int ReceiverId { get; set; }

        // Conte�do da avalia��o
        public string ReviewText { get; set; } = string.Empty;

        // Nota (1 a 5, por exemplo)
        public int Rating { get; set; }

        // Data opcional
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
