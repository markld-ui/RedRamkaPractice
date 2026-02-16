namespace Domain.Models;

//    Developer,
//    Tester,
//    ProductManager,
//    ProjectManager,
//    DevOps

public class Role
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<UserRole> UserRoles { get; set; } =
        new List<UserRole>();

}
