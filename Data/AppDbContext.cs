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
    }
}
