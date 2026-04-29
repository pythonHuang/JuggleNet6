namespace Juggle.Application.Models.Response;

/// <summary>
/// 分页查询结果封装
/// 统一分页接口的返回格式
/// </summary>
/// <typeparam name="T">分页数据的类型</typeparam>
public class PageResult<T>
{
    /// <summary>
    /// 总记录数
    /// </summary>
    public long Total { get; set; }

    /// <summary>
    /// 当前页码
    /// </summary>
    public int PageNum { get; set; }

    /// <summary>
    /// 每页记录数
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 当前页的数据列表
    /// </summary>
    public List<T> Records { get; set; } = new();
}