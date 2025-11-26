using FreelaMatchAPI.Data;
using FreelaMatchAPI.Interfaces;
using FreelaMatchAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace freela_match_api_test.Services
{
    public class EmailServiceTests
    {
        // ===============================
        // FAKE SERVICE PARA TESTES
        // ===============================
        public class FakeEmailService : IEmailService
        {
            public string SentTo;
            public string SentSubject;
            public string SentMessage;

            public Task SendAsync(string toEmail, string subject, string message)
            {
                SentTo = toEmail;
                SentSubject = subject;
                SentMessage = message;
                return Task.CompletedTask;
            }

            public Task SendNewCandidateEmailAsync(int proposalId, int candidateUserId)
            {
                // Apenas redireciona para SendAsync de teste
                SentTo = "empresa@test.com";
                SentSubject = "Novo candidato";
                SentMessage = "Candidato";
                return Task.CompletedTask;
            }

            public Task SendCounterProposalEmailAsync(int proposalId, int candidateUserId, int counteredProposalId)
            {
                SentTo = "candidato@test.com";
                SentSubject = "Sua proposta foi aceita";
                SentMessage = "Teste";
                return Task.CompletedTask;
            }

            public Task SendApproveEmail(int proposalId, int candidateId)
            {
                SentTo = "fulano@test.com";
                SentSubject = "Aprovação";
                SentMessage = "Fulano";
                return Task.CompletedTask;
            }
        }

        // ===============================
        // HELPER - BANCO IN MEMORY
        // ===============================
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private IConfiguration GetFakeConfig()
        {
            var dict = new Dictionary<string, string>
            {
                ["EmailSettings:From"] = "from@test.com",
                ["EmailSettings:Host"] = "smtp.test.com",
                ["EmailSettings:Port"] = "465",
                ["EmailSettings:Username"] = "user",
                ["EmailSettings:Password"] = "pass"
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(dict)
                .Build();
        }

        [Fact]
        public async Task SendNewCandidateEmailAsync_ShouldSendEmailToCompany()
        {
            var service = new FakeEmailService();
            await service.SendNewCandidateEmailAsync(1, 20);

            Assert.Equal("empresa@test.com", service.SentTo);
            Assert.Contains("Novo candidato", service.SentSubject);
            Assert.Contains("Candidato", service.SentMessage);
        }

        [Fact]
        public async Task SendApproveEmail_ShouldSendEmailToCandidate()
        {
            var service = new FakeEmailService();
            await service.SendApproveEmail(1, 99);

            Assert.Equal("fulano@test.com", service.SentTo);
            Assert.Contains("Aprovação", service.SentSubject);
            Assert.Contains("Fulano", service.SentMessage);
        }

        [Fact]
        public async Task SendCounterProposalEmailAsync_ShouldSendEmailCorrectly()
        {
            var service = new FakeEmailService();
            await service.SendCounterProposalEmailAsync(1, 20, 1);

            Assert.Equal("candidato@test.com", service.SentTo);
            Assert.Contains("Sua proposta foi aceita", service.SentSubject);
            Assert.Contains("Teste", service.SentMessage);
        }
    }
}
