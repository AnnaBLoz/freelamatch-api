using FreelaMatchAPI.Data;
using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class EmailService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public EmailService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task SendAsync(string toEmail, string subject, string message)
    {
        var email = new MimeMessage();

        email.From.Add(new MailboxAddress("Freela Match", _config["EmailSettings:From"]));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;
        email.Body = new TextPart("plain") { Text = message };

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(
            _config["EmailSettings:Host"],
            int.Parse(_config["EmailSettings:Port"]),
            SecureSocketOptions.SslOnConnect // Porta 465
        );

        await smtp.AuthenticateAsync(
            _config["EmailSettings:Username"],
            _config["EmailSettings:Password"]
        );

        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }

    public async Task SendNewCandidateEmailAsync(int proposalId, int candidateUserId)
    {
        var proposal = await _context.Proposal
            .FirstOrDefaultAsync(p => p.ProposalId == proposalId);

        var candidate = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == candidateUserId);

        var company = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == proposal.OwnerId);

        if (company == null || candidate == null || proposal == null)
            return;

        string subject = "Novo candidato em sua vaga";

        string message = $@"
Olá, {company.Name}!

Você recebeu um novo candidato para a vaga: {proposal.Title}

➡ Nome do candidato: {candidate.Name}
➡ E-mail: {candidate.Email}
➡ Data da candidatura: {DateTime.Now:dd/MM/yyyy HH:mm}

Acesse o FreelaMatch para visualizar os detalhes.

Equipe FreelaMatch.
";

        await SendAsync(company.Email, subject, message);
    }

    public async Task SendCounterProposalEmailAsync(int proposalId, int candidateUserId, int counteredProposalId)
    {
        var proposal = await _context.Proposal
            .FirstOrDefaultAsync(p => p.ProposalId == proposalId);

        var candidate = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == candidateUserId);

        var company = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == proposal.OwnerId);

        var counteredProposal = await _context.CounterProposal
            .FirstOrDefaultAsync(p => p.CounterProposalId == counteredProposalId);

        if (company == null || candidate == null || proposal == null || counteredProposal == null)
            return;

        string name;
        if (counteredProposal.IsSendedByCompany == true)
            name = proposal.Owner.Name;
        else name = candidate.Name;

            string subject = "Nova contra proposta!";
        string message = $@"
Olá, {name}!

Você recebeu uma nova contraproposta para a vaga: {proposal.Title}

➡ Entrega estimada: {(counteredProposal.EstimatedDate.ToString("dd/MM/yyyy") ?? "Não informado")}
➡ Valor: R$ {counteredProposal.ProposedPrice}
➡ Mensagem: {counteredProposal.Message ?? "Sem mensagem"}

Acesse o FreelaMatch para visualizar os detalhes.

Equipe FreelaMatch.
";

        if (counteredProposal.IsSendedByCompany == true)
        await SendAsync(candidate.Email, subject, message);
        else await SendAsync(proposal.Owner.Email, subject, message);
    }
}
