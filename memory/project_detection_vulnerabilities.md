---
name: detection-vulnerabilities
description: 协议层检测漏洞完整分析 — ApkSignatureMd5、SdkBuildTime、Qimei、DeviceName、Tlv52D、keystore 持久化
type: project
---

## 检测漏洞分析 (2026-05-14)

**Why:** 账号存活 ~1 周后被封，提示"设备风险提醒"，重新部署无效。服务端多维度累积分析后标记账号。

**How to apply:** 每次改协议层时参考此分析，避免引入新的检测指纹。封号后必须删除 keystore 换新身份。

---

## P0 - 致命问题

### 1. Keystore 持久化 → 重新部署无效

**位置：** `BotKeystore.cs:28-41` → keystore 文件存 Docker 挂载卷
**原因：** Guid (16字节随机)、AndroidId 在 keystore 中持久化。服务器封的是这个身份，不是 IP/容器。
**解决：** 删除 NAS `/vol1/1000/Lagrange/{uin}.keystore`，换全新身份。

### 2. ApkSignatureMd5 = "com.tencent.qq" 字符串（非 MD5）

**位置：** `BotAppInfo.cs:43`
```csharp
ApkSignatureMd5 = "com.tencent.qq"u8.ToArray(),  // Linux - 这是UTF-8文本!
```
对比 Android 用的是真正 16 字节 MD5：`[0xA6, 0xB7, 0x45, 0xBF, ...]`
通过 Tlv147 在每个登录包发送。服务器一眼识破。

### 3. SdkBuildTime = 0

**位置：** `BotAppInfo.cs:46`
Linux/Mac/Windows 全部发 0，Android 发真实时间戳（1740483688 等）。明显异常。

---

## P1 - 重要问题

### 4. wrapper.node 15 个环境检测点

逆向 sub_57E1131 链路中发现（不仅 DFLJ 检查）：
| 检测目标 | xrefs | Docker 环境表现 |
|----------|-------|---------------|
| getenv | 15 | 环境变量与真实桌面完全不同 |
| /proc/self/exe | 5 | 指向 dotnet，不是 QQ |
| /proc/self/maps | 1 | Docker overlay 内存布局 |
| /proc/mounts | 1 | overlayfs 挂载点 |
| /proc/self/cmdline | 1 | dotnet Lagrange.Milky |
| /proc/self/auxv | 1 | 容器 auxv 异常 |
| /sys/.../tsc_freq_khz | 1 | 容器内可能不存在 |

DFLJ 已通过（不创建 symlink），但这些可能设置其他标志位，累积触发。

### 5. Qimei 为空字符串

**位置：** `BotKeystore.cs:25`
每个 SSO 包的 SsoReserveFields 和 Tlv545 都发送空 Qimei。

### 6. DeviceName 模式可聚类

默认 `Lagrange-XXXXXX` 或 CI 构建 `CT-{hex}`，都可识别。且在 `WtExchangeLogic.cs:304` 以未加密 HTTP 明文发送到 oidb.tim.qq.com。

---

## P2 - 次要问题

### 7. Tlv52D 发送硬编码 Android 数据

**位置：** `Tlv.cs:617-635`
```csharp
BootId = "unknown",
ProcVersion = "Linux version 4.19.157-perf-... (Android NDK...)",
Fingerprint = "Redmi/alioth/alioth:13/..."
CodeName = "REL",
```
虽然 Linux 登录路径 (BuildOicq09 → Tlv144) 不包含 Tlv52D（那是 Android 的 Tlv144Report），但任何使用此 TLV 的地方都暴露硬编码的小米手机指纹。

### 8. SdkVersion = "nt.wtlogin.0.0.1"

Linux 用 `nt.wtlogin.0.0.1`，Android 用 `6.0.0.2568`。版本号可能被校验。

---

## 生产环境修复记录

| 日期 | 操作 | 说明 |
|------|------|------|
| 2026-05-14 | 删除 keystore + DeviceName → deepin | 换全新身份重新登录 |

## 项目文件已修复（历史）

| 修复 | Commit | 说明 |
|------|--------|------|
| BotAppInfo 版本同步 3.2.28 | 93d73e2 | 协议层版本与 wrapper.node 一致 |
| V1/V2 sign delegate 修复 | 5f933ed | int vs long 返回导致随机被踢 |
