namespace FreelaMatchAPI.Models
{
    public enum UserType { Freelancer = 1, Company }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public UserType Type { get; set; }
        public string Token { get; set; }
        public Profile? Profile { get; set; }
        public bool IsAvailable { get; set; }

        public ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();

        // Avaliações feitas por esse usuário
        public ICollection<Reviews> ReviewsGiven { get; set; }

        // Avaliações recebidas por esse usuário
        public ICollection<Reviews> ReviewsReceived { get; set; }
    }
    public class UserResume
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public UserType Type { get; set; }
        public ProfileResume? Profile { get; set; }
        public bool IsAvailable { get; set; }

        public ICollection<UserSkillResume> UserSkills { get; set; } = new List<UserSkillResume>();
    }

    public class UpdateUser
    {
        public string Name { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class Freelancer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Bio { get; set; }
        public string Skills { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }

    public class Company
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string Description { get; set; }
        public string Industry { get; set; }
        public string ContactPerson { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
