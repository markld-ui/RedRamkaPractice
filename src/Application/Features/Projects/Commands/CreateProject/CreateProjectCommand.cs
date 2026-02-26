using Domain.Projects;
using MediatR;
using FluentValidation;
using Application.Common.Constants;
using Application.Common.Interfaces;
using Domain.Events;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Projects.Commands.CreateProject;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(n => n.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .MaximumLength(200).WithMessage("Project name cannot exceed 200 characters.")
            .Matches(@"^[\p{L}0-9\s\-_]+$").WithMessage("Project name can only contain letters, " +
                                                         "numbers, spaces, hyphens, and underscores.");

        RuleFor(d => d.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(m => m.MemberIds)
            .Must(ids => ids == null || ids.All(id => id != Guid.Empty))
            .WithMessage("Invalid member IDs provided.");
    }
}

public class CreateProjectCommandHandler :
    IRequestHandler<CreateProjectCommand, CreateProjectResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateProjectCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<CreateProjectResult> Handle(CreateProjectCommand request, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedAccessException("User must be authenticated.");

        var creatorId = _currentUser.UserId!.Value;
        var memberIds = (request.MemberIds ?? new List<Guid>())
            .Distinct()
            .ToList();

        if (!memberIds.Contains(creatorId))
            memberIds.Insert(0, creatorId);

        var users = await _context.Users
            .Where(u => memberIds.Contains(u.Id))
            .ToListAsync(ct);

        if (users.Count != memberIds.Count)
            throw new InvalidOperationException("Some users not found.");

        var roles = await _context.Roles
            .Where(r => r.Name == RoleConstants.ProjectManager
                     || r.Name == RoleConstants.Developer)
            .ToListAsync(ct);

        var projectManagerRole = roles.First(r => r.Name == RoleConstants.ProjectManager);
        var developerRole = roles.First(r => r.Name == RoleConstants.Developer);

        if (await _context.Projects.AnyAsync(p => p.Name == request.Name, ct))
            throw new InvalidOperationException("Project already exists.");

        var project = new Project(request.Name, request.Description);

        foreach (var uid in memberIds)
        {
            var role = uid == creatorId
                ? projectManagerRole
                : developerRole;

            project.AddMember(uid, role.Id);
        }

        _context.Projects.Add(project);

        await _context.SaveChangesAsync(ct);

        return new CreateProjectResult(
            project.Id,
            project.Name,
            project.Stage,
            project.CreatedAt);
    }

}

public record CreateProjectCommand(
    string Name,
    string Description,
    List<Guid> MemberIds) : IRequest<CreateProjectResult>;

public record CreateProjectResult(
    Guid Id,
    string Name,
    ProjectStage Stage,
    DateTime CreatedAt);