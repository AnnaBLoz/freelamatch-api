using Microsoft.EntityFrameworkCore;
using FreelaMatchAPI.Models;

namespace FreelaMatchAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Freelancer> Freelancers { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<UserSkill> UserSkills { get; set; }
        public DbSet<Portfolio> Portfolio { get; set; }
        public DbSet<Sector> Sector { get; set; }
        public DbSet<Proposal> Proposal { get; set; }
        public DbSet<ProposalSkill> ProposalSkill { get; set; }
        public DbSet<Candidate> Candidate { get; set; }
        public DbSet<Reviews> Reviews { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Profile>()
                .HasOne(p => p.User)          
                .WithOne(u => u.Profile)      
                .HasForeignKey<Profile>(p => p.UserId);

            modelBuilder.Entity<Reviews>()
                .HasOne(r => r.Reviewer)
                .WithMany(u => u.ReviewsGiven)
                .HasForeignKey(r => r.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reviews>()
                .HasOne(r => r.Receiver)
                .WithMany(u => u.ReviewsReceived)
                .HasForeignKey(r => r.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
