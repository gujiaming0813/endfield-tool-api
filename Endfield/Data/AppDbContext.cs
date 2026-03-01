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
            // 表名
            entity.ToTable("bilibili_videos");

            // 主键
            entity.HasKey(e => e.Id);

            // 字段配置
            entity.Property(e => e.Bvid)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("bvid");

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("title");

            entity.Property(e => e.Cover)
                .IsRequired()
                .HasMaxLength(1000)
                .HasColumnName("cover");

            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");

            entity.Property(e => e.Duration)
                .HasColumnName("duration");

            entity.Property(e => e.OwnerName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("owner_name");

            entity.Property(e => e.Url)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("url");

            entity.Property(e => e.ViewCount)
                .HasColumnName("view_count");

            entity.Property(e => e.LikeCount)
                .HasColumnName("like_count");

            entity.Property(e => e.PublishTime)
                .HasColumnName("publish_time");

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
            // 表名
            entity.ToTable("video_tags");

            // 主键
            entity.HasKey(e => e.Id);

            // 字段配置
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("name");

            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("code");

            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");

            entity.Property(e => e.SortOrder)
                .HasColumnName("sort_order");

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
            // 表名
            entity.ToTable("video_tag_mappings");

            // 复合主键
            entity.HasKey(e => new { e.VideoId, e.TagId });

            // 字段配置
            entity.Property(e => e.VideoId)
                .HasColumnName("video_id");

            entity.Property(e => e.TagId)
                .HasColumnName("tag_id");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");

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
            // 表名
            entity.ToTable("users");

            // 主键
            entity.HasKey(e => e.Id);

            // 字段配置
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("username");

            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("password");

            entity.Property(e => e.Nickname)
                .HasMaxLength(50)
                .HasColumnName("nickname");

            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active");

            // 索引
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.IsDeleted);

            // 全局查询过滤器
            entity.HasQueryFilter(e => !e.IsDeleted);
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
                    .HasColumnName("comment");

                // 是否删除
                builder.Property(nameof(BaseAuditModel.IsDeleted))
                    .HasColumnName("is_deleted");

                // 创建人ID
                builder.Property(nameof(BaseAuditModel.CreatedBy))
                    .HasColumnName("created_by");

                // 创建人名称
                builder.Property(nameof(BaseAuditModel.CreatedName))
                    .HasMaxLength(100)
                    .HasColumnName("created_name");

                // 创建时间
                builder.Property(nameof(BaseAuditModel.CreatedAt))
                    .HasColumnName("created_at");

                // 最后修改人ID
                builder.Property(nameof(BaseAuditModel.UpdatedBy))
                    .HasColumnName("updated_by");

                // 最后修改人名称
                builder.Property(nameof(BaseAuditModel.UpdatedName))
                    .HasMaxLength(100)
                    .HasColumnName("updated_name");

                // 最后修改时间
                builder.Property(nameof(BaseAuditModel.UpdatedAt))
                    .HasColumnName("updated_at");
            }
        }
    }
}
