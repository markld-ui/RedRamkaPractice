using Application.Features.Projects.Commands;
using Application.Features.Projects.Specifications.Commands;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Validators;

/// <summary>
/// Тесты валидатора <see cref="ArchiveCommandValidator"/>.
/// </summary>
public class ArchiveCommandValidatorTests
{
    private readonly ArchiveCommandValidator _sut = new();

    // ─── Reason ───────────────────────────────────────────────────────────────

    [Fact]
    public void Reason_WhenEmpty_ShouldHaveValidationError()
    {
        var result = _sut.TestValidate(new ArchiveCommand(Guid.NewGuid(), ""));

        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage("Reason is required when archiving a project.");
    }

    [Fact]
    public void Reason_WhenWhitespace_ShouldHaveValidationError()
    {
        var result = _sut.TestValidate(new ArchiveCommand(Guid.NewGuid(), "   "));

        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public void Reason_WhenExceeds1000Chars_ShouldHaveValidationError()
    {
        var reason = new string('x', 1001);

        var result = _sut.TestValidate(new ArchiveCommand(Guid.NewGuid(), reason));

        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage("Reason cannot exceed 1000 characters.");
    }

    [Fact]
    public void Reason_WhenExactly1000Chars_ShouldNotHaveValidationError()
    {
        var reason = new string('x', 1000);

        var result = _sut.TestValidate(new ArchiveCommand(Guid.NewGuid(), reason));

        result.ShouldNotHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public void Reason_WhenValid_ShouldNotHaveValidationError()
    {
        var result = _sut.TestValidate(new ArchiveCommand(Guid.NewGuid(), "Project archived after completion."));

        result.ShouldNotHaveValidationErrorFor(x => x.Reason);
    }
}

/// <summary>
/// Тесты валидатора <see cref="FailQACommandValidator"/>.
/// </summary>
public class FailQACommandValidatorTests
{
    private readonly FailQACommandValidator _sut = new();

    [Fact]
    public void Reason_WhenEmpty_ShouldHaveValidationError()
    {
        var result = _sut.TestValidate(new FailQACommand(Guid.NewGuid(), ""));

        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage("Reason is required when failing QA.");
    }

    [Fact]
    public void Reason_WhenExceeds1000Chars_ShouldHaveValidationError()
    {
        var result = _sut.TestValidate(new FailQACommand(Guid.NewGuid(), new string('a', 1001)));

        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage("Reason cannot exceed 1000 characters.");
    }

    [Fact]
    public void Reason_WhenExactly1000Chars_ShouldPassValidation()
    {
        var result = _sut.TestValidate(new FailQACommand(Guid.NewGuid(), new string('a', 1000)));

        result.ShouldNotHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public void Reason_WhenValid_ShouldPassValidation()
    {
        var result = _sut.TestValidate(new FailQACommand(Guid.NewGuid(), "Critical bug in payment module."));

        result.ShouldNotHaveValidationErrorFor(x => x.Reason);
    }
}

/// <summary>
/// Тесты валидатора <see cref="ReturnToDesignCommandValidator"/>.
/// </summary>
public class ReturnToDesignCommandValidatorTests
{
    private readonly ReturnToDesignCommandValidator _sut = new();

    [Fact]
    public void Reason_WhenEmpty_ShouldHaveValidationError()
    {
        var result = _sut.TestValidate(new ReturnToDesignCommand(Guid.NewGuid(), ""));

        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage("Reason is required when returning to design.");
    }

    [Fact]
    public void Reason_WhenExceeds1000Chars_ShouldHaveValidationError()
    {
        var result = _sut.TestValidate(new ReturnToDesignCommand(Guid.NewGuid(), new string('z', 1001)));

        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage("Reason cannot exceed 1000 characters.");
    }

    [Fact]
    public void Reason_WhenValid_ShouldPassValidation()
    {
        var result = _sut.TestValidate(new ReturnToDesignCommand(Guid.NewGuid(), "Requirements changed significantly."));

        result.ShouldNotHaveValidationErrorFor(x => x.Reason);
    }
}

/// <summary>
/// Тесты валидатора <see cref="CreateProjectCommandValidator"/>.
/// </summary>
public class CreateProjectCommandValidatorTests
{
    private readonly CreateProjectCommandValidator _sut = new();

    private static CreateProjectCommand ValidCommand() =>
        new("Valid Project", "Description", new List<Guid>());

    // ─── Name ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Name_WhenEmpty_ShouldHaveValidationError()
    {
        var cmd = ValidCommand() with { Name = "" };

        var result = _sut.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Project name is required.");
    }

    [Fact]
    public void Name_WhenExceeds200Chars_ShouldHaveValidationError()
    {
        var cmd = ValidCommand() with { Name = new string('a', 201) };

        var result = _sut.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Project name cannot exceed 200 characters.");
    }

    [Fact]
    public void Name_WhenExactly200Chars_ShouldPassValidation()
    {
        var cmd = ValidCommand() with { Name = new string('a', 200) };

        var result = _sut.TestValidate(cmd);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("Project@Name")]
    [InlineData("Project!")]
    [InlineData("Project#1")]
    public void Name_WithDisallowedChars_ShouldHaveValidationError(string name)
    {
        var cmd = ValidCommand() with { Name = name };

        var result = _sut.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Project name can only contain letters, numbers, spaces, hyphens, and underscores.");
    }

    [Theory]
    [InlineData("Valid Project")]
    [InlineData("Project-Name_2024")]
    [InlineData("Проект Альфа")]
    [InlineData("Project123")]
    public void Name_WithAllowedChars_ShouldPassValidation(string name)
    {
        var cmd = ValidCommand() with { Name = name };

        var result = _sut.TestValidate(cmd);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    // ─── Description ──────────────────────────────────────────────────────────

    [Fact]
    public void Description_WhenExceeds1000Chars_ShouldHaveValidationError()
    {
        var cmd = ValidCommand() with { Description = new string('d', 1001) };

        var result = _sut.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description cannot exceed 1000 characters.");
    }

    [Fact]
    public void Description_WhenEmpty_ShouldPassValidation()
    {
        var cmd = ValidCommand() with { Description = "" };

        var result = _sut.TestValidate(cmd);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Description_WhenNull_ShouldPassValidation()
    {
        var cmd = ValidCommand() with { Description = null! };

        var result = _sut.TestValidate(cmd);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Description_WhenExactly1000Chars_ShouldPassValidation()
    {
        var cmd = ValidCommand() with { Description = new string('d', 1000) };

        var result = _sut.TestValidate(cmd);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    // ─── MemberIds ────────────────────────────────────────────────────────────

    [Fact]
    public void MemberIds_WithEmptyGuid_ShouldHaveValidationError()
    {
        var cmd = ValidCommand() with { MemberIds = new List<Guid> { Guid.Empty } };

        var result = _sut.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.MemberIds)
            .WithErrorMessage("Invalid member IDs provided.");
    }

    [Fact]
    public void MemberIds_WithValidGuids_ShouldPassValidation()
    {
        var cmd = ValidCommand() with { MemberIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() } };

        var result = _sut.TestValidate(cmd);

        result.ShouldNotHaveValidationErrorFor(x => x.MemberIds);
    }

    [Fact]
    public void MemberIds_WhenNull_ShouldPassValidation()
    {
        var cmd = ValidCommand() with { MemberIds = null! };

        var result = _sut.TestValidate(cmd);

        result.ShouldNotHaveValidationErrorFor(x => x.MemberIds);
    }

    [Fact]
    public void MemberIds_WhenEmpty_ShouldPassValidation()
    {
        var cmd = ValidCommand() with { MemberIds = new List<Guid>() };

        var result = _sut.TestValidate(cmd);

        result.ShouldNotHaveValidationErrorFor(x => x.MemberIds);
    }

    [Fact]
    public void MemberIds_MixedValidAndEmptyGuid_ShouldHaveValidationError()
    {
        var cmd = ValidCommand() with
        {
            MemberIds = new List<Guid> { Guid.NewGuid(), Guid.Empty }
        };

        var result = _sut.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.MemberIds);
    }

    // ─── Полностью валидная команда ───────────────────────────────────────────

    [Fact]
    public void AllFields_WhenValid_ShouldHaveNoErrors()
    {
        var result = _sut.TestValidate(ValidCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }
}

/// <summary>
/// Тесты валидатора <see cref="CreateSpecificationCommandValidator"/>.
/// </summary>
public class CreateSpecificationCommandValidatorTests
{
    private readonly CreateSpecificationCommandValidator _sut = new();

    [Fact]
    public void Content_WhenEmpty_ShouldHaveValidationError()
    {
        var cmd = new CreateSpecificationCommand(Guid.NewGuid(), "");

        var result = _sut.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Specification content is required.");
    }

    [Fact]
    public void Content_WhenWhitespace_ShouldHaveValidationError()
    {
        var cmd = new CreateSpecificationCommand(Guid.NewGuid(), "   ");

        var result = _sut.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void Content_WhenExceeds10000Chars_ShouldHaveValidationError()
    {
        var cmd = new CreateSpecificationCommand(Guid.NewGuid(), new string('x', 10001));

        var result = _sut.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Specification content cannot exceed 10000 characters.");
    }

    [Fact]
    public void Content_WhenExactly10000Chars_ShouldPassValidation()
    {
        var cmd = new CreateSpecificationCommand(Guid.NewGuid(), new string('x', 10000));

        var result = _sut.TestValidate(cmd);

        result.ShouldNotHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void Content_WhenValid_ShouldHaveNoErrors()
    {
        var cmd = new CreateSpecificationCommand(Guid.NewGuid(), "Functional requirements v1.0");

        var result = _sut.TestValidate(cmd);

        result.ShouldNotHaveAnyValidationErrors();
    }
}