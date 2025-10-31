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
}
