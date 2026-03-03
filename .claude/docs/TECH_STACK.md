# 技术栈地图

> 本文档定义项目的技术选型和环境配置，是 AI 理解项目的"入职手册"。

## 运行环境

| 项目 | 版本/配置 |
|------|-----------|
| 运行时 | .NET 8 |
| 操作系统 | macOS (开发) / Docker (部署) |
| 数据库 | MySQL 8.0 |
| IDE | Rider / VS Code |

## 核心依赖

### 后端框架
```
ASP.NET Core 8.0      # Web API 框架
Entity Framework Core # ORM
Pomelo.EntityFrameworkCore.MySql  # MySQL 提供程序
```

### 安全与认证
```
JWT Bearer Authentication  # Token 认证
Microsoft.IdentityModel.Tokens
```

### 日志与监控
```
Serilog                    # 结构化日志
Serilog.Sinks.Console
Serilog.Sinks.File
```

### API 文档
```
Swashbuckle.AspNetCore  # Swagger/OpenAPI
```

## 配置文件

| 文件 | 用途 |
|------|------|
| `appsettings.json` | 主配置（日志、JWT、数据库连接） |
| `appsettings.Development.json` | 开发环境覆盖配置 |

### 关键配置项

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=...;"
  },
  "Jwt": {
    "SecretKey": "...",
    "Issuer": "...",
    "Audience": "...",
    "ExpirationMinutes": 1440
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "Console": { "Enabled": true },
    "File": { "Enabled": true, "Path": "logs/log-.json" }
  }
}
```

## 环境变量

| 变量名 | 用途 |
|--------|------|
| `ASPNETCORE_ENVIRONMENT` | 运行环境 (Development/Production) |

## 外部服务

| 服务 | 用途 | 备注 |
|------|------|------|
| Bilibili API | 视频/标签数据获取 | 需要网络访问 |

---

*最后更新: 2026-03-03*
