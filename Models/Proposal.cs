using System.Text.Json.Serialization;

namespace FreelaMatchAPI.Models
{
    public enum ProposalStatus
    {
        Pending = 1,
        Accepted,
        Rejected
    }

    public class Proposal
    {
        public int ProposalId { get; set; }
        public string Title { get; set; }
        public DateTime MaxDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public int OwnerId { get; set; }
        public User Owner { get; set; }
        public bool IsAvailable { get; set; }
        public int Price { get; set; }
        public string Description { get; set; }

        public ICollection<Candidate> Candidates { get; set; } = new List<Candidate>();
        public ICollection<ProposalSkill> RequiredSkills { get; set; } = new List<ProposalSkill>();
    }

    public class Candidate
    {
        public int CandidateId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int ProposalId { get; set; }
        [JsonIgnore]
        public Proposal Proposal { get; set; }
        public DateTime AppliedAt { get; set; }
        public int ProposedPrice { get; set; }
        public ProposalStatus Status { get; set; }
    }

    public class ProposalSkill
    {
        public int ProposalSkillId { get; set; }

        public int ProposalId { get; set; }
        [JsonIgnore]
        public Proposal Proposal { get; set; }

        public int SkillId { get; set; }
        public UserSkill Skill { get; set; }
    }
}
