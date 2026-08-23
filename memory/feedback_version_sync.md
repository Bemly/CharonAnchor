---
name: version-sync-requirement
description: BotAppInfo.Linux 版本必须与 wrapper.node 提取源版本一致
type: feedback
---

## 规则

BotAppInfo.Linux 的版本号必须与 wrapper.node 提取源 QQ 客户端的版本号一致。

**Why:** 2026-05-10 IDA 逆向分析发现，wrapper.node (3.2.28-48517) 的签名函数内部可能编码了版本信息。但协议层 BotAppInfo 声明为 3.2.26-46494，服务端检测到版本不一致后标记账号风险。存活约 1 周后被踢，重新部署无法恢复（账号已标记）。

**How to apply:**
- 更新 wrapper.node 时，同步更新 `Lagrange.Core/Common/BotAppInfo.cs` 中的 Linux AppInfo
- 三个字段：`Qua`、`CurrentVersion`、`AppClientVersion`
- 当前对齐版本：3.2.29-260528
- AppClientVersion = QQ 版本号中的 build number（如 3.2.28-48517 → 48517）
