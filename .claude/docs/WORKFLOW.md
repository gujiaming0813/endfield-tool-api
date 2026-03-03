# 工作流程

> 本文档定义 AI 协作的工作流程和任务管理模式。

## 全局配置

| 配置项 | 值 |
|--------|-----|
| 语言 | 始终使用简体中文 |
| 操作系统 | macOS |
| 包管理器 | dotnet (NuGet) |

## Agent 授权指令表

> 明确界定 AI 可自主执行的命令与必须询问人类的命令

### 自主执行白名单（无需确认）

| 类别 | 命令 | 说明 |
|------|------|------|
| **编译构建** | `dotnet build` | 编译检查 |
| | `dotnet clean` | 清理构建产物 |
| | `dotnet restore` | 还原依赖 |
| **代码检查** | `dotnet format --verify-no-changes` | 格式检查 |
| **运行测试** | `dotnet test` | 执行测试 |
| **只读查询** | `git status`, `git log`, `git diff` | Git 只读操作 |
| | `git branch -a` | 查看分支 |
| **文件搜索** | Glob, Grep 工具 | 搜索文件和内容 |
| **Docker 查询** | `docker ps`, `docker logs` | 查看容器状态 |
| | `docker-compose ps` | 查看服务状态 |

### 需人类确认的高危命令

| 类别 | 命令 | 风险等级 | 原因 |
|------|------|:--------:|------|
| **数据删除** | `rm -rf` | 🔴 高 | 不可恢复 |
| | `dotnet ef database drop` | 🔴 高 | 删除数据库 |
| **数据库变更** | `dotnet ef migrations add` | 🟡 中 | 创建迁移文件 |
| | `dotnet ef database update` | 🟡 中 | 修改数据库结构 |
| | `dotnet ef migrations remove` | 🟡 中 | 删除迁移 |
| **Git 修改** | `git commit` | 🟡 中 | 提交代码 |
| | `git push` | 🔴 高 | 推送到远程 |
| | `git merge`, `git rebase` | 🔴 高 | 分支操作 |
| | `git reset --hard` | 🔴 高 | 丢弃更改 |
| **Docker 操作** | `docker-compose down` | 🟡 中 | 停止并移除容器 |
| | `docker-compose down -v` | 🔴 高 | 同时删除数据卷 |
| | `docker system prune` | 🔴 高 | 清理未使用资源 |
| **环境配置** | 修改 `.env` 文件 | 🟡 中 | 环境变量变更 |
| | 修改 `appsettings.json` | 🟡 中 | 配置变更 |

### 权限说明摘要

```
🟢 自主执行: 编译、测试、只读查询、文件搜索
🟡 需确认: 数据库迁移、Git commit、配置修改
🔴 禁止自主: 数据删除、Git push、强制重置
```

## 文件操作规范

**核心原则**: 使用专用工具，禁止 Shell 命令操作文件

| 操作 | 必须使用 | 严格禁止 |
|------|----------|----------|
| 创建文件 | **Write 工具** | `touch`、`echo >`、`cat <<EOF` |
| 编辑文件 | **Edit 工具** | `sed`、`awk` |
| 读取文件 | **Read 工具** | `cat`、`head`、`tail` |
| 搜索文件 | **Glob 工具** | `find`、`ls` |
| 搜索内容 | **Grep 工具** | `grep`、`rg` |

**Bash 工具仅用于**: 包管理、版本控制（读取）、编译等系统命令。

## 任务处理流程

### 1. 接收任务

```
用户描述需求 → 分析复杂度 → 决定是否使用 TodoWrite
```

### 2. 复杂任务处理

对于复杂任务，按以下顺序使用专用代理：

| 场景 | 使用代理 |
|------|----------|
| 模糊需求分析 | `requirements-analyst` |
| 架构设计 | `senior-code-architect` |
| 测试编写 | `vitest-tester` |
| 代码审查 | `code-reviewer` |

### 3. 代码修改原则

1. **精准修改**: 只修改必要的部分，不随意重构无关代码
2. **保持风格**: 遵循原有缩进和格式
3. **增量更新**: 修复 Bug 后更新 ANTI_PATTERNS.md

## 常用命令速查

### 开发调试

```bash
# 运行项目
cd Endfield && dotnet run

# 热重载
dotnet watch --project Endfield

# 编译检查
dotnet build Endfield
```

### 数据库操作

```bash
# 创建迁移
dotnet ef migrations add <MigrationName> --project Endfield

# 应用迁移
dotnet ef database update --project Endfield

# 撤销迁移
dotnet ef migrations remove --project Endfield
```

### 依赖管理

```bash
# 添加包
dotnet add Endfield/Endfield.Api.csproj package <PackageName>

# 还原依赖
dotnet restore
```

## 问题排查流程

1. **读取错误日志**: 查看 `Endfield/logs/` 目录下的日志文件
2. **定位问题文件**: 使用 Grep 搜索错误信息
3. **分析根因**: 阅读相关代码，理解上下文
4. **修复问题**: 使用 Edit 工具精准修改
5. **验证修复**: 运行 `dotnet build` 确认
6. **更新文档**: 将踩坑记录写入 ANTI_PATTERNS.md

---

*最后更新: 2026-03-03*
