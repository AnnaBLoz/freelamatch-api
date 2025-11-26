using FreelaMatchAPI.Models;
using Xunit;

namespace freela_match_api_test.Models
{
    // =====================================================================
    // COMPANY - 0% → 100% (14 linhas)
    // =====================================================================
    public class CompanyTests
    {
        [Fact]
        public void Company_ShouldSetAllProperties()
        {
            var company = new Company
            {
                Id = 1,
                CompanyName = "Test Company",
                Description = "Description",
                UserId = 10
            };

            Assert.Equal(1, company.Id);
            Assert.Equal("Test Company", company.CompanyName);
            Assert.Equal("Description", company.Description);
            Assert.Equal(10, company.UserId);
        }

        [Fact]
        public void Company_ShouldInitializeWithDefaultValues()
        {
            var company = new Company();
            Assert.NotNull(company);
        }
    }

    // =====================================================================
    // FREELANCER - 0% → 100% (12 linhas)
    // =====================================================================
    public class FreelancerTests
    {
        [Fact]
        public void Freelancer_ShouldSetAllProperties()
        {
            var freelancer = new Freelancer
            {
                Id = 1,
                UserId = 10,
                Bio = "Experienced developer",
            };

            Assert.Equal(1, freelancer.Id);
            Assert.Equal(10, freelancer.UserId);
            Assert.Equal("Experienced developer", freelancer.Bio);
        }

        [Fact]
        public void Freelancer_ShouldInitializeWithDefaultValues()
        {
            var freelancer = new Freelancer();
            Assert.NotNull(freelancer);
        }

        [Fact]
        public void Freelancer_ShouldSetNavigationProperties()
        {
            var user = new User { Id = 1, Name = "User", Email = "test@test.com", Password = "pass", Token = "token" };

            var freelancer = new Freelancer
            {
                Id = 1,
                UserId = 1,
                User = user
            };

            Assert.NotNull(freelancer.User);
        }
    }

    // =====================================================================
    // PROFILERESUME - 0% → 100% (8 linhas)
    // =====================================================================
    public class ProfileResumeTests
    {
        [Fact]
        public void ProfileResume_ShouldSetAllProperties()
        {
            var profileResume = new ProfileResume
            {
                ProfileId = 1,
                Biography = "Test Bio",
                PricePerHour = 100,
                ExperienceLevel = ExperienceLevel.Senior,
            };

            Assert.Equal(1, profileResume.ProfileId);
            Assert.Equal("Test Bio", profileResume.Biography);
            Assert.Equal(100, profileResume.PricePerHour);
            Assert.Equal(ExperienceLevel.Senior, profileResume.ExperienceLevel);
        }

        [Fact]
        public void ProfileResume_ShouldInitializeWithDefaultValues()
        {
            var profileResume = new ProfileResume();
            Assert.NotNull(profileResume);
        }

        [Theory]
        [InlineData(ExperienceLevel.Junior)]
        [InlineData(ExperienceLevel.Pleno)]
        [InlineData(ExperienceLevel.Senior)]
        public void ProfileResume_ShouldSetDifferentExperienceLevels(ExperienceLevel level)
        {
            var profileResume = new ProfileResume
            {
                ExperienceLevel = level
            };

            Assert.Equal(level, profileResume.ExperienceLevel);
        }
    }

    // =====================================================================
    // USERRESUME - 0% → 100% (12 linhas)
    // =====================================================================
    public class UserResumeTests
    {
        [Fact]
        public void UserResume_ShouldInitializeWithDefaultValues()
        {
            var userResume = new UserResume();
            Assert.NotNull(userResume);
        }

        [Fact]
        public void UserResume_ShouldSetNavigationProperties()
        {
            var profile = new ProfileResume { ProfileId = 1 };
            var skills = new List<UserSkillResume>();

            var userResume = new UserResume
            {
                Id = 1,
                Profile = profile,
                UserSkills = skills
            };

            Assert.NotNull(userResume.Profile);
            Assert.NotNull(userResume.UserSkills);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void UserResume_ShouldSetIsAvailable(bool isAvailable)
        {
            var userResume = new UserResume
            {
                IsAvailable = isAvailable
            };

            Assert.Equal(isAvailable, userResume.IsAvailable);
        }
    }

    // =====================================================================
    // USERSKILLRESUME - 0% → 100% (10 linhas)
    // =====================================================================
    public class UserSkillResumeTests
    {
        [Fact]
        public void UserSkillResume_ShouldSetAllProperties()
        {
            var skillResume = new UserSkillResume
            {
                SkillId = 1,
                Name = "C#",
                IsActive = true
            };

            Assert.Equal(1, skillResume.SkillId);
            Assert.Equal("C#", skillResume.Name);
            Assert.True(skillResume.IsActive);
        }

        [Fact]
        public void UserSkillResume_ShouldInitializeWithDefaultValues()
        {
            var skillResume = new UserSkillResume();
            Assert.NotNull(skillResume);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void UserSkillResume_ShouldSetIsActive(bool isActive)
        {
            var skillResume = new UserSkillResume
            {
                IsActive = isActive
            };

            Assert.Equal(isActive, skillResume.IsActive);
        }

        [Theory]
        [InlineData("C#")]
        [InlineData("JavaScript")]
        [InlineData("Python")]
        public void UserSkillResume_ShouldSetDifferentSkillNames(string skillName)
        {
            var skillResume = new UserSkillResume
            {
                Name = skillName
            };

            Assert.Equal(skillName, skillResume.Name);
        }
    }

    // =====================================================================
    // USERSKILL - 31.2% → 100% (16 linhas restantes)
    // =====================================================================
    public class UserSkillTests
    {
        [Fact]
        public void UserSkill_ShouldSetAllProperties()
        {
            var userSkill = new UserSkill
            {
                UserId = 1,
                SkillId = 2,
                ProfileId = 3,
                IsActive = true
            };

            Assert.Equal(1, userSkill.UserId);
            Assert.Equal(2, userSkill.SkillId);
            Assert.Equal(3, userSkill.ProfileId);
            Assert.True(userSkill.IsActive);
        }

        [Fact]
        public void UserSkill_ShouldInitializeWithDefaultValues()
        {
            var userSkill = new UserSkill();
            Assert.NotNull(userSkill);
        }

        [Fact]
        public void UserSkill_ShouldSetNavigationProperties()
        {
            var user = new User { Id = 1, Name = "Test", Email = "test@test.com", Password = "pass", Token = "token" };
            var skill = new Skill { SkillId = 1, Name = "C#" };
            var profile = new Profile { ProfileId = 1, UserId = 1 };

            var userSkill = new UserSkill
            {
                UserId = 1,
                SkillId = 1,
                ProfileId = 1,
                User = user,
                Skill = skill,
                Profile = profile
            };

            Assert.NotNull(userSkill.User);
            Assert.NotNull(userSkill.Skill);
            Assert.NotNull(userSkill.Profile);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void UserSkill_ShouldSetIsActive(bool isActive)
        {
            var userSkill = new UserSkill
            {
                IsActive = isActive
            };

            Assert.Equal(isActive, userSkill.IsActive);
        }
    }

    // =====================================================================
    // PROPOSALSKILL - 50% → 100% (6 linhas restantes)
    // =====================================================================
    public class ProposalSkillTests
    {
        [Fact]
        public void ProposalSkill_ShouldSetAllProperties()
        {
            var proposalSkill = new ProposalSkill
            {
                ProposalId = 1,
                SkillId = 2
            };

            Assert.Equal(1, proposalSkill.ProposalId);
            Assert.Equal(2, proposalSkill.SkillId);
        }

        [Fact]
        public void ProposalSkill_ShouldInitializeWithDefaultValues()
        {
            var proposalSkill = new ProposalSkill();
            Assert.NotNull(proposalSkill);
        }

        [Fact]
        public void ProposalSkill_ShouldSetNavigationProperties()
        {
            var proposal = new Proposal { ProposalId = 1 };
            var skill = new Skill { SkillId = 1, Name = "C#" };

            var proposalSkill = new ProposalSkill
            {
                ProposalId = 1,
                SkillId = 1,
                Proposal = proposal,
                Skill = skill
            };

            Assert.NotNull(proposalSkill.Proposal);
            Assert.NotNull(proposalSkill.Skill);
        }
    }

    // =====================================================================
    // PROPOSALSKILLCREATE - 50% → 100% (2 linhas restantes)
    // =====================================================================
    public class ProposalSkillCreateTests
    {
        [Fact]
        public void ProposalSkillCreate_ShouldSetAllProperties()
        {
            var proposalSkillCreate = new ProposalSkillCreate
            {
                SkillId = 5
            };

            Assert.Equal(5, proposalSkillCreate.SkillId);
        }

        [Fact]
        public void ProposalSkillCreate_ShouldInitializeWithDefaultValues()
        {
            var proposalSkillCreate = new ProposalSkillCreate();
            Assert.NotNull(proposalSkillCreate);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(999)]
        public void ProposalSkillCreate_ShouldSetDifferentSkillIds(int skillId)
        {
            var proposalSkillCreate = new ProposalSkillCreate
            {
                SkillId = skillId
            };

            Assert.Equal(skillId, proposalSkillCreate.SkillId);
        }
    }

    // =====================================================================
    // CANDIDATEAPPROVE - 66.6% → 100% (2 linhas restantes)
    // =====================================================================
    public class CandidateApproveTests
    {
        [Fact]
        public void CandidateApprove_ShouldSetAllProperties()
        {
            var candidateApprove = new CandidateApprove
            {
                CandidateId = 1,
                ProposalId = 2
            };

            Assert.Equal(1, candidateApprove.CandidateId);
            Assert.Equal(2, candidateApprove.ProposalId);
        }

        [Fact]
        public void CandidateApprove_ShouldInitializeWithDefaultValues()
        {
            var candidateApprove = new CandidateApprove();
            Assert.NotNull(candidateApprove);
        }
    }

    // =====================================================================
    // PORTFOLIO - 80% → 100% (2 linhas restantes)
    // =====================================================================
    public class PortfolioTests
    {
        [Fact]
        public void Portfolio_ShouldSetAllProperties()
        {
            var portfolio = new Portfolio
            {
                PortfolioId = 1,
                URL = "https://project.com",
                UserId = 10
            };

            Assert.Equal(1, portfolio.PortfolioId);
            Assert.Equal("https://project.com", portfolio.URL);
            Assert.Equal(10, portfolio.UserId);
        }

        [Fact]
        public void Portfolio_ShouldInitializeWithDefaultValues()
        {
            var portfolio = new Portfolio();
            Assert.NotNull(portfolio);
        }

        [Fact]
        public void Portfolio_ShouldSetNavigationProperties()
        {
            var user = new User { Id = 1, Name = "User", Email = "test@test.com", Password = "pass", Token = "token" };

            var portfolio = new Portfolio
            {
                PortfolioId = 1,
                UserId = 1,
                User = user
            };

            Assert.NotNull(portfolio.User);
        }
    }

    // =====================================================================
    // PROFILE - 65% → 100% (20 linhas restantes)
    // =====================================================================
    public class ProfileTests
    {
        [Fact]
        public void Profile_ShouldSetAllProperties()
        {
            var profile = new Profile
            {
                ProfileId = 1,
                UserId = 10,
                Biography = "Bio",
                PricePerHour = 100,
                ExperienceLevel = ExperienceLevel.Senior,
                SectorId = 5
            };

            Assert.Equal(1, profile.ProfileId);
            Assert.Equal(10, profile.UserId);
            Assert.Equal("Bio", profile.Biography);
            Assert.Equal(100, profile.PricePerHour);
            Assert.Equal(ExperienceLevel.Senior, profile.ExperienceLevel);
            Assert.Equal(5, profile.SectorId);
        }

        [Fact]
        public void Profile_ShouldInitializeWithDefaultValues()
        {
            var profile = new Profile();
            Assert.NotNull(profile);
        }

        [Fact]
        public void Profile_ShouldSetNavigationProperties()
        {
            var user = new User { Id = 1, Name = "User", Email = "test@test.com", Password = "pass", Token = "token" };
            var sector = new Sector { SectorId = 1, Name = "Tech" };
            var userSkills = new List<UserSkill>();

            var profile = new Profile
            {
                ProfileId = 1,
                UserId = 1,
                User = user,
                Sector = sector,
                UserSkills = userSkills
            };

            Assert.NotNull(profile.User);
            Assert.NotNull(profile.Sector);
            Assert.NotNull(profile.UserSkills);
        }
    }

    // =====================================================================
    // REVIEWS - 65% → 100% (8 linhas restantes)
    // =====================================================================
    public class ReviewsTests
    {
        [Fact]
        public void Reviews_ShouldSetAllProperties()
        {
            var review = new Reviews
            {
                Id = 1,
                ReviewerId = 10,
                ReceiverId = 20,
                ReviewText = "Great work!",
                Rating = 5,
                ProposalId = 30,
                CreatedAt = DateTime.Now
            };

            Assert.Equal(1, review.Id);
            Assert.Equal(10, review.ReviewerId);
            Assert.Equal(20, review.ReceiverId);
            Assert.Equal("Great work!", review.ReviewText);
            Assert.Equal(5, review.Rating);
            Assert.Equal(30, review.ProposalId);
        }

        [Fact]
        public void Reviews_ShouldInitializeWithDefaultValues()
        {
            var review = new Reviews();
            Assert.NotNull(review);
        }

        [Fact]
        public void Reviews_ShouldSetNavigationProperties()
        {
            var reviewer = new User { Id = 1, Name = "Reviewer", Email = "r@test.com", Password = "pass", Token = "token" };
            var receiver = new User { Id = 2, Name = "Receiver", Email = "rec@test.com", Password = "pass", Token = "token" };

            var review = new Reviews
            {
                Id = 1,
                ReviewerId = 1,
                ReceiverId = 2,
                Reviewer = reviewer,
                Receiver = receiver
            };

            Assert.NotNull(review.Reviewer);
            Assert.NotNull(review.Receiver);
        }
    }
}