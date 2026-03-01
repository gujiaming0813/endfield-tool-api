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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 配置B站视频实体
        modelBuilder.Entity<BilibiliVideo>(entity =>
        {
            // BV号唯一索引
            entity.HasIndex(e => e.Bvid).IsUnique();

            // 创建时间索引（用于查询排序）
            entity.HasIndex(e => e.CreatedAt);

            // 软删除索引
            entity.HasIndex(e => e.IsDeleted);

            // 全局查询过滤器（自动过滤已删除记录）
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // 配置视频标签实体
        modelBuilder.Entity<VideoTag>(entity =>
        {
            // 编码唯一索引
            entity.HasIndex(e => e.Code).IsUnique();

            // 排序索引
            entity.HasIndex(e => e.SortOrder);

            // 软删除索引
            entity.HasIndex(e => e.IsDeleted);

            // 全局查询过滤器（自动过滤已删除记录）
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // 配置视频标签关联表（多对多）
        modelBuilder.Entity<VideoTagMapping>(entity =>
        {
            // 复合主键
            entity.HasKey(e => new { e.VideoId, e.TagId });

            // 配置与视频的关系
            entity.HasOne(m => m.Video)
                .WithMany(v => v.VideoTagMappings)
                .HasForeignKey(m => m.VideoId)
                .OnDelete(DeleteBehavior.Cascade);

            // 配置与标签的关系
            entity.HasOne(m => m.Tag)
                .WithMany(t => t.VideoTagMappings)
                .HasForeignKey(m => m.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            // 创建时间索引
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
