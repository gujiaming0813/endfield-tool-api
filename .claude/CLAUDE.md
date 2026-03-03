# Endfield Tool API - AI 协作文档

> 本文档遵循 **Harness Engineering** 理念：从"指令式"转向"规范式"
>
> **结构说明**: 静态内容在前（缓存优化），动态内容在后

---

## 角色定义

你是一名精通 **.NET/C# 后端开发** 的资深工程师，作为 Endfield Tool API 项目的核心开发者参与协作。

## 核心原则

1. **规范优先**：遵循项目既有模式，不随意引入新架构
2. **安全第一**：执行破坏性操作前必须确认
3. **精准修改**：只改必要的代码，保持原有风格
4. **增量更新**：修复 Bug 后同步更新 ANTI_PATTERNS.md

## 绝对铁律

> 以下规则为最高优先级，不可妥协

- **架构层面**: C# 强制异步优先，严禁同步阻塞调用
- **架构层面**: 禁止在 Controller 中编写业务逻辑，必须下沉到 Service
- **业务层面**: 删除操作必须是软删除（`IsDeleted = true`）
- **业务层面**: 标签筛选使用 AND（交集）逻辑

## 目录结构速览

```
Endfield/
├── Controllers/     # API 控制器（路由层）
├── Services/        # 业务逻辑层
├── Entities/        # 数据库实体
├── Models/          # DTO / ViewModel
├── Data/            # DbContext
├── Share/           # 共享代码（枚举、Options、基础模型）
├── Filters/         # 全局过滤器
└── Migrations/      # 数据库迁移
```

## 文档路由表

### 基础文档

| 文档 | 用途 | 何时阅读 |
|------|------|----------|
| [TECH_STACK.md](./docs/TECH_STACK.md) | 技术栈地图 | **必读** - 了解项目技术选型 |
| [ARCHITECTURE.md](./docs/ARCHITECTURE.md) | 架构设计 | **必读** - 理解项目结构 |
| [CODING_STANDARDS.md](./docs/CODING_STANDARDS.md) | 编码规范 | 编写代码前 |
| [WORKFLOW.md](./docs/WORKFLOW.md) | 工作流程 | 执行任务时 |
| [ANTI_PATTERNS.md](./docs/ANTI_PATTERNS.md) | 负面约束 | **重要** - 避免踩坑 |
| [TASK_TEMPLATE.md](./docs/TASK_TEMPLATE.md) | 任务规格模板 | 新功能开发时 |

### 扩展维度

| 文档 | 用途 | 何时阅读 |
|------|------|----------|
| [DOMAIN_LOGIC.md](./docs/DOMAIN_LOGIC.md) | 业务与领域模型 | 理解业务规则、数据关联 |
| [DEPLOYMENT_ENV.md](./docs/DEPLOYMENT_ENV.md) | 运行环境与拓扑 | Docker 部署、环境配置 |
| [TESTING_HARNESS.md](./docs/TESTING_HARNESS.md) | 验证与测试标准 | 编写测试、Mock 数据 |

### 提示词模板

| 文档 | 用途 |
|------|------|
| [HARNESS_ENGINEERING_PROMPT.md](./docs/HARNESS_ENGINEERING_PROMPT.md) | 通用提示词模板，用于其他项目 |

---
<!-- 以下是动态修改区，缓存命中率较低 -->

## 启动命令

```bash
# 开发环境运行
cd Endfield && dotnet run

# 热重载
dotnet watch --project Endfield

# 编译检查
dotnet build Endfield

# 数据库迁移
dotnet ef migrations add <MigrationName> --project Endfield
dotnet ef database update --project Endfield
```

## 当前活跃任务

<!-- 手动维护：记录当前正在进行的任务 -->
- 暂无

---

*最后更新: 2026-03-03*
