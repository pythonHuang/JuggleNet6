namespace Juggle.Domain.Entities;

/// <summary>
/// 数据源配置实体
/// 用于存储流程中数据库节点所需的连接信息
/// 支持 MySQL、SQLite、PostgreSQL、SQL Server、Oracle、达梦等多种数据库
/// </summary>
public class DataSourceEntity : BaseEntity
{
    /// <summary>
    /// 数据源名称（唯一标识）
    /// 流程中通过此名称引用数据源
    /// </summary>
    public string? DsName { get; set; }

    /// <summary>
    /// 数据源类型：
    /// - mysql：MySQL
    /// - sqlite：SQLite
    /// - postgresql / postgres：PostgreSQL
    /// - sqlserver / mssql：SQL Server
    /// - oracle：Oracle
    /// - dm：达梦数据库
    /// </summary>
    public string? DsType { get; set; }

    /// <summary>
    /// 数据库服务器地址
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// 数据库服务端口
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// 数据库名称
    /// </summary>
    public string? DbName { get; set; }

    /// <summary>
    /// 数据库用户名
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// 数据库密码（建议加密存储）
    /// </summary>
    public string? Password { get; set; }
}
