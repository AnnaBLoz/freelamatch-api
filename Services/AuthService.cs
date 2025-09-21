using FreelaMatchAPI.Data;
using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext context, IPasswordHasher<User> passwordHasher, IConfiguration config)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _config = config;
    }

    public async Task<User> RegisterAsync(RegisterDto dto)
    {
        // Verifica se email já existe
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (existingUser != null)
            throw new InvalidOperationException("Email já está em uso.");

        var user = new User
        {
            Email = dto.Email,
            Type = dto.Type
        };

        // Hash da senha
        user.Password = _passwordHasher.HashPassword(user, dto.Password);

        // Gera token
        user.Token = GenerateJwtToken(user);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user; 
    }

    public async Task<User?> LoginAsync(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return null;

        var result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
        if (result != PasswordVerificationResult.Success) return null;

        // Atualiza o token
        user.Token = GenerateJwtToken(user);
        await _context.SaveChangesAsync();

        return user; // agora retorna o User
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _config.GetSection("Jwt");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiresInMinutes"]!)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
