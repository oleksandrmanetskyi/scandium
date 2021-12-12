using MongoDB.Bson;

namespace Scandium.Entities;

public class User
{
    public ObjectId Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
}

public enum UserRole
{
    User,
    Admin
}