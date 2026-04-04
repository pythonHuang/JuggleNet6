namespace Juggle.Domain.Entities;

/// <summary>角色</summary>
public class RoleEntity : BaseEntity
{
    public string RoleName { get; set; } = "";
    public string? RoleCode { get; set; }
    public string? Remark { get; set; }
}
