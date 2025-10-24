using System.Text.Json.Serialization;

namespace FreelaMatchAPI.Models
{
    public enum ExperienceLevel
    {
        Junior = 1,
        Pleno,
        Senior,
        Especialista
    }

    public class Profile
    {
        public int ProfileId { get; set; }

        [JsonIgnore]
        public User User { get; set; } 

        public int UserId { get; set; }

        public string? Biography { get; set; }

        public ExperienceLevel ExperienceLevel { get; set; }

        public int PricePerHour { get; set; }

        public ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
    }

    public class UpdateProfile
    {
        public string? Biography { get; set; }

        public ExperienceLevel ExperienceLevel { get; set; }

        public int PricePerHour { get; set; }

        public ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
    }

    public class Skill
    {
        public int SkillId { get; set; }
        public string Name { get; set; } = string.Empty;

        [JsonIgnore]
        public ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
    }

    public class UserSkill
    {
        public int UserSkillId { get; set; }

        public int UserId { get; set; }

        public int ProfileId { get; set; }

        [JsonIgnore]
        public Profile? Profile { get; set; }

        [JsonIgnore]
        public User? User { get; set; }

        public int SkillId { get; set; }

        public Skill? Skill { get; set; }

        public bool IsActive { get; set; }
    }
}
