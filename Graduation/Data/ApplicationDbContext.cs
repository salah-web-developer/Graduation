using Graduation.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Graduation.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public DbSet<User> Users { get; private set; }
        public DbSet<Project> Projects { get; private set; }
        public DbSet<ProjectIdeaRequest> ProjectIdeaRequests { get; private set; }
    }
}
