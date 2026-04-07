namespace Juggle.Application.Models.Request;

public class DataSourceAddRequest
{
    public string DsName { get; set; } = "";
    public string DsType { get; set; } = "mysql";
    public string Host { get; set; } = "";
    public int Port { get; set; } = 3306;
    public string DbName { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}