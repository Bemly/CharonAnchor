---
name: project-status
description: CharonAnchor 当前状态 - Fork LagrangeV2 + 本地签名嵌入
type: project
---

## 当前架构

**CharonAnchor = LagrangeV2 fork + CharonSignProvider 本地签名**

### 分支结构

| 分支 | 说明 |
|------|------|
| `Lagrange` | 主分支，本地签名嵌入（3.2.32 升级在此完成） |
| `LagrangeV2` | 2026-08-22 从 Lagrange 分出，已合并 upstream/Lagrange.Core master（46 commits），Milky 重构为新 Extensions 结构 |
| `3.2.19-39038` | origin/HEAD，Lagrange 的 base（封存） |
| `3.2.28-48517` | 新版签名偏移，独立工作流构建（封存） |
| `3.2.29-260528` | push 触发 workflow 已改仅手动 |

上游 remote：`upstream = https://github.com/LagrangeDev/Lagrange.Core.git`（原 LagrangeV2 已改指）

### 核心实现

```
Charon/
├── CharonSignProvider.cs   # 继承 BotSignProvider，调用 wrapper.node
├── WrapperLoader.cs        # 加载 wrapper.node（DFLJ 是 canary，不创建 symlink）
└── WrapperInterop.cs       # P/Invoke: dlopen, dlsym, memfd_create
```

---

## 签名函数偏移（逆向分析结果仍有效）

| 版本 | 偏移 | 说明 |
|------|------|------|
| 3.2.19-39038 | `0x5ADE220` | 老版本 |
| 3.2.28-48517 | `0x57E1131` | 旧版 |
| 3.2.29-260528 | `0x5BD3EA1` | 上一版（workflow 改仅手动） |
| 3.2.32-260812 | `0x65E55D1` | 当前使用（2026-08-22 逆向验证：1405 字节，dladdr+DFLJ canary 逻辑不变，全局 flag 改为 dword_8EB4160） |

---

## 版本对齐

| 字段 | 值 |
|------|------|
| `CurrentVersion` | `3.2.32-260812` |
| `QUA` | `V1_LNX_NQ_3.2.32_260812_GW_B` |
| `AppClientVersion` | `260812` |

**Why:** 服务端检测到协议层版本 (3.2.26) 与 wrapper.node 签名编码版本 (3.2.28) 不一致，累积分析后标记账号风险。存活 ~1 周后被踢。

## AOT 构建优化

**配置（csproj）：**
```xml
<PublishAot>true</PublishAot>
<InvariantGlobalization>true</InvariantGlobalization>
```

**优化参数（workflow）：**
```
-p:DebugType=none -p:StripSymbols=true -p:OptimizationPreference=Size
```

**预期体积：** ~20MB 单文件

---

## 依赖库

| 库 | 来源 | 说明 |
|---|------|-----|
| wrapper.node | QQ deb | 签名模块 |
| libsymbols.so | 自己编译 | qq_magic_napi_register 空符号 |
| libbugly.so | QQ deb | Bugly 库 |
| libcrbase.so | QQ deb | 基础运行库 |

---

## 当前阻塞（2026-08-22）

**QQ 服务端静默丢弃 `wtlogin.trans_emp`（QR 登录获取请求）**，导致无法出码登录：

- 探针实证：GetSecSign 正常返回 → 611 字节帧正常发出 → 服务器永不回包；心跳正常收发
- 新旧代码均复现：3.2.29 旧镜像（5 月能登录的同一镜像）今天同样卡死
- 签名输出 sign=32B、token=0、extra=0，新旧 wrapper.node 行为一致
- **结论：非迁移引入的问题**，疑似服务端策略变更或账号/IP 风控（当日多次换身份重试可能加重）
- 缓解方向：冷却 24~48h 重试 / 换出口 IP 验证 / 密码登录路径绕行（wtlogin.login 不经过 trans_emp）
- ⚠️ CI 的 "Verify Docker image" 步骤是**误报**：grep 匹配到了日志文本 "use QRCode Login"，不代表真的出码

## 待办

- [ ] trans_emp 阻塞解除后完成 NAS 上 3.2.32 部署（配置需迁移为 appsettings.json 新结构）
- [ ] NAS 测试 AOT 构建验证体积