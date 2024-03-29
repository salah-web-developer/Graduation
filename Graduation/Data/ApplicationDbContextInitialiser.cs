using Graduation.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Graduation.Data
{
    public class ApplicationDbContextInitialiser
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public ApplicationDbContextInitialiser(ApplicationDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task InitiliseAsync()
        {
            await _context.Database.MigrateAsync();
            await SeedAsync();
        }

        private async Task SeedAsync()
        {
            if(!await _context.Users.AnyAsync())
            {
                var user = new User
                {
                    Id = 1,
                    Name = "Default admin account",
                    HashedPassword = _passwordHasher.HashPassword(null!, "admin")
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
        }
    }
}
