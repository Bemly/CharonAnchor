---
name: sign-function-reverse-analysis
description: sub_57E1131 签名函数完整逆向结果 — DFLJ 检查、环境检测、版本问题
type: project
---

## sub_57E1131 签名函数完整逆向 (2026-05-10)

### 函数签名
```c
__int64 sub_57E1131(cmd, data, data_len, seq, output_buffer)
// Always returns 0
```

### 内部调用链
```
sub_57E1131
  ├── dladdr(__builtin_return_address(0), &info)  ← 每次签名都执行
  ├── strstr(info.dli_fname, needle)              ← 用 XOR 解码 "DFLJ" 后搜索
  ├── sub_57CD620 → 计算 sign (MD5)
  ├── sub_57CBF68 → 计算 extra
  └── sub_57BD1A4 → 计算 token
```

### DFLJ 反逆向检查逻辑
```c
dladdr(retaddr, &info);                    // 获取 caller 模块路径
// XOR 解码构建 needle = "DFLJ"
if (strstr(info.dli_fname, "DFLJ") == 0)  // DFLJ NOT in path
    dword_7EE72B0 = 1;                      // → 正常
else                                       // DFLJ IN path
    dword_7EE72B0 = 0;                      // → 检测到篡改
```

**注意：dword_7EE72B0 在 sign 函数内部不被消费！** 签名计算完全不受此 flag 影响。flag=0 的后果由其他函数（可能是 token refresh）触发。

### P/Invoke 调用时的行为
- 我们直接调用 sub_57E1131（绕过 sub_2DD24A0 MSFSign wrapper）
- `__builtin_return_address(0)` 返回 P/Invoke stub 中的地址
- dladdr 返回 .NET runtime 或 Lagrange.Milky 的路径
- 路径不含 "DFLJ" → flag=1 → 检查通过

### wrapper.node 环境检测（全部发现）
| 检测目标 | xrefs |
|----------|-------|
| /proc/self/exe | 5 |
| /proc/self/cmdline | 1 |
| /proc/self/maps | 1 |
| /proc/self/auxv | 1 |
| /proc/self/task/*/maps | 1 |
| /proc/mounts | 1 |
| /sys/.../tsc_freq_khz | 1 |
| getenv | 15 个调用者 |
| getauxval | 2 个调用者 |

### 根因推断
服务端通过签名中的版本信息与协议层声明对比，检测到 3.2.26（协议）≠ 3.2.28（签名模块），累积分析后标记账号。修复：BotAppInfo 版本同步到 3.2.28-48517。
