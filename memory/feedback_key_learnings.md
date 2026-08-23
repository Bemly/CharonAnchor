---
name: key-learnings
description: P/Invoke 嵌入方案的关键经验和踩坑
type: feedback
---

## 关键经验

### 1. wrapper.node 需要空符号初始化

**问题：** dlopen 失败，找不到 `qq_magic_napi_register`
**原因：** wrapper.node 是 Node.js native module，需要该符号
**解决：** libsymbols.so 提供空函数即可，不需要完整 Node.js 环境

### 2. DFLJ symlink 不需要创建（之前理解反了）

**问题：** 签名返回空结果 / 5小时后强制下线
**原因：** wrapper.node 的 "DFLJ" 是 canary（陷阱标记）。strstr 找到 DFLJ → flag=0（检测到篡改），找不到 → flag=1（正常）
**解决：** 直接加载 wrapper.node，不创建 DFLJ symlink

### 3. NuGet 与 submodule 不兼容

**问题：** Lagrange.Core NuGet 版本不对
**原因：** NuGet 是 0.3.1，submodule 是 2.0.14-beta
**解决：** 必须使用 submodule，不能用 NuGet 包

### 4. 依赖库要放同目录

**问题：** wrapper.node 找不到 libbugly/libcrbase
**解决：** 所有库文件放同一目录，或设置 LD_LIBRARY_PATH

### 5. AOT 需要 Linux 环境

**问题：** macOS 编译缺少 llvm-objcopy
**原因：** AOT StripSymbols 需要 LLVM 工具链
**解决：** 在 Linux 或 GitHub Actions 构建

### 7. V1/V2 签名函数只有返回值类型不同，但用错会导致随机被踢

**问题：** V2 (3.2.28+) 用 V1 delegate（返回 long），函数实际返回 int
**原因：** x86-64 上 int 只设 eax（低 32 位），rax 高 32 位是随机垃圾。高 32 位恰好 0 → 正常；不是 0 → 误判"签名失败"→ QQ 踢下线
**解决：** V2 用 `int` 返回的 delegate，V1 用 `long`。版本判断直接用 `version != "3.2.19"`，不要用偏移大小比较（新版偏移 `0x57E1131` < `0x50000000` 判错）

### 8. Docker 容器用 bridge 网络，不是 host

**正确命令：**
```bash
sudo docker stop lagrange && sudo docker rm lagrange
sudo docker pull ghcr.io/bemly/charonanchor:3.2.29
sudo docker run -d --name lagrange \
    -v /vol1/1000/Lagrange:/root \
    --network bridge \
    --add-host=host.docker.internal:host-gateway \
    -p 616:616 \
    ghcr.io/bemly/charonanchor:3.2.29
```
**Why:** bridge + add-host 让容器通过 `host.docker.internal` 访问宿主，不需要 host 网络模式。

### 6. Lagrange 容器用 host 网络（旧，已废弃）

**问题：** 容器内访问签名服务复杂
**解决：** `--network host` 共享宿主机网络，直接访问 127.0.0.1

---

## 逆向分析要点

### 如何找签名函数

1. 搜索 "MSFSign" 字符串
2. 追踪到 sub_57E1131（核心函数）
3. **不要用 secSign**，那是业务函数！

### 如何判断反逆向

搜索 `dladdr` 调用和路径检查字符串（如 "DFLJ"）