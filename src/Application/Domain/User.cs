using Application.Domain.Projects;

namespace Application.Domain;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public Guid CredId { get; set; }
    public Credentials Credentials { get; set; } = null!;
    public ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();
}
