using FreelaMatchAPI.Controllers;
using FreelaMatchAPI.DTOs;
using FreelaMatchAPI.Interfaces;
using FreelaMatchAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FreelaMatchAPI.Tests
{
    public class GeneralControllerTests
    {
        private readonly Mock<IGeneralService> _generalServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly GeneralController _controller;

        public GeneralControllerTests()
        {
            _generalServiceMock = new Mock<IGeneralService>();
            _userServiceMock = new Mock<IUserService>();

            _controller = new GeneralController(
                _generalServiceMock.Object,
                _userServiceMock.Object
            );
        }

        // -----------------------------------------------------
        // GetFreelancers - SUCESSO
        // -----------------------------------------------------
        [Fact]
        public async Task GetFreelancers_ShouldReturnOk_WhenFreelancersExist()
        {
            // Arrange
            var freelancers = new List<User>
            {
                new User { Id = 1, Name = "Anna", Email = "anna@test.com", Type = UserType.Freelancer },
                new User { Id = 2, Name = "Bob", Email = "bob@test.com", Type = UserType.Freelancer }
            };
            _generalServiceMock.Setup(s => s.GetFreelancers()).ReturnsAsync(freelancers);

            // Act
            var actionResult = await _controller.GetFreelancers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var data = Assert.IsType<List<User>>(okResult.Value);
            Assert.Equal(2, data.Count);
        }

        [Fact]
        public async Task GetFreelancers_ShouldReturnNotFound_WhenNoFreelancers()
        {
            // Arrange
            _generalServiceMock.Setup(s => s.GetFreelancers()).ReturnsAsync((List<User>?)null);

            // Act
            var actionResult = await _controller.GetFreelancers();

            // Assert
            Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        }

        // -----------------------------------------------------
        // GetSectors - SUCESSO
        // -----------------------------------------------------
        [Fact]
        public async Task GetSectors_ShouldReturnOk_WhenSectorsExist()
        {
            // Arrange
            var sectors = new List<Sector>
            {
                new Sector { SectorId = 1, Name = "TI" },
                new Sector { SectorId = 2, Name = "Marketing" }
            };
            _generalServiceMock.Setup(s => s.GetSectors()).ReturnsAsync(sectors);

            // Act
            var actionResult = await _controller.GetSectors();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var data = Assert.IsType<List<Sector>>(okResult.Value);
            Assert.Equal(2, data.Count);
        }

        // -----------------------------------------------------
        // GetSkills - SUCESSO
        // -----------------------------------------------------
        [Fact]
        public async Task GetSkills_ShouldReturnOk_WhenSkillsExist()
        {
            // Arrange
            var skills = new List<Skill>
            {
                new Skill { SkillId = 1, Name = "C#" },
                new Skill { SkillId = 2, Name = "JavaScript" }
            };
            _generalServiceMock.Setup(s => s.GetSkills()).ReturnsAsync(skills);

            // Act
            var actionResult = await _controller.GetSkills();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var data = Assert.IsType<List<Skill>>(okResult.Value);
            Assert.Equal(2, data.Count);
        }

        // -----------------------------------------------------
        // CompletedProjects - SUCESSO
        // -----------------------------------------------------
        [Fact]
        public async Task CompletedProjects_ShouldReturnOk_WhenProjectsExist()
        {
            // Arrange
            int userId = 1;
            var projects = new List<Candidate>
            {
                new Candidate { CandidateId = 1, UserId = userId, Status = ProposalStatus.Accepted },
                new Candidate { CandidateId = 2, UserId = userId, Status = ProposalStatus.Accepted }
            };
            _generalServiceMock.Setup(s => s.CompletedProjects(userId)).ReturnsAsync(projects);

            // Act
            var actionResult = await _controller.CompletedProjects(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var data = Assert.IsType<List<Candidate>>(okResult.Value);
            Assert.Equal(2, data.Count);
        }

        [Fact]
        public async Task CompletedProjects_ShouldReturnNotFound_WhenNoProjects()
        {
            // Arrange
            int userId = 1;
            _generalServiceMock.Setup(s => s.CompletedProjects(userId)).ReturnsAsync((List<Candidate>?)null);

            // Act
            var actionResult = await _controller.CompletedProjects(userId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        }
    }
}
