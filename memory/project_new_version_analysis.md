---
name: new-version-analysis
description: 3.2.28-48517 版本 wrapper.node 逆向分析完整结果 - 签名函数调用方式
type: project
---

## 新版本签名架构分析 (3.2.28-48517)

**Why:** 老版本直接调用固定偏移函数，新版本使用 NAPI 回调架构，但核心签名函数仍然可直接调用。

**How to apply:** 使用 `sub_57E1131` 作为签名入口，参数和老版本类似，输出结构完全一样。

---

## 🎯 核心签名函数

### sub_5BD3EA1 @ 0x5BD3EA1 (3.2.29-260528)

**真正的签名计算函数，不依赖 NAPI，可直接调用！**

(3.2.28 版本为 sub_57E1131 @ 0x57E1131)

```c
// 函数签名
int sub_57E1131(
    const char* module_id,  // 命令名 (如 "foo")
    const void* data,       // 数据指针
    int data_len,           // 数据长度
    int seq,                // 序列号
    unsigned char* output   // 输出缓冲区 (768 bytes)
);

// 返回值: 0 = 成功, 非0 = 失败
```

**输出结构（和老版本完全一样）：**
```
offset 0:   token (std::string), 长度在 offset 255
offset 256: extra (std::string), 长度在 offset 511
offset 512: sign  (std::string), 长度在 offset 767
```

---

## 📞 调用链

```
JavaScript secSign
    → sub_301EB50 (NAPI 包装器, 处理参数)
        → sub_30208A0 (业务逻辑, 处理 addBuddy 等功能)
            → ⚠️ secSign 在这里是输入参数，不是计算！

JavaScript MSFSign
    → sub_2DD24A0 (真正的签名入口)
        → sub_57E1131 (核心签名计算) ← 这是我们要调用的！
            → sub_57CD620 (计算 sign 部分, MD5)
            → sub_57CBF68 (计算 extra 部分)
            → sub_57BD1A4 (计算 token 部分)
```

---

## ⚠️ 反逆向检查

### dladdr 路径检查

wrapper.node 使用 `dladdr` 检查加载路径是否包含 `"DFLJ"`：

```c
dladdr(return_address, &info);
strstr(info.dli_fname, "DFLJ");  // "DFLJ" 是 canary（陷阱标记）
```

⚠️ **重要更正 (2026-05-01)：之前分析方向反了！**

- `strstr` 返回非 NULL（路径**包含** DFLJ）→ `dword_7EE72B0 = 0` → **检测到篡改**
- `strstr` 返回 NULL（路径**不含** DFLJ）→ `dword_7EE72B0 = 1` → **正常**

**正确做法：不要创建 DFLJ symlink，直接加载 wrapper.node。**
flag=0 会导致 5 小时后 token 刷新失败、强制下线。
```

**绕过方法：创建符号链接**

```bash
mkdir -p /tmp/DFLJ
ln -s /path/to/wrapper.node /tmp/DFLJ/wrapper.node
# 然后加载 /tmp/DFLJ/wrapper.node
```

### 全局标志

```c
dword_7EE72B0 = 1;  // 检查通过后设置
```

---

## 🔧 关键函数地址

| 函数 | 偏移 | 说明 |
|------|------|------|
| **sub_5BD3EA1** | 0x5BD3EA1 | 🔴 **核心签名函数 (3.2.29)，直接调用这个** |
| sub_57E1131 | 0x57E1131 | 旧版签名函数 (3.2.28) |
| sub_2DD24A0 | 0x2DD24A0 | MSFSign NAPI 入口 |
| sub_30208A0 | 0x30208A0 | secSign NAPI 入口 (业务函数，不是签名) |
| sub_301EB50 | 0x301EB50 | secSign 上层调用者 |
| sub_57CD620 | 0x57CD620 | 计算 sign (MD5) |
| sub_57CBF68 | 0x57CBF68 | 计算 extra |
| sub_57BD1A4 | 0x57BD1A4 | 计算 token |
| sub_57D794A | 0x57D794A | 初始化函数，获取全局上下文 |

---

## 📁 JavaScript 导出方法

| 方法名 | 字符串地址 | 函数地址 |
|--------|------------|----------|
| `secSign` | 0x9A8C2E | 0x30208A0, 0x3026F40 |
| `MSFSign` | 0x992042 | 0x2DD24A0 |
| `setToken` | 0x9BD7DF | 0x318DBD0 |

⚠️ 注意：`secSign` 导出的函数是业务处理函数，不是签名计算！

---

## 🧩 算法分析

### sub_57CD620 (计算 sign)

- 使用标准 MD5 算法
- MD5 常量：`xmmword_A02490` (0x67452301, 0xefcdab89, ...)
- 标准 MD5 轮常量：-680876936 (0xD76AA478), -389564586 (0xE8C7B756)...

### 需要初始化

```c
void* ctx = sub_57D794A();  // 获取全局上下文
if (!*(ctx + 1)) {          // 检查是否已初始化
    // 进行初始化...
}
```

---

## 📊 新旧版本对比

| 项目 | 老版本 (3.2.19) | 中版 (3.2.28) | 新版本 (3.2.29) |
|------|-----------------|-----------------|-----------------|
| 偏移 | 0x5ADE220 | 0x57E1131 | **0x5BD3EA1** |
| 参数 | (cmd, src, len, seq, out) | (module_id, data, len, seq, out) |
| 输出结构 | offset 0/256/512 | ✅ **完全一样** |
| 反逆向 | 无 | ✅ dladdr 检查 (DFLJ 是 canary，不要创建 symlink) |
| 初始化 | 无 | ✅ 需要 sub_57D794A |

---

## 📝 适配步骤

1. 提取新版本 wrapper.node
2. **不要创建 DFLJ symlink**，直接加载 wrapper.node 即可（DFLJ 是 canary，找到反而触发检测）
3. 编译时指定 `-DSIGN_OFFSET=0x57E1131`
4. 其他依赖库不变：libsymbols.so, libbugly.so, libcrbase.so

---

## ⚠️ 注意事项

1. **secSign 不是签名函数** - 它是业务函数，签名只是输入参数
2. **MSFSign 才是真正的签名入口** - 但内部调用 sub_57E1131
3. **直接调用 sub_57E1131 最简单** - 不需要经过 NAPI 层
4. **必须绕过 dladdr 检查** - 否则签名结果会是空字符串

---

## 🔍 secSign (sub_30208A0) 内部分析

**结论：secSign 内部没有异常环境检测**

sub_30208A0 是纯粹的参数解析 + 业务转发函数：

```
流程：
1. napi_get_named_property 获取参数（targetInfo, sig, secSign, token, verify 等）
2. 参数类型转换和内存拷贝
3. 调用 sub_3023820 处理业务逻辑
4. 内存清理
```

**检测项检查：**

| 检测类型 | 是否存在 |
|---------|---------|
| ptrace 反调试 | ❌ 无 |
| dladdr 路径检查 | ❌ 无 |
| strstr 字符串检查 | ❌ 无 |
| 环境变量检查 | ❌ 无 |

**环境检测位置：**
- 在签名计算链路（sub_57E1131）中
- dladdr 检查路径含 "DFLJ"
- 业务层（secSign）无检测