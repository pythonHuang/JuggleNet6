namespace Juggle.Domain.Entities;

public class SuiteEntity : BaseEntity
{
    public string? SuiteCode { get; set; }
    public string? SuiteName { get; set; }
    public long? SuiteClassifyId { get; set; }
    public string? SuiteImage { get; set; }
    public string? SuiteVersion { get; set; }
    public string? SuiteDesc { get; set; }
    public string? SuiteHelpDocJson { get; set; }
    public int SuiteFlag { get; set; } = 0; // 0:自定义, 1:官方
}
