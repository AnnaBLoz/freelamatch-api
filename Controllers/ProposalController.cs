using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class ProposalController : ControllerBase
{
    private readonly ProposalService _proposalService;
    private readonly UserService _userService;

    public ProposalController(ProposalService proposalService, UserService userService)
    {
        _proposalService = proposalService;
        _userService = userService;
    }

    [HttpGet("company/{companyId}")]
    public async Task<ActionResult<List<Proposal>>> GetProposals(int companyId)
    {
        var proposals = await _proposalService.GetProposals(companyId);

        if (proposals == null)
            return NotFound(new { message = "Proposals not found" });

        return Ok(proposals);
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<Proposal>>> GetAllProposals()
    {
        var proposals = await _proposalService.GetAllProposals();

        if (proposals == null || !proposals.Any())
            return NotFound(new { message = "Proposals not found" });

        return Ok(proposals);
    }


    [HttpGet("proposalId/{proposalId}")]
    public async Task<ActionResult<Proposal>> GetProposalById(int proposalId)
    {
        var proposal = await _proposalService.GetProposalById(proposalId);

        if (proposal == null)
            return NotFound(new { message = "Proposal not found" });

        return Ok(proposal);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateProposal proposalCreate)
    {
        try
        {
            var proposal = await _proposalService.CreateProposal(proposalCreate);
            return Ok(new
            {
                proposal
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    [HttpPut("approve")]
    public async Task<IActionResult> ApproveCandidate([FromBody] CandidateApprove candidateApprove)
    {
        try
        {
            var candidateApproved = await _proposalService.ApproveCandidate(candidateApprove);
            return Ok(new
            {
                candidateApproved
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("candidate")]
    public async Task<IActionResult> Candidate([FromBody] CandidateProposal proposalCreate)
    {
        try
        {
            var candidate = await _proposalService.Candidate(proposalCreate);
            return Ok(new
            {
                candidate
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}