using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Models;

namespace FreelaMatchAPI.Interfaces
{
    public interface IProposalService
    {
        Task<List<Proposal?>> GetProposals(int companyId);
        Task<List<Proposal?>> GetAllProposals();
        Task<Proposal?> GetProposalById(int proposalId);
        Task<Proposal?> GetProposalByIdAndCandidate(int proposalId, int candidateId);
        Task<Proposal> CreateProposal(CreateProposal proposalCreated);
        Task<(bool Success, string Message, Candidate? Candidate)> ApproveCandidate(CandidateApprove candidateApprove);
        Task<(bool Success, string Message, Candidate? Candidate)> DisapproveCandidate(CandidateApprove candidateDisapprove);
        Task<Candidate> Candidate(CandidateProposal proposalCreated);
        Task<List<Candidate?>> GetFreelancersToReview(int userId);
        Task<List<Proposal>> GetCompaniesToReview(int userId);
        Task<(bool Success, string Message, Proposal? Proposal)> CounterProposal(CounterProposalCreate dto);
        Task<List<CounterProposal>> GetCounterProposalByProposalId(int proposalId);
        Task<List<Proposal>> GetProposalsByUserId(int userId);
    }
}
