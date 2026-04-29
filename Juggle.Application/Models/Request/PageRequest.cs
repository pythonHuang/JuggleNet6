namespace Juggle.Application.Models.Request;

/// <summary>
/// 分页查询请求模型
/// 用于列表数据的分页查询
/// </summary>
public class PageRequest
{
    /// <summary>
    /// 页码（从 1 开始）
    /// </summary>
    public int PageNum { get; set; } = 1;

    /// <summary>
    /// 每页记录数
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// 关键词搜索（可选）
    /// 用于模糊匹配相关字段
    /// </summary>
    public string? Keyword { get; set; }
}