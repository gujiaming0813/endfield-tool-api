# 负面约束手册

> 本文档记录项目中 AI 容易犯的错误和历史踩坑记录。
>
> **来源**: 从项目代码、注释、TODO 和实际踩坑中提取
> **维护**: 修复 Bug 后，请同步更新本文档！

---

## 项目特定约束

> 以下是从本项目代码中提取的具体陷阱

### 1. BVID 提取使用正则表达式

**来源**: `BilibiliService.cs:334`

```csharp
// 使用 GeneratedRegex 特性编译正则，性能更好
[GeneratedRegex(@"BV[a-zA-Z0-9]{10,12}", RegexOptions.IgnoreCase)]
private static partial Regex BvidRegex();

// 提取时使用
var match = BvidRegex().Match(input);
return match.Success ? match.Value : null;
```

**注意**: BVID 格式为 `BV` 开头 + 10-12 位字母数字

### 2. 标签筛选必须使用 AND（交集）逻辑

**来源**: `BilibiliService.cs:174-184`

```csharp
// ❌ 错误：使用 Contains 会导致 OR 逻辑
query = query.Where(v => v.Tags.Any(t => tagIds.Contains(t.Id)));

// ✅ 正确：使用子查询实现 AND 逻辑
var tagIds = inputDto.TagIds.Distinct().ToList();
var requiredCount = tagIds.Count;
query = query.Where(v =>
    dbContext.VideoTagMappings
        .Where(m => m.VideoId == v.Id && tagIds.Contains(m.TagId))
        .Count() == requiredCount);
```

### 3. 软删除使用 IsDeleted 标记

**来源**: `BilibiliService.cs:235`

```csharp
// ❌ 错误：物理删除
_dbContext.Videos.Remove(video);

// ✅ 正确：软删除
video.IsDeleted = true;
video.UpdatedAt = DateTime.Now;
await _dbContext.SaveChangesAsync();
```

### 4. 更新标签后需重新查询导航属性

**来源**: `BilibiliService.cs:145-149`

```csharp
// 更新标签后，导航属性可能不完整，需要重新查询
var updatedVideo = await _dbContext.BilibiliVideos
    .Include(v => v.VideoTagMappings)
    .ThenInclude(m => m.Tag)
    .FirstAsync(v => v.Id == inputDto.VideoId, token);
```

---

## C# 通用避雷

### 5. 异步方法不要同步调用

```csharp
// ❌ 错误：会导致死锁
var result = _service.GetAsync().Result;

// ✅ 正确：使用 await
var result = await _service.GetAsync();
```

### 6. 不要在控制器中写业务逻辑

```csharp
// ❌ 错误：控制器包含业务逻辑
[HttpPost]
public async Task<IActionResult> Create(VideoInputDto dto)
{
    // 业务逻辑写在 Controller 里
}

// ✅ 正确：业务逻辑下沉到 Service
[HttpPost]
public async Task<IActionResult> Create(VideoInputDto dto)
{
    var result = await _videoService.CreateAsync(dto);
    return Ok(result);
}
```

### 7. DbContext 不要注册为 Singleton

```csharp
// ❌ 错误：DbContext 不是线程安全的
builder.Services.AddSingleton<AppDbContext>();

// ✅ 正确：使用 Scoped 生命周期
builder.Services.AddDbContext<AppDbContext>(...);
```

### 8. 日志使用结构化占位符

```csharp
// ❌ 错误：丢失结构化信息
_logger.LogInformation($"用户 {userId} 登录成功");

// ✅ 正确：使用占位符
_logger.LogInformation("用户 {UserId} 登录成功", userId);
```

---

## 安全避雷

### 9. 敏感配置不要硬编码

```csharp
// ❌ 错误
var secretKey = "my-super-secret-key-12345";

// ✅ 正确：从配置读取
var secretKey = _configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey 未配置");
```

### 10. DTO 与 Entity 分离

```csharp
// ❌ 错误：直接暴露 Entity
return Ok(await _dbContext.Users.FindAsync(id));

// ✅ 正确：使用 ViewModel 映射
var user = await _dbContext.Users.FindAsync(id);
var viewModel = new VUserModel { Id = user.Id, Name = user.Name };
return Ok(ReturnData<VUserModel>.Success(viewModel));
```

---

## 踩坑记录

> 以下记录实际遇到的问题，持续更新

<!-- 暂无实际踩坑记录，请在遇到 Bug 后手动添加 -->

---

## 踩坑记录模板

修复 Bug 后，按以下格式添加到「踩坑记录」区块：

```markdown
### P-001. [问题描述]

**现象**:
[描述 Bug 表现，包括错误信息、异常行为]

**原因**:
[根本原因分析]

**解决**:
[修复方案简述]

**代码示例**:
```csharp
// ❌ 错误代码
[错误代码示例]

// ✅ 正确代码
[正确代码示例]
```

**影响范围**: [受影响的功能/模块]
**发现日期**: YYYY-MM-DD
**修复日期**: YYYY-MM-DD
```

---

*最后更新: 2026-03-03*
