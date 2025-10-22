namespace FreelaMatchAPI.Models
{
    public enum UserType { Freelancer, Company }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public UserType Type { get; set; }
        public string Token { get; set; }
        public Profile? Profile { get; set; }

        public ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
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
