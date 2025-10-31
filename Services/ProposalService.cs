using FreelaMatchAPI.Data;
using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.Design;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class ProposalService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public ProposalService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public Task<List<Proposal?>> GetProposals(int companyId)
    {
        return _context.Proposal.Where(p => p.OwnerId == companyId)
            .Include(p => p.RequiredSkills).ThenInclude(p => p.Skill).ThenInclude(p => p.UserSkills)
            .Include(p => p.Candidates).ThenInclude(p => p.User)
            .ToListAsync();
    }

    public Task<List<Proposal?>> GetAllProposals()
    {
        return _context.Proposal
            .Where(p => p.IsAvailable == true)
            .Include(p => p.RequiredSkills).ThenInclude(p => p.Skill)
            .Include(p => p.Candidates).ThenInclude(p => p.User)
            .ToListAsync();
    }

    public async Task<Proposal?> GetProposalById(int proposalId)
    {
        return await _context.Proposal
            .AsNoTracking()
            .Include(p => p.RequiredSkills)
            .ThenInclude(rs => rs.Skill).ThenInclude(rs => rs.UserSkills)
            .Include(p => p.Candidates)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(p => p.ProposalId == proposalId);
    }

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
        var candidate = await _context.Candidate
            .FirstOrDefaultAsync(u => u.CandidateId == candidateApprove.CandidateId);

        if (candidate == null)
            return (false, "Candidate not found", null);

        candidate.Status = ProposalStatus.Accepted;

        var otherCandidates = await _context.Candidate
            .Where(u => u.CandidateId != candidateApprove.CandidateId
                        && u.ProposalId == candidateApprove.ProposalId)
            .ToListAsync();

        foreach (var rejectedCandidate in otherCandidates)
        {
            rejectedCandidate.Status = ProposalStatus.Rejected;
        }

        var finishProposal = await _context.Proposal
           .FirstOrDefaultAsync(u => u.ProposalId == candidateApprove.ProposalId);

        if (finishProposal != null)
            finishProposal.IsAvailable = false;

        try
        {
            await _context.SaveChangesAsync();
            return (true, "Candidates updated successfully", candidate);
        }
        catch (Exception ex)
        {
            return (false, $"Error updating candidates: {ex.Message}", null);
        }
    }

    public async Task<Candidate> Candidate(CandidateProposal proposalCreated)
    {
        var candidate = new Candidate
        {
            ProposalId = proposalCreated.ProposalId,
            AppliedAt = DateTime.UtcNow,
            UserId = proposalCreated.UserId,
            Status = ProposalStatus.Pending
        };

        _context.Add(candidate);

        await _context.SaveChangesAsync();
        return candidate;
    }

    public async Task<List<Candidate>> GetFreelancersToReview(int userId)
    {
        return await _context.Candidate
            .Include(r => r.User)
            .Include(r => r.Proposal)
            .Where(r =>
                r.Proposal.OwnerId == userId &&
                r.Proposal.MaxDate < DateTime.UtcNow &&
                !r.Proposal.IsAvailable &&
                r.Status == ProposalStatus.Accepted &&
                !r.User.ReviewsReceived.Any(rc => rc.ReviewerId == userId)
            )
            .ToListAsync();
    }
}
