using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Models;
using FreelaMatchAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelaMatchAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProposalController : ControllerBase
    {
        private readonly IProposalService _proposalService;
        private readonly IUserService _userService;

        public ProposalController(IProposalService proposalService, IUserService userService)
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

        [HttpGet("proposalId/{proposalId}/candidate/{candidateId}")]
        public async Task<ActionResult<Proposal>> GetProposalByIdAndCandidate(int proposalId, int candidateId)
        {
            var proposal = await _proposalService.GetProposalByIdAndCandidate(proposalId, candidateId);
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
                return Ok(new { proposal });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("approve")]
        public async Task<IActionResult> ApproveCandidate([FromBody] CandidateApprove candidateApprove)
        {
            var result = await _proposalService.ApproveCandidate(candidateApprove);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { candidateApproved = result.Candidate });
        }

        [HttpPut("disapprove")]
        public async Task<IActionResult> DisapproveCandidate([FromBody] CandidateApprove candidateDisapprove)
        {
            var result = await _proposalService.DisapproveCandidate(candidateDisapprove);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { candidateDisapproved = result.Candidate });
        }

        [HttpPost("candidate")]
        public async Task<IActionResult> Candidate([FromBody] CandidateProposal proposalCreate)
        {
            var candidate = await _proposalService.Candidate(proposalCreate);
            return Ok(new { candidate });
        }

        [HttpPost("counterproposal")]
        public async Task<IActionResult> CounterProposal([FromBody] CounterProposalCreate counterProposal)
        {
            var result = await _proposalService.CounterProposal(counterProposal);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message, proposal = result.Proposal });
        }

        [HttpGet("counterproposal/proposalId/{proposalId}")]
        public async Task<ActionResult<List<CounterProposal>>> GetCounterProposalByProposalId(int proposalId)
        {
            var counterProposals = await _proposalService.GetCounterProposalByProposalId(proposalId);
            if (counterProposals == null)
                return NotFound(new { message = "No counter proposals found" });

            return Ok(counterProposals);
        }

        [HttpGet("candidate/userId/{userId}")]
        public async Task<ActionResult<List<Proposal>>> GetProposalsByUserId(int userId)
        {
            var candidateProposals = await _proposalService.GetProposalsByUserId(userId);
            if (candidateProposals == null)
                return NotFound(new { message = "No candidate proposals found" });

            return Ok(candidateProposals);
        }
    }
}
