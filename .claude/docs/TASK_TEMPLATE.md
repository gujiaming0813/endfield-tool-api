# 任务规格书模板

> 本模板用于定义新功能开发的结构化需求，遵循 Harness Engineering 理念。

---

## 使用说明

当需要开发新功能时，创建一个临时的 `.md` 文件，按照以下模板填写，然后让 AI 阅读。

---

## 模板

```markdown
# 任务规格书: [功能名称]

## Goal (目标)
<!-- 用一句话描述要实现什么 -->
[清晰的目标描述]

## Context (上下文)
<!-- 描述功能所在的业务背景 -->
- 后端技术: C#/.NET 8
- 相关模块: [涉及的模块，如: Auth, Video, Tag]
- 依赖服务: [外部服务或API]

## Constraints (约束)
<!-- 列出必须遵守的规则 -->
- [ ] 必须在 [目录路径] 下新建文件
- [ ] 必须遵循 [已有模式/接口]
- [ ] 配置必须从 appsettings.json 读取，禁止硬编码
- [ ] 必须编写单元测试（可选）
- [ ] [其他约束...]

## Technical Design (技术设计)
<!-- 可选：描述实现思路或架构图 -->

### API 设计
| 方法 | 路径 | 描述 |
|------|------|------|
| GET | /api/xxx | 获取列表 |
| POST | /api/xxx | 创建 |

### 数据模型
<!-- 如果需要新增实体，描述字段 -->

## Definition of Done (完成标准)
<!-- 明确的验收条件 -->
1. [ ] API 接口可正常调用
2. [ ] 返回数据符合 ReturnData<T> 格式
3. [ ] 日志记录完整
4. [ ] [其他验收条件...]

## References (参考)
<!-- 相关文档或代码链接 -->
- [相关代码文件路径]
- [API 文档链接]
```

---

## 示例: 添加推送服务

```markdown
# 任务规格书: 推送服务集成

## Goal
集成小米/OPPO/VIVO 厂商的推送通道，实现消息推送功能。

## Context
- 后端技术: C#/.NET 8
- 相关模块: Notification
- 依赖服务: 小米/OPPO/VIVO 推送 API

## Constraints
- [ ] 必须在 `Services/Push/` 目录下新建 Provider
- [ ] 必须实现 `IPushProvider` 接口
- [ ] 配置从 `appsettings.json` 的 `Push` 节点读取
- [ ] 推送失败必须有重试机制（最多 3 次）
- [ ] 禁止硬编码 API 密钥

## Technical Design

### API 设计
| 方法 | 路径 | 描述 |
|------|------|------|
| POST | /api/push/send | 发送单条推送 |
| POST | /api/push/batch | 批量推送 |

### 架构
```
PushController
    └── IPushService
            ├── XiaomiPushProvider
            ├── OppoPushProvider
            └── VivoPushProvider
```

## Definition of Done
1. [ ] 三家厂商推送 API 对接完成
2. [ ] 推送结果记录到数据库
3. [ ] Swagger 文档更新
4. [ ] 错误处理和日志完善

## References
- `Services/IAuthService.cs` (参考服务模式)
- https://dev.mi.com/console/doc/push (小米推送文档)
```

---

## 快速使用

1. **复制模板** 到临时文件（如 `task_push.md`）
2. **填写内容** 根据实际需求
3. **引导 AI** 告诉它：`请阅读 task_push.md 并实现该功能`

---

*最后更新: 2026-03-03*
