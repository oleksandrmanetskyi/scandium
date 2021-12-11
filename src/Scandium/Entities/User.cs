using MongoDB.Bson;

namespace Scandium.Entities;

public class User
{
    public ObjectId Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
}

public enum UserRole
{
    User,
    Admin
}