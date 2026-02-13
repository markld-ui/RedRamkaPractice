using Domain.Projects;

namespace Domain.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public Credentials Credentials { get; set; } = null!;

    public ICollection<ProjectMember> ProjectMemberships { get; set; } =
        new List<ProjectMember>();
    public ICollection<UserRole> UserRoles { get; set; } = 
        new List<UserRole>();
}
