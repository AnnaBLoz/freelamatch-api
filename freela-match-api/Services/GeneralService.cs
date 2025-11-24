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
                //.Include(f => f.Profile).ThenInclude(f => f.UserSkills).ThenInclude(f => f.Skill)
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
    }
}
