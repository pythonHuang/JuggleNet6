namespace Juggle.Application.Models.Request;

/// <summary>
/// 新增数据源请求模型
/// </summary>
public class DataSourceAddRequest
{
    /// <summary>
    /// 数据源名称（唯一标识）
    /// </summary>
    public string DsName { get; set; } = "";

    /// <summary>
    /// 数据源类型：mysql / sqlite / postgresql / sqlserver / oracle / dm
    /// </summary>
    public string DsType { get; set; } = "mysql";

    /// <summary>
    /// 服务器地址
    /// </summary>
    public string Host { get; set; } = "";

    /// <summary>
    /// 端口号（MySQL 默认 3306）
    /// </summary>
    public int Port { get; set; } = 3306;

    /// <summary>
    /// 数据库名称
    /// </summary>
    public string DbName { get; set; } = "";

    /// <summary>
    /// 数据库用户名
    /// </summary>
    public string Username { get; set; } = "";

    /// <summary>
    /// 数据库密码
    /// </summary>
    public string Password { get; set; } = "";
}