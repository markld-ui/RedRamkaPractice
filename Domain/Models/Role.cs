namespace Domain.Models;

//public enum Role
//{
//    Developer = 1,
//    Tester = 2,
//    ProductManager = 3,
//    ProjectManager = 4,
//    DevOps = 5
//}

public class Role
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<UserRole> UserRoles { get; set; } =
        new List<UserRole>();

}
