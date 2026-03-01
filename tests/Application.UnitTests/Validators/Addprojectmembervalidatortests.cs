using Application.Common.Constants;
using Application.Features.Projects.Commands;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Validators;

/// <summary>
/// Тесты валидатора <see cref="AddProjectMemberCommandValidator"/>.
/// </summary>
public class AddProjectMemberCommandValidatorTests
{
    private readonly AddProjectMemberCommandValidator _sut = new();

    private static AddProjectMemberCommand ValidCommand() =>
        new(Guid.NewGuid(), Guid.NewGuid(), RoleConstants.Developer);

    // ─── UserId ───────────────────────────────────────────────────────────────

    [Fact]
    public void UserId_WhenEmpty_ShouldHaveValidationError()
    {
        var cmd = ValidCommand() with { UserId = Guid.Empty };

        var result = _sut.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void UserId_WhenValid_ShouldPassValidation()
    {
        var result = _sut.TestValidate(ValidCommand());

        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }

    // ─── RoleName ─────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(RoleConstants.Developer)]
    [InlineData(RoleConstants.Tester)]
    [InlineData(RoleConstants.ProductManager)]
    [InlineData(RoleConstants.DevOps)]
    public void RoleName_WhenAllowedRole_ShouldPassValidation(string role)
    {
        var cmd = ValidCommand() with { RoleName = role };

        var result = _sut.TestValidate(cmd);

        result.ShouldNotHaveValidationErrorFor(x => x.RoleName);
    }

    [Theory]
    [InlineData(RoleConstants.ProjectManager)]
    [InlineData(RoleConstants.Admin)]
    [InlineData("UnknownRole")]
    [InlineData("")]
    public void RoleName_WhenDisallowedRole_ShouldHaveValidationError(string role)
    {
        var cmd = ValidCommand() with { RoleName = role };

        var result = _sut.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.RoleName);
    }

    // ─── Полностью валидная команда ───────────────────────────────────────────

    [Fact]
    public void AllFields_WhenValid_ShouldHaveNoErrors()
    {
        var result = _sut.TestValidate(ValidCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }
}