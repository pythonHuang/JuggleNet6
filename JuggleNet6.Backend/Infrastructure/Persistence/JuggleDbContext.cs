using JuggleNet6.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JuggleNet6.Backend.Infrastructure.Persistence;

public class JuggleDbContext : DbContext
{
    public JuggleDbContext(DbContextOptions<JuggleDbContext> options) : base(options) { }

    public DbSet<UserEntity> Users { get; set; } = null!;
    public DbSet<SuiteEntity> Suites { get; set; } = null!;
    public DbSet<ApiEntity> Apis { get; set; } = null!;
    public DbSet<ParameterEntity> Parameters { get; set; } = null!;
    public DbSet<ObjectEntity> Objects { get; set; } = null!;
    public DbSet<FlowDefinitionEntity> FlowDefinitions { get; set; } = null!;
    public DbSet<VariableInfoEntity> VariableInfos { get; set; } = null!;
    public DbSet<FlowInfoEntity> FlowInfos { get; set; } = null!;
    public DbSet<FlowVersionEntity> FlowVersions { get; set; } = null!;
    public DbSet<TokenEntity> Tokens { get; set; } = null!;
    public DbSet<DataSourceEntity> DataSources { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 表名映射
        modelBuilder.Entity<UserEntity>().ToTable("t_user");
        modelBuilder.Entity<SuiteEntity>().ToTable("t_suite");
        modelBuilder.Entity<ApiEntity>().ToTable("t_api");
        modelBuilder.Entity<ParameterEntity>().ToTable("t_parameter");
        modelBuilder.Entity<ObjectEntity>().ToTable("t_object");
        modelBuilder.Entity<FlowDefinitionEntity>().ToTable("t_flow_definition");
        modelBuilder.Entity<VariableInfoEntity>().ToTable("t_variable_info");
        modelBuilder.Entity<FlowInfoEntity>().ToTable("t_flow_info");
        modelBuilder.Entity<FlowVersionEntity>().ToTable("t_flow_version");
        modelBuilder.Entity<TokenEntity>().ToTable("t_token");
        modelBuilder.Entity<DataSourceEntity>().ToTable("t_data_source");

        // 列名映射（snake_case）
        modelBuilder.Entity<UserEntity>(e => {
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.Deleted).HasColumnName("deleted");
            e.Property(p => p.CreatedAt).HasColumnName("created_at");
            e.Property(p => p.CreatedBy).HasColumnName("created_by");
            e.Property(p => p.UpdatedAt).HasColumnName("updated_at");
            e.Property(p => p.UpdatedBy).HasColumnName("updated_by");
            e.Property(p => p.UserName).HasColumnName("user_name");
            e.Property(p => p.Password).HasColumnName("password");
        });

        modelBuilder.Entity<SuiteEntity>(e => {
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.Deleted).HasColumnName("deleted");
            e.Property(p => p.CreatedAt).HasColumnName("created_at");
            e.Property(p => p.CreatedBy).HasColumnName("created_by");
            e.Property(p => p.UpdatedAt).HasColumnName("updated_at");
            e.Property(p => p.UpdatedBy).HasColumnName("updated_by");
            e.Property(p => p.SuiteCode).HasColumnName("suite_code");
            e.Property(p => p.SuiteName).HasColumnName("suite_name");
            e.Property(p => p.SuiteClassifyId).HasColumnName("suite_classify_id");
            e.Property(p => p.SuiteImage).HasColumnName("suite_image");
            e.Property(p => p.SuiteVersion).HasColumnName("suite_version");
            e.Property(p => p.SuiteDesc).HasColumnName("suite_desc");
            e.Property(p => p.SuiteHelpDocJson).HasColumnName("suite_help_doc_json");
            e.Property(p => p.SuiteFlag).HasColumnName("suite_flag");
        });

        modelBuilder.Entity<ApiEntity>(e => {
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.Deleted).HasColumnName("deleted");
            e.Property(p => p.CreatedAt).HasColumnName("created_at");
            e.Property(p => p.CreatedBy).HasColumnName("created_by");
            e.Property(p => p.UpdatedAt).HasColumnName("updated_at");
            e.Property(p => p.UpdatedBy).HasColumnName("updated_by");
            e.Property(p => p.SuiteCode).HasColumnName("suite_code");
            e.Property(p => p.MethodCode).HasColumnName("method_code");
            e.Property(p => p.MethodName).HasColumnName("method_name");
            e.Property(p => p.MethodDesc).HasColumnName("method_desc");
            e.Property(p => p.Url).HasColumnName("url");
            e.Property(p => p.RequestType).HasColumnName("request_type");
            e.Property(p => p.ContentType).HasColumnName("content_type");
            e.Property(p => p.MockJson).HasColumnName("mock_json");
        });

        modelBuilder.Entity<ParameterEntity>(e => {
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.Deleted).HasColumnName("deleted");
            e.Property(p => p.CreatedAt).HasColumnName("created_at");
            e.Property(p => p.CreatedBy).HasColumnName("created_by");
            e.Property(p => p.UpdatedAt).HasColumnName("updated_at");
            e.Property(p => p.UpdatedBy).HasColumnName("updated_by");
            e.Property(p => p.OwnerId).HasColumnName("owner_id");
            e.Property(p => p.OwnerCode).HasColumnName("owner_code");
            e.Property(p => p.ParamType).HasColumnName("param_type");
            e.Property(p => p.ParamCode).HasColumnName("param_code");
            e.Property(p => p.ParamName).HasColumnName("param_name");
            e.Property(p => p.DataType).HasColumnName("data_type");
            e.Property(p => p.ObjectCode).HasColumnName("object_code");
            e.Property(p => p.Required).HasColumnName("required");
            e.Property(p => p.DefaultValue).HasColumnName("default_value");
            e.Property(p => p.Description).HasColumnName("description");
            e.Property(p => p.SortNum).HasColumnName("sort_num");
        });

        modelBuilder.Entity<ObjectEntity>(e => {
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.Deleted).HasColumnName("deleted");
            e.Property(p => p.CreatedAt).HasColumnName("created_at");
            e.Property(p => p.CreatedBy).HasColumnName("created_by");
            e.Property(p => p.UpdatedAt).HasColumnName("updated_at");
            e.Property(p => p.UpdatedBy).HasColumnName("updated_by");
            e.Property(p => p.ObjectCode).HasColumnName("object_code");
            e.Property(p => p.ObjectName).HasColumnName("object_name");
            e.Property(p => p.ObjectDesc).HasColumnName("object_desc");
        });

        modelBuilder.Entity<FlowDefinitionEntity>(e => {
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.Deleted).HasColumnName("deleted");
            e.Property(p => p.CreatedAt).HasColumnName("created_at");
            e.Property(p => p.CreatedBy).HasColumnName("created_by");
            e.Property(p => p.UpdatedAt).HasColumnName("updated_at");
            e.Property(p => p.UpdatedBy).HasColumnName("updated_by");
            e.Property(p => p.FlowKey).HasColumnName("flow_key");
            e.Property(p => p.FlowName).HasColumnName("flow_name");
            e.Property(p => p.FlowDesc).HasColumnName("flow_desc");
            e.Property(p => p.FlowContent).HasColumnName("flow_content");
            e.Property(p => p.FlowType).HasColumnName("flow_type");
            e.Property(p => p.Status).HasColumnName("status");
        });

        modelBuilder.Entity<VariableInfoEntity>(e => {
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.Deleted).HasColumnName("deleted");
            e.Property(p => p.CreatedAt).HasColumnName("created_at");
            e.Property(p => p.CreatedBy).HasColumnName("created_by");
            e.Property(p => p.UpdatedAt).HasColumnName("updated_at");
            e.Property(p => p.UpdatedBy).HasColumnName("updated_by");
            e.Property(p => p.FlowDefinitionId).HasColumnName("flow_definition_id");
            e.Property(p => p.FlowKey).HasColumnName("flow_key");
            e.Property(p => p.VariableCode).HasColumnName("variable_code");
            e.Property(p => p.VariableName).HasColumnName("variable_name");
            e.Property(p => p.DataType).HasColumnName("data_type");
            e.Property(p => p.VariableType).HasColumnName("variable_type");
            e.Property(p => p.DefaultValue).HasColumnName("default_value");
            e.Property(p => p.Description).HasColumnName("description");
        });

        modelBuilder.Entity<FlowInfoEntity>(e => {
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.Deleted).HasColumnName("deleted");
            e.Property(p => p.CreatedAt).HasColumnName("created_at");
            e.Property(p => p.CreatedBy).HasColumnName("created_by");
            e.Property(p => p.UpdatedAt).HasColumnName("updated_at");
            e.Property(p => p.UpdatedBy).HasColumnName("updated_by");
            e.Property(p => p.FlowKey).HasColumnName("flow_key");
            e.Property(p => p.FlowName).HasColumnName("flow_name");
            e.Property(p => p.FlowDesc).HasColumnName("flow_desc");
            e.Property(p => p.FlowType).HasColumnName("flow_type");
            e.Property(p => p.Status).HasColumnName("status");
        });

        modelBuilder.Entity<FlowVersionEntity>(e => {
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.Deleted).HasColumnName("deleted");
            e.Property(p => p.CreatedAt).HasColumnName("created_at");
            e.Property(p => p.CreatedBy).HasColumnName("created_by");
            e.Property(p => p.UpdatedAt).HasColumnName("updated_at");
            e.Property(p => p.UpdatedBy).HasColumnName("updated_by");
            e.Property(p => p.FlowInfoId).HasColumnName("flow_info_id");
            e.Property(p => p.FlowKey).HasColumnName("flow_key");
            e.Property(p => p.Version).HasColumnName("version");
            e.Property(p => p.FlowContent).HasColumnName("flow_content");
            e.Property(p => p.Status).HasColumnName("status");
        });

        modelBuilder.Entity<TokenEntity>(e => {
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.Deleted).HasColumnName("deleted");
            e.Property(p => p.CreatedAt).HasColumnName("created_at");
            e.Property(p => p.CreatedBy).HasColumnName("created_by");
            e.Property(p => p.UpdatedAt).HasColumnName("updated_at");
            e.Property(p => p.UpdatedBy).HasColumnName("updated_by");
            e.Property(p => p.TokenValue).HasColumnName("token_value");
            e.Property(p => p.TokenName).HasColumnName("token_name");
            e.Property(p => p.ExpiredAt).HasColumnName("expired_at");
            e.Property(p => p.Status).HasColumnName("status");
        });

        modelBuilder.Entity<DataSourceEntity>(e => {
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.Deleted).HasColumnName("deleted");
            e.Property(p => p.CreatedAt).HasColumnName("created_at");
            e.Property(p => p.CreatedBy).HasColumnName("created_by");
            e.Property(p => p.UpdatedAt).HasColumnName("updated_at");
            e.Property(p => p.UpdatedBy).HasColumnName("updated_by");
            e.Property(p => p.DsName).HasColumnName("ds_name");
            e.Property(p => p.DsType).HasColumnName("ds_type");
            e.Property(p => p.Host).HasColumnName("host");
            e.Property(p => p.Port).HasColumnName("port");
            e.Property(p => p.DbName).HasColumnName("db_name");
            e.Property(p => p.Username).HasColumnName("username");
            e.Property(p => p.Password).HasColumnName("password");
        });

        // 初始数据
        modelBuilder.Entity<UserEntity>().HasData(new UserEntity
        {
            Id = 1,
            UserName = "juggle",
            Password = "24cb6bcbc65730e9650745d379613563", // juggle MD5
            Deleted = 0,
            CreatedAt = "2026-01-01T00:00:00"
        });
    }
}
