using FreelaMatchAPI.Models;

namespace FreelaMatchAPI.DTOs
{
    public class RegisterDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public UserType Type { get; set; } // Freelancer ou Company
        //public string Name { get; set; }
        //public string Bio { get; set; }
        //public string CompanyName { get; set; }
        //public string Description { get; set; }
        //public string Industry { get; set; }
        //public string ContactPerson { get; set; }
    }
}
