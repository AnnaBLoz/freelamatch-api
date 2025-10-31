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

public class ReviewsService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public ReviewsService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<List<Reviews>> GetReviews(int userId)
    {
        return await _context.Reviews
            .Include(r => r.Reviewer)
            .Include(r => r.Receiver)
            .Where(r => r.ReceiverId == userId) // pega as avaliações recebidas por esse usuário
            .ToListAsync();
    }
}
