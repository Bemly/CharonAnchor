---
name: aot-linux-only
description: AOT 编译需要在 Linux 环境，macOS 缺少 llvm-objcopy
type: feedback
---

## 规则

AOT 编译（特别是 StripSymbols）需要在 Linux 环境完成。

**Why:** macOS 缺少 llvm-objcopy 工具，无法剥离符号表。AOT 完整优化需要 LLVM 工具链。

**How to apply:**
1. 开发在 macOS，普通 build/test 可以正常跑
2. 发布 AOT 构建在 GitHub Actions（ubuntu-latest）或 NAS Linux
3. 命令：`dotnet publish -c Release -r linux-x64 -p:PublishAot=true -p:StripSymbols=true`