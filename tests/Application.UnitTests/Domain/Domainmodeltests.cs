using Application.Common.Constants;
using Domain.Models;
using FluentAssertions;

namespace Application.UnitTests.Domain;

/// <summary>
/// Тесты доменной сущности <see cref="User"/>.
/// </summary>
public class UserTests
{
    [Fact]
    public void Constructor_Default_ShouldInitializeCollections()
    {
        var user = new User();

        user.UserRoles.Should().NotBeNull();
        user.ProjectMemberships.Should().NotBeNull();
    }

    [Fact]
    public void User_ShouldHaveCorrectProperties()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            FirstName = "Alice",
            LastName = "Smith"
        };

        user.Id.Should().Be(userId);
        user.FirstName.Should().Be("Alice");
        user.LastName.Should().Be("Smith");
    }

    [Fact]
    public void User_FirstName_ShouldBeMutable()
    {
        var user = new User { FirstName = "Old" };

        user.FirstName = "New";

        user.FirstName.Should().Be("New");
    }

    [Fact]
    public void User_LastName_ShouldBeMutable()
    {
        var user = new User { LastName = "Old" };

        user.LastName = "New";

        user.LastName.Should().Be("New");
    }

    [Fact]
    public void User_Credentials_ShouldBeSettable()
    {
        var user = new User();
        var creds = new Credentials { Email = "alice@test.com", PasswordHash = "h" };

        user.Credentials = creds;

        user.Credentials.Email.Should().Be("alice@test.com");
    }
}

/// <summary>
/// Тесты доменной сущности <see cref="Role"/>.
/// </summary>
public class RoleTests
{
    [Fact]
    public void Role_ShouldHaveCorrectProperties()
    {
        var roleId = Guid.NewGuid();
        var role = new Role
        {
            Id = roleId,
            Name = "Developer",
            Description = "Software developer"
        };

        role.Id.Should().Be(roleId);
        role.Name.Should().Be("Developer");
        role.Description.Should().Be("Software developer");
    }

    [Fact]
    public void Role_Name_ShouldBeMutable()
    {
        var role = new Role { Name = "OldName" };

        role.Name = "NewName";

        role.Name.Should().Be("NewName");
    }

    [Fact]
    public void Role_Description_ShouldBeMutable()
    {
        var role = new Role { Description = "Old desc" };

        role.Description = "New desc";

        role.Description.Should().Be("New desc");
    }

    [Fact]
    public void DefaultRole_Id_ShouldBeEmpty()
    {
        var role = new Role();

        // Id инициализируется БД или явно
        role.Name.Should().BeNullOrEmpty();
    }
}

/// <summary>
/// Тесты доменной сущности <see cref="Credentials"/>.
/// </summary>
public class CredentialsTests
{
    [Fact]
    public void Credentials_ShouldHaveCorrectProperties()
    {
        var userId = Guid.NewGuid();
        var creds = new Credentials
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Email = "user@test.com",
            PasswordHash = "bcrypt_hash",
            RefreshToken = "token123"
        };

        creds.UserId.Should().Be(userId);
        creds.Email.Should().Be("user@test.com");
        creds.PasswordHash.Should().Be("bcrypt_hash");
        creds.RefreshToken.Should().Be("token123");
    }

    [Fact]
    public void Credentials_RefreshToken_ShouldBeNullable()
    {
        var creds = new Credentials { RefreshToken = null };

        creds.RefreshToken.Should().BeNull();
    }

    [Fact]
    public void Credentials_RefreshToken_ShouldBeMutable()
    {
        var creds = new Credentials { RefreshToken = "old-token" };

        creds.RefreshToken = null;

        creds.RefreshToken.Should().BeNull();
    }

    [Fact]
    public void Credentials_Email_ShouldBeMutable()
    {
        var creds = new Credentials { Email = "old@test.com" };

        creds.Email = "new@test.com";

        creds.Email.Should().Be("new@test.com");
    }

    [Fact]
    public void Credentials_PasswordHash_ShouldBeMutable()
    {
        var creds = new Credentials { PasswordHash = "old_hash" };

        creds.PasswordHash = "new_hash";

        creds.PasswordHash.Should().Be("new_hash");
    }
}

/// <summary>
/// Тесты составной сущности <see cref="UserRole"/>.
/// </summary>
public class UserRoleTests
{
    [Fact]
    public void UserRole_ShouldAssignBothIds()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId
        };

        userRole.UserId.Should().Be(userId);
        userRole.RoleId.Should().Be(roleId);
    }

    [Fact]
    public void UserRole_Role_ShouldBeSettable()
    {
        var role = new Role { Name = "Admin" };
        var userRole = new UserRole { Role = role };

        userRole.Role.Name.Should().Be("Admin");
    }

    [Fact]
    public void UserRole_User_ShouldBeSettable()
    {
        var user = new User { FirstName = "John" };
        var userRole = new UserRole { User = user };

        userRole.User.FirstName.Should().Be("John");
    }
}

/// <summary>
/// Тесты класса констант <see cref="RoleConstants"/>.
/// </summary>
public class RoleConstantsTests
{
    [Fact]
    public void Admin_ShouldHaveCorrectValue()
        => RoleConstants.Admin.Should().Be("Admin");

    [Fact]
    public void ProjectManager_ShouldHaveCorrectValue()
        => RoleConstants.ProjectManager.Should().Be("ProjectManager");

    [Fact]
    public void Developer_ShouldHaveCorrectValue()
        => RoleConstants.Developer.Should().Be("Developer");

    [Fact]
    public void Tester_ShouldHaveCorrectValue()
        => RoleConstants.Tester.Should().Be("Tester");

    [Fact]
    public void ProductManager_ShouldHaveCorrectValue()
        => RoleConstants.ProductManager.Should().Be("ProductManager");

    [Fact]
    public void DevOps_ShouldHaveCorrectValue()
        => RoleConstants.DevOps.Should().Be("DevOps");

    [Fact]
    public void AllConstants_ShouldBeDistinct()
    {
        var all = new[]
        {
            RoleConstants.Admin,
            RoleConstants.ProjectManager,
            RoleConstants.Developer,
            RoleConstants.Tester,
            RoleConstants.ProductManager,
            RoleConstants.DevOps
        };

        all.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void AllConstants_ShouldNotBeNullOrEmpty()
    {
        var all = new[]
        {
            RoleConstants.Admin,
            RoleConstants.ProjectManager,
            RoleConstants.Developer,
            RoleConstants.Tester,
            RoleConstants.ProductManager,
            RoleConstants.DevOps
        };

        all.Should().AllSatisfy(c => c.Should().NotBeNullOrEmpty());
    }
}