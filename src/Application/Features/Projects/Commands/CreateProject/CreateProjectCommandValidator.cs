using FluentValidation;

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