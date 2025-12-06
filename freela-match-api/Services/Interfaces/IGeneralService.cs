using FreelaMatchAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FreelaMatchAPI.Interfaces
{
    public interface IGeneralService
    {
        Task<List<User?>> GetFreelancers();
        Task<List<Sector?>> GetSectors();
        Task<List<Skill?>> GetSkills();
        Task<List<Candidate?>> CompletedProjects(int userId);
        Task<List<UserResume>> Match(int companyUserId);
    }
}
