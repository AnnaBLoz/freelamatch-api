using System.Threading.Tasks;

namespace FreelaMatchAPI.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(string toEmail, string subject, string message);
        Task SendNewCandidateEmailAsync(int proposalId, int candidateUserId);
        Task SendCounterProposalEmailAsync(int proposalId, int candidateUserId, int counteredProposalId);
        Task SendApproveEmail(int proposalId, int candidateId);
    }
}
