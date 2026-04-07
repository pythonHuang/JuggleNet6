namespace Juggle.Domain.Entities;

public class DataSourceEntity : BaseEntity
{
    public string? DsName { get; set; }
    public string? DsType { get; set; }   // mysql
    public string? Host { get; set; }
    public int Port { get; set; }
    public string? DbName { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}
