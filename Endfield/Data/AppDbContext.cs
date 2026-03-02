using Endfield.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Endfield.Api.Data;

/// <summary>
/// 应用数据库上下文
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// B站视频信息表
    /// </summary>
    public DbSet<BilibiliVideo> BilibiliVideos => Set<BilibiliVideo>();

    /// <summary>
    /// 视频标签表
    /// </summary>
    public DbSet<VideoTag> VideoTags => Set<VideoTag>();

    /// <summary>
    /// 视频标签关联表
    /// </summary>
    public DbSet<VideoTagMapping> VideoTagMappings => Set<VideoTagMapping>();

    /// <summary>
    /// 用户表
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// 请求日志表
    /// </summary>
    public DbSet<RequestLog> RequestLogs => Set<RequestLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 配置B站视频实体
        ConfigureBilibiliVideo(modelBuilder);

        // 配置视频标签实体
        ConfigureVideoTag(modelBuilder);

        // 配置视频标签关联表
        ConfigureVideoTagMapping(modelBuilder);

        // 配置用户实体
        ConfigureUser(modelBuilder);

        // 配置请求日志实体
        ConfigureRequestLog(modelBuilder);

        // 配置审计模型基类
        ConfigureBaseAuditModel(modelBuilder);
    }

    /// <summary>
    /// 配置B站视频实体
    /// </summary>
    private static void ConfigureBilibiliVideo(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BilibiliVideo>(entity =>
        {
            // 表名和表注释
            entity.ToTable("bilibili_videos", t => t.HasComment("B站视频信息表"));

            // 主键
            entity.HasKey(e => e.Id);

            // 字段配置
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasComment("主键ID");

            entity.Property(e => e.Bvid)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("bvid")
                .HasComment("B站视频唯一标识");

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("title")
                .HasComment("视频标题");

            entity.Property(e => e.Cover)
                .IsRequired()
                .HasMaxLength(1000)
                .HasColumnName("cover")
                .HasComment("视频封面URL");

            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description")
                .HasComment("视频简介");

            entity.Property(e => e.Duration)
                .HasColumnName("duration")
                .HasComment("视频时长（秒）");

            entity.Property(e => e.OwnerName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("owner_name")
                .HasComment("UP主名称");

            entity.Property(e => e.Url)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("url")
                .HasComment("视频链接");

            entity.Property(e => e.ViewCount)
                .HasColumnName("view_count")
                .HasComment("播放量");

            entity.Property(e => e.LikeCount)
                .HasColumnName("like_count")
                .HasComment("点赞数");

            entity.Property(e => e.PublishTime)
                .HasColumnName("publish_time")
                .HasComment("发布时间");

            // 索引
            entity.HasIndex(e => e.Bvid).IsUnique();
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.IsDeleted);

            // 全局查询过滤器
            entity.HasQueryFilter(e => !e.IsDeleted);

            // 关联关系
            entity.HasMany(e => e.VideoTagMappings)
                .WithOne(m => m.Video)
                .HasForeignKey(m => m.VideoId);
        });
    }

    /// <summary>
    /// 配置视频标签实体
    /// </summary>
    private static void ConfigureVideoTag(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VideoTag>(entity =>
        {
            // 表名和表注释
            entity.ToTable("video_tags", t => t.HasComment("视频标签表"));

            // 主键
            entity.HasKey(e => e.Id);

            // 字段配置
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasComment("主键ID");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("name")
                .HasComment("标签名称");

            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("code")
                .HasComment("标签编码（唯一标识）");

            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description")
                .HasComment("标签描述");

            entity.Property(e => e.SortOrder)
                .HasColumnName("sort_order")
                .HasComment("排序序号");

            // 索引
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.SortOrder);
            entity.HasIndex(e => e.IsDeleted);

            // 全局查询过滤器
            entity.HasQueryFilter(e => !e.IsDeleted);

            // 关联关系
            entity.HasMany(e => e.VideoTagMappings)
                .WithOne(m => m.Tag)
                .HasForeignKey(m => m.TagId);
        });
    }

    /// <summary>
    /// 配置视频标签关联表
    /// </summary>
    private static void ConfigureVideoTagMapping(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VideoTagMapping>(entity =>
        {
            // 表名和表注释
            entity.ToTable("video_tag_mappings", t => t.HasComment("视频标签关联表"));

            // 复合主键
            entity.HasKey(e => new { e.VideoId, e.TagId });

            // 字段配置
            entity.Property(e => e.VideoId)
                .HasColumnName("video_id")
                .HasComment("视频ID");

            entity.Property(e => e.TagId)
                .HasColumnName("tag_id")
                .HasComment("标签ID");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasComment("创建时间");

            // 索引
            entity.HasIndex(e => e.CreatedAt);

            // 关联关系
            entity.HasOne(m => m.Video)
                .WithMany(v => v.VideoTagMappings)
                .HasForeignKey(m => m.VideoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.Tag)
                .WithMany(t => t.VideoTagMappings)
                .HasForeignKey(m => m.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// 配置用户实体
    /// </summary>
    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            // 表名和表注释
            entity.ToTable("users", t => t.HasComment("用户表"));

            // 主键
            entity.HasKey(e => e.Id);

            // 字段配置
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasComment("用户ID");

            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("username")
                .HasComment("用户名（登录名）");

            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("password")
                .HasComment("密码（加密存储）");

            entity.Property(e => e.Nickname)
                .HasMaxLength(50)
                .HasColumnName("nickname")
                .HasComment("昵称");

            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email")
                .HasComment("邮箱地址");

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasComment("是否启用");

            // 索引
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.IsDeleted);

            // 全局查询过滤器
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

    /// <summary>
    /// 配置请求日志实体
    /// </summary>
    private static void ConfigureRequestLog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RequestLog>(entity =>
        {
            // 表名和表注释
            entity.ToTable("request_logs", t => t.HasComment("请求日志表"));

            // 主键
            entity.HasKey(e => e.Id);

            // 字段配置
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasComment("主键ID");

            entity.Property(e => e.CorrelationId)
                .HasMaxLength(64)
                .HasColumnName("correlation_id")
                .HasComment("请求追踪ID，用于关联一次请求的完整链路");

            entity.Property(e => e.RequestTime)
                .HasColumnName("request_time")
                .HasComment("请求时间（UTC）");

            entity.Property(e => e.RequestMethod)
                .HasMaxLength(10)
                .HasColumnName("request_method")
                .HasComment("请求方法（GET/POST/PUT/DELETE等）");

            entity.Property(e => e.RequestPath)
                .HasMaxLength(500)
                .HasColumnName("request_path")
                .HasComment("请求路径");

            entity.Property(e => e.QueryString)
                .HasMaxLength(2000)
                .HasColumnName("query_string")
                .HasComment("查询字符串");

            entity.Property(e => e.ApiName)
                .HasMaxLength(200)
                .HasColumnName("api_name")
                .HasComment("接口名称（Controller/Action格式）");

            entity.Property(e => e.RequestHeaders)
                .HasColumnType("text")
                .HasColumnName("request_headers")
                .HasComment("请求头（JSON格式）");

            entity.Property(e => e.RequestBody)
                .HasColumnType("longtext")
                .HasColumnName("request_body")
                .HasComment("请求入参（JSON格式，敏感信息已脱敏）");

            entity.Property(e => e.ClientIp)
                .HasMaxLength(50)
                .HasColumnName("client_ip")
                .HasComment("客户端IP地址");

            entity.Property(e => e.UserAgent)
                .HasMaxLength(500)
                .HasColumnName("user_agent")
                .HasComment("用户代理（浏览器/客户端信息）");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .HasComment("用户ID");

            entity.Property(e => e.UserName)
                .HasMaxLength(100)
                .HasColumnName("user_name")
                .HasComment("用户名");

            entity.Property(e => e.ResponseTime)
                .HasColumnName("response_time")
                .HasComment("响应时间（UTC）");

            entity.Property(e => e.DurationMs)
                .HasColumnName("duration_ms")
                .HasComment("执行耗时（毫秒）");

            entity.Property(e => e.StatusCode)
                .HasColumnName("status_code")
                .HasComment("HTTP状态码");

            entity.Property(e => e.IsSuccess)
                .HasColumnName("is_success")
                .HasComment("是否成功（200-399为成功）");

            entity.Property(e => e.ResponseHeaders)
                .HasColumnType("text")
                .HasColumnName("response_headers")
                .HasComment("响应头（JSON格式）");

            entity.Property(e => e.ResponseBody)
                .HasColumnType("longtext")
                .HasColumnName("response_body")
                .HasComment("返回结果（JSON格式）");

            entity.Property(e => e.ExceptionType)
                .HasMaxLength(200)
                .HasColumnName("exception_type")
                .HasComment("异常类型");

            entity.Property(e => e.ExceptionMessage)
                .HasMaxLength(4000)
                .HasColumnName("exception_message")
                .HasComment("异常消息");

            entity.Property(e => e.ExceptionStackTrace)
                .HasColumnType("text")
                .HasColumnName("exception_stack_trace")
                .HasComment("异常堆栈信息");

            entity.Property(e => e.LogLevel)
                .HasMaxLength(20)
                .HasColumnName("log_level")
                .HasComment("日志级别（Information/Warning/Error等）");

            entity.Property(e => e.Message)
                .HasMaxLength(4000)
                .HasColumnName("message")
                .HasComment("日志消息");

            entity.Property(e => e.ExtraData)
                .HasColumnType("text")
                .HasColumnName("extra_data")
                .HasComment("额外数据（JSON格式，用于存储自定义字段）");

            entity.Property(e => e.Environment)
                .HasMaxLength(50)
                .HasColumnName("environment")
                .HasComment("运行环境（Development/Production等）");

            entity.Property(e => e.MachineName)
                .HasMaxLength(100)
                .HasColumnName("machine_name")
                .HasComment("机器名/主机名");

            // 索引（提高查询性能）
            entity.HasIndex(e => e.CorrelationId);
            entity.HasIndex(e => e.RequestTime);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.IsSuccess);
            entity.HasIndex(e => e.StatusCode);
            entity.HasIndex(e => e.ApiName);
        });
    }

    /// <summary>
    /// 配置审计模型基类（共享字段配置）
    /// </summary>
    private static void ConfigureBaseAuditModel(ModelBuilder modelBuilder)
    {
        // 为所有继承 BaseAuditModel 的实体配置共享字段
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseAuditModel).IsAssignableFrom(entityType.ClrType))
            {
                var builder = modelBuilder.Entity(entityType.ClrType);

                // 备注
                builder.Property(nameof(BaseAuditModel.Comment))
                    .HasMaxLength(500)
                    .HasColumnName("comment")
                    .HasComment("备注信息");

                // 是否删除
                builder.Property(nameof(BaseAuditModel.IsDeleted))
                    .HasColumnName("is_deleted")
                    .HasComment("是否删除（软删除标记）");

                // 创建人ID
                builder.Property(nameof(BaseAuditModel.CreatedBy))
                    .HasColumnName("created_by")
                    .HasComment("创建人ID");

                // 创建人名称
                builder.Property(nameof(BaseAuditModel.CreatedName))
                    .HasMaxLength(100)
                    .HasColumnName("created_name")
                    .HasComment("创建人名称");

                // 创建时间
                builder.Property(nameof(BaseAuditModel.CreatedAt))
                    .HasColumnName("created_at")
                    .HasComment("创建时间");

                // 最后修改人ID
                builder.Property(nameof(BaseAuditModel.UpdatedBy))
                    .HasColumnName("updated_by")
                    .HasComment("最后修改人ID");

                // 最后修改人名称
                builder.Property(nameof(BaseAuditModel.UpdatedName))
                    .HasMaxLength(100)
                    .HasColumnName("updated_name")
                    .HasComment("最后修改人名称");

                // 最后修改时间
                builder.Property(nameof(BaseAuditModel.UpdatedAt))
                    .HasColumnName("updated_at")
                    .HasComment("最后修改时间");
            }
        }
    }
}
