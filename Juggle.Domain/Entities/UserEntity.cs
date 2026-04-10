namespace Juggle.Domain.Entities;

public class UserEntity : BaseEntity
{
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public long? RoleId { get; set; }
    // TenantId 继承自 BaseEntity
}
