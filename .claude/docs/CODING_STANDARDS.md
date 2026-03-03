# 编码规范

> 本文档定义项目的编码风格和最佳实践，AI 在编写代码时必须遵循。

## C# 编码规范

### 命名约定

| 类型 | 命名风格 | 示例 |
|------|----------|------|
| 类、接口 | PascalCase | `BilibiliService`, `IAuthService` |
| 方法 | PascalCase | `GetVideoByIdAsync` |
| 属性 | PascalCase | `CreatedAt`, `IsDeleted` |
| 私有字段 | _camelCase | `_logger`, `_dbContext` |
| 参数 | camelCase | `videoId`, `userName` |
| 常量 | PascalCase 或 UPPER_CASE | `MaxRetryCount` |

### 异步编程

**必须遵循异步优先原则**：

```csharp
// ✅ 正确：异步方法以 Async 结尾
public async Task<ReturnData<Video>> GetVideoByIdAsync(int id)
{
    var video = await _dbContext.Videos.FindAsync(id);
    return ReturnData<Video>.Success(video);
}

// ❌ 错误：同步阻塞
public ReturnData<Video> GetVideoById(int id)
{
    var video = _dbContext.Videos.Find(id);  // 同步调用
    return ReturnData<Video>.Success(video);
}
```

### 依赖注入

**构造函数注入**：

```csharp
public class BilibiliService : IBilibiliService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<BilibiliService> _logger;

    public BilibiliService(
        AppDbContext dbContext,
        ILogger<BilibiliService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
}
```

### 控制器规范

```csharp
[ApiController]
[Route("api/[controller]")]
public class VideosController : BaseController
{
    private readonly IVideoService _videoService;

    public VideosController(IVideoService videoService)
    {
        _videoService = videoService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _videoService.GetVideoByIdAsync(id);
        return Ok(result);
    }
}
```

### 实体规范

```csharp
public class BilibiliVideo : BaseAuditModel
{
    public string Title { get; set; } = string.Empty;
    public string Bvid { get; set; } = string.Empty;

    // 导航属性
    public ICollection<VideoTagMapping> TagMappings { get; set; } = [];
}
```

### DTO 规范

```csharp
// InputDto: 用于接收请求参数
public class VideoInputDto
{
    public string Bvid { get; set; } = string.Empty;
    public List<int> TagIds { get; set; } = [];
}

// ViewModel: 用于返回响应数据
public class VVideoInfoModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
}
```

### 日志规范

```csharp
// ✅ 使用结构化日志
_logger.LogInformation("获取视频成功，VideoId: {VideoId}", videoId);

// ❌ 字符串插值（丢失结构化信息）
_logger.LogInformation($"获取视频成功，VideoId: {videoId}");
```

## 通用规范

### 禁止硬编码

```csharp
// ❌ 错误
var connectionString = "Server=localhost;Database=test;";

// ✅ 正确：从配置读取
var connectionString = _configuration.GetConnectionString("DefaultConnection");
```

### 空值处理

```csharp
// 使用可空引用类型
public string? Description { get; set; }

// 使用 null 合并运算符
var name = user?.Name ?? "未知用户";

// 提前返回
if (video == null)
{
    return ReturnData<Video>.Fail("视频不存在");
}
```

### 异常处理

```csharp
// 在 Service 层处理异常，返回业务结果
try
{
    // 业务逻辑
}
catch (HttpRequestException ex)
{
    _logger.LogError(ex, "请求外部API失败");
    return ReturnData<T>.Fail("网络请求失败，请稍后重试");
}
```

---

*最后更新: 2026-03-03*
