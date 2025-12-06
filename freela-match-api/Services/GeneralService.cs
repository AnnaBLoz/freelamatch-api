using FreelaMatchAPI.Data;
using FreelaMatchAPI.Interfaces;
using FreelaMatchAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreelaMatchAPI.Services
{
    public class GeneralService : IGeneralService
    {
        private readonly AppDbContext _context;

        public GeneralService(AppDbContext context)
        {
            _context = context;
        }

        public Task<List<User?>> GetFreelancers()
        {
            return _context.Users
                .Where(f => f.Type == UserType.Freelancer)
                .Include(f => f.Profile).ThenInclude(f => f.UserSkills).ThenInclude(f => f.Skill)
                .ToListAsync();
        }

        public Task<List<Sector?>> GetSectors()
        {
            return _context.Sector
                .ToListAsync();
        }

        public Task<List<Skill?>> GetSkills()
        {
            return _context.Skills
                .ToListAsync();
        }

        public Task<List<Candidate?>> CompletedProjects(int userId)
        {
            return _context.Candidate
                .Where(c => c.UserId == userId && c.Status == ProposalStatus.Accepted)
                .ToListAsync();
        }

        public async Task<List<UserResume>> Match(int companyUserId)
        {
            // 1 — Buscar skills necessárias nos projetos em aberto
            var requiredSkills = await _context.Proposal
                .Where(p => p.OwnerId == companyUserId && p.IsAvailable)
                .SelectMany(p => p.RequiredSkills.Select(rs => rs.SkillId))
                .Distinct()
                .ToListAsync();

            if (!requiredSkills.Any())
                return new List<UserResume>();

            // 2 — Buscar freelancers com Includes e cálculo
            var freelancers = await _context.Users
                .Where(u => u.Type == UserType.Freelancer && u.IsAvailable)
                .Include(u => u.Profile)
                .Include(u => u.UserSkills)
                    .ThenInclude(us => us.Skill)
                .Select(u => new
                {
                    User = u,
                    MatchSkills = u.UserSkills.Count(us => requiredSkills.Contains(us.SkillId)),
                    AvgRating = _context.Reviews
                        .Where(r => r.ReviewerId == u.Id)
                        .Average(r => (double?)r.Rating) ?? 0
                })
                .Where(f => f.MatchSkills > 0)
                .ToListAsync();

            // 3 — Ordenar e pegar top 5
            var topFreelancers = freelancers
                .OrderByDescending(f => f.MatchSkills)
                .ThenByDescending(f => f.AvgRating)
                .Take(5)
                .Select(f => f.User)
                .ToList();

            // 4 — Mapear User -> UserResume
            var result = topFreelancers.Select(u => new UserResume
            {
                Id = u.Id,
                Name = u.Name,
                Type = u.Type,
                IsAvailable = u.IsAvailable,

                Profile = u.Profile == null ? null : new ProfileResume
                {
                    Biography = u.Profile.Biography,
                    ExperienceLevel = u.Profile.ExperienceLevel,
                    PricePerHour = u.Profile.PricePerHour,
                },

                UserSkills = u.UserSkills
                    .Select(us => new UserSkillResume
                    {
                        SkillId = us.SkillId,
                        Name = us.Skill?.Name ?? ""
                    })
                    .ToList()
            })
            .ToList();

            return result;
        }
    }
}
