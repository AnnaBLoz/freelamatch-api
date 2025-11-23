using FreelaMatchAPI.Models;

namespace FreelaMatchAPI.Interfaces
{
    public interface IProfileService
    {
        Task<Profile?> GetProfileByUserIdAsync(int userId);
        Task<List<Skill>> GetSkills();
        Task<(bool Success, string Message, Profile? Profile)> CreateProfileAsync(int userId, UpdateProfile updatedProfile);
        Task<(bool Success, string Message, Profile? Profile)> UpdateProfileAsync(int userId, UpdateProfile updatedProfile);
    }
}