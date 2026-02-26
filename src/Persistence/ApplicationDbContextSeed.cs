using Application.Common.Constants;
using Application.Common.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Persistence;

public class ApplicationDbContextSeed
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _hasher;

    public ApplicationDbContextSeed(
        ApplicationDbContext context,
        IPasswordHasher hasher)
    {
        _context = context;
        _hasher = hasher;
    }

    public async Task SeedAsync()
    {
        await _context.Database.MigrateAsync();
        await SeedRolesAsync();
        await SeedUsersAsync();
    }

    // ─── Roles ───────────────────────────────────────────────────────────────

    private async Task SeedRolesAsync()
    {
        var rolesToSeed = new[]
        {
            new { Name = RoleConstants.Admin,          Description = "System administrator with full access" },
            new { Name = RoleConstants.ProjectManager, Description = "Manages project lifecycle and team" },
            new { Name = RoleConstants.Developer,      Description = "Develops project features" },
            new { Name = RoleConstants.Tester,         Description = "Performs quality assurance" },
            new { Name = RoleConstants.ProductManager, Description = "Defines product requirements" },
            new { Name = RoleConstants.DevOps,         Description = "Manages infrastructure and deployments" },
        };

        foreach (var r in rolesToSeed)
        {
            if (!await _context.Roles.AnyAsync(x => x.Name == r.Name))
            {
                _context.Roles.Add(new Role
                {
                    Id = Guid.NewGuid(),
                    Name = r.Name,
                    Description = r.Description
                });
            }
        }

        await _context.SaveChangesAsync(CancellationToken.None);
    }

    // ─── Users ───────────────────────────────────────────────────────────────

    private async Task SeedUsersAsync()
    {
        var usersToSeed = new[]
        {
            new
            {
                FirstName = "Roman",
                LastName  = "Slinkov",
                Email     = "thankr3@gmail.com",
                Password  = "Hjvfy3105",
                RoleName  = RoleConstants.Admin
            },
            new
            {
                FirstName = "Alexey",
                LastName  = "Petrov",
                Email     = "alexey.petrov@psl.dev",
                Password  = "Manager123",
                RoleName  = RoleConstants.ProjectManager
            },
            new
            {
                FirstName = "Dmitry",
                LastName  = "Kozlov",
                Email     = "dmitry.kozlov@psl.dev",
                Password  = "Developer123",
                RoleName  = RoleConstants.Developer
            },
            new
            {
                FirstName = "Elena",
                LastName  = "Morozova",
                Email     = "elena.morozova@psl.dev",
                Password  = "Tester123",
                RoleName  = RoleConstants.Tester
            },
            new
            {
                FirstName = "Ivan",
                LastName  = "Sokolov",
                Email     = "ivan.sokolov@psl.dev",
                Password  = "Product123",
                RoleName  = RoleConstants.ProductManager
            },
            new
            {
                FirstName = "Sergey",
                LastName  = "Volkov",
                Email     = "sergey.volkov@psl.dev",
                Password  = "DevOps123",
                RoleName  = RoleConstants.DevOps
            },
        };

        foreach (var u in usersToSeed)
        {
            // Идемпотентность — пропускаем если email уже есть
            if (await _context.Credentials.AnyAsync(c => c.Email == u.Email))
                continue;

            var role = await _context.Roles
                .FirstAsync(r => r.Name == u.RoleName);

            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = u.FirstName,
                LastName = u.LastName,
            };

            var credentials = new Credentials
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Email = u.Email,
                PasswordHash = _hasher.Hash(u.Password),
            };

            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id,
            };

            _context.Users.Add(user);
            _context.Credentials.Add(credentials);
            _context.UserRoles.Add(userRole);
        }

        await _context.SaveChangesAsync(CancellationToken.None);
    }
}