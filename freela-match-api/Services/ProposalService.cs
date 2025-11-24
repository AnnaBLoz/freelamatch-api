using FreelaMatchAPI.Data;
using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Models;
using FreelaMatchAPI.Interfaces;
using Microsoft.EntityFrameworkCore;

public class ProposalService : IProposalService
{
    private readonly AppDbContext _context;
    private readonly EmailService _emailService;

    public ProposalService(AppDbContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public Task<List<Proposal?>> GetProposals(int companyId) =>
        _context.Proposal.Where(p => p.OwnerId == companyId)
            .Include(p => p.RequiredSkills).ThenInclude(p => p.Skill).ThenInclude(p => p.UserSkills)
            .Include(p => p.Candidates).ThenInclude(p => p.User)
            .ToListAsync();

    public Task<List<Proposal?>> GetAllProposals() =>
        _context.Proposal
            .Where(p => p.IsAvailable)
            .Include(p => p.RequiredSkills).ThenInclude(p => p.Skill)
            .Include(p => p.Candidates).ThenInclude(p => p.User)
            .ToListAsync();

    public async Task<Proposal?> GetProposalById(int proposalId) =>
        await _context.Proposal
            .AsNoTracking()
            .Include(p => p.RequiredSkills).ThenInclude(rs => rs.Skill).ThenInclude(rs => rs.UserSkills)
            .Include(p => p.Candidates).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(p => p.ProposalId == proposalId);

    public async Task<Proposal?> GetProposalByIdAndCandidate(int proposalId, int candidateId) =>
        await _context.Proposal
            .AsNoTracking()
            .Where(p => p.ProposalId == proposalId)
            .Include(p => p.RequiredSkills).ThenInclude(rs => rs.Skill).ThenInclude(s => s.UserSkills)
            .Include(p => p.Candidates.Where(c => c.UserId == candidateId)).ThenInclude(c => c.User)
            .FirstOrDefaultAsync();

    public async Task<Proposal> CreateProposal(CreateProposal proposalCreated)
    {
        var proposal = new Proposal
        {
            Title = proposalCreated.Title,
            Description = proposalCreated.Description,
            Price = proposalCreated.Price,
            MaxDate = proposalCreated.MaxDate,
            OwnerId = proposalCreated.OwnerId,
            IsAvailable = true,
            CreatedDate = DateTime.UtcNow
        };

        _context.Proposal.Add(proposal);
        await _context.SaveChangesAsync();

        foreach (var ps in proposalCreated.RequiredSkills)
        {
            _context.ProposalSkill.Add(new ProposalSkill
            {
                ProposalId = proposal.ProposalId,
                SkillId = ps.SkillId,
                IsActive = true
            });
        }

        await _context.SaveChangesAsync();
        return proposal;
    }

    public async Task<(bool Success, string Message, Candidate? Candidate)> ApproveCandidate(CandidateApprove candidateApprove)
    {
        var candidate = await _context.Candidate.FirstOrDefaultAsync(u => u.CandidateId == candidateApprove.CandidateId);
        if (candidate == null) return (false, "Candidate not found", null);

        candidate.Status = ProposalStatus.Accepted;

        var otherCandidates = await _context.Candidate
            .Where(u => u.CandidateId != candidateApprove.CandidateId && u.ProposalId == candidateApprove.ProposalId)
            .ToListAsync();

        foreach (var rejectedCandidate in otherCandidates)
            rejectedCandidate.Status = ProposalStatus.Rejected;

        var finishProposal = await _context.Proposal.FirstOrDefaultAsync(u => u.ProposalId == candidateApprove.ProposalId);
        if (finishProposal != null) finishProposal.IsAvailable = false;

        await _context.SaveChangesAsync();
        await _emailService.SendApproveEmail(candidate.ProposalId, candidate.UserId);
        return (true, "Candidates updated successfully", candidate);
    }

    public async Task<(bool Success, string Message, Candidate? Candidate)> DisapproveCandidate(CandidateApprove candidateDisapprove)
    {
        var candidate = await _context.Candidate.FirstOrDefaultAsync(u => u.CandidateId == candidateDisapprove.CandidateId);
        if (candidate == null) return (false, "Candidate not found", null);

        candidate.Status = ProposalStatus.Rejected;
        await _context.SaveChangesAsync();
        return (true, "Candidate disapproved successfully", candidate);
    }

    public async Task<Candidate> Candidate(CandidateProposal proposalCreated)
    {
        var candidate = new Candidate
        {
            ProposalId = proposalCreated.ProposalId,
            AppliedAt = DateTime.UtcNow,
            UserId = proposalCreated.UserId,
            Status = ProposalStatus.Pending,
            EstimatedDate = proposalCreated.EstimatedDate,
            ProposedPrice = proposalCreated.ProposedPrice,
            Message = proposalCreated.Message
        };

        _context.Add(candidate);
        await _context.SaveChangesAsync();

        await _emailService.SendNewCandidateEmailAsync(proposalCreated.ProposalId, proposalCreated.UserId);
        return candidate;
    }

    public Task<List<Candidate?>> GetFreelancersToReview(int userId) =>
        _context.Candidate
            .Include(r => r.User)
            .Include(r => r.Proposal)
            .Where(r => r.Proposal.OwnerId == userId && r.Proposal.MaxDate < DateTime.UtcNow && !r.Proposal.IsAvailable && r.Status == ProposalStatus.Accepted && r.Status != ProposalStatus.Reviewed)
            .ToListAsync();

    public Task<List<Proposal>> GetCompaniesToReview(int userId) =>
        _context.Proposal
            .Where(r => r.Candidates.Any(c => c.UserId == userId && c.Status != ProposalStatus.Reviewed) && r.MaxDate < DateTime.UtcNow && !r.IsAvailable)
            .Include(r => r.Owner)
            .ToListAsync();

    public async Task<(bool Success, string Message, Proposal? Proposal)> CounterProposal(CounterProposalCreate dto)
    {
        var proposal = await _context.Proposal.FirstOrDefaultAsync(u => u.ProposalId == dto.ProposalId);
        if (proposal == null) return (false, "Proposal not found", null);

        var counterProposal = new CounterProposal
        {
            ProposalId = dto.ProposalId,
            EstimatedDate = dto.EstimatedDate,
            ProposedPrice = dto.ProposedPrice,
            Message = dto.Message,
            FreelancerId = dto.FreelancerId,
            CompanyId = dto.CompanyId,
            IsSendedByCompany = dto.IsSendedByCompany,
            IsAccepted = dto.IsAccepted
        };

        _context.CounterProposal.Add(counterProposal);
        await _context.SaveChangesAsync();

        await _emailService.SendCounterProposalEmailAsync(proposal.ProposalId, dto.FreelancerId, counterProposal.CounterProposalId);
        return (true, "Counter Proposal sent successfully", proposal);
    }

    public Task<List<CounterProposal>> GetCounterProposalByProposalId(int proposalId) =>
        _context.CounterProposal
            .Where(p => p.ProposalId == proposalId)
            .Include(p => p.Freelancer)
            .Include(p => p.Company)
            .ToListAsync();

    public Task<List<Proposal>> GetProposalsByUserId(int userId) =>
        _context.Proposal
            .AsNoTracking()
            .Include(p => p.Candidates).ThenInclude(c => c.User)
            .Where(p => p.Candidates.Any(c => c.UserId == userId))
            .ToListAsync();
}
