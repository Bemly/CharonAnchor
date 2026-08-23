---
name: charonanchor-implementation
description: CharonAnchor 完整实现过程和逆向逻辑 - 包括新老版本分析方法
type: project
---

## 项目目标

为 Lagrange.Milky 提供本地签名服务，替代官方签名服务器。

---

## 当前方案：P/Invoke 嵌入 wrapper.node

```
Lagrange.Milky
    └── CharonSignProvider (继承 BotSignProvider)
            ├── WrapperLoader.Load() → dlopen wrapper.node
            ├── 创建 DFLJ symlink 绕过反逆向
            ├── dlsym 获取签名函数
            └── 调用签名函数返回结果
```

### 核心代码

**CharonSignProvider.cs：**
```csharp
public override ValueTask<SignResponse> Sign(SignInfo info)
{
    int result = WrapperLoader.Sign(
        info.Command,      // module_id
        info.Body,         // data
        info.Sequence,     // seq
        out var output     // 768 bytes
    );
    // 解析 output: token(0), extra(256), sign(512)
}
```

**WrapperLoader.cs：**
```csharp
// 创建 DFLJ symlink 绕过反逆向
var symlink = "/tmp/DFLJ/wrapper.node";
File.CreateSymbolicLink(symlink, wrapperPath);

// dlopen 加载
_handle = dlopen(symlink, RTLD_NOW);

// dlsym 获取签名函数（使用偏移）
_signFunc = dlsym(_handle, SIGN_OFFSET);
```

---

## 核心逆向逻辑

### 1. 签名模块来源

wrapper.node 来自 LinuxQQ deb 包：
```
deb → 解压 → opt/QQ/resources/app/wrapper.node
```

### 2. 签名函数定位

#### 老版本方法 (3.2.19)

1. 搜索字符串 "token"/"sign"/"extra"
2. 找格式化输出函数 "token_hex:%s extra_hex:%s sign:%s"
3. 向上追踪调用者找到签名入口
4. 直接使用偏移调用

#### 新版本方法 (3.2.28+)

**⚠️ 不要用 secSign！那是业务函数！**

正确的追踪路径：
```
1. 搜索 "MSFSign" 字符串 (0x992042)
2. 找到 NAPI 方法引用 → sub_2DD24A0
3. 反编译 sub_2DD24A0，找核心调用
4. 发现 sub_57E1131 是真正的签名函数
5. 分析 sub_57E1131 的参数和输出
```

关键区别：
- secSign → sub_30208A0 → **业务处理函数**
- MSFSign → sub_2DD24A0 → sub_57E1131 → **签名计算函数**

### 3. 函数签名

**老版本 (偏移 0x5ADE220)：**
```c
func(cmd, src, len, seq, out)
```

**新版本 (偏移 0x57E1131)：**
```c
int sub_57E1131(module_id, data, data_len, seq, output)
// module_id: 命令名字符串
// data: 数据指针
// data_len: 数据长度
// seq: 序列号
// output: 输出缓冲区 768 bytes
// 返回: 0 成功
```

### 4. 输出缓冲区布局 (新老版本相同)

```
offset 0:   token, 镀度在 offset 255
offset 256: extra, 长度在 offset 511
offset 512: sign,  长度在 offset 767
```

### 5. 反逆向检查 (新版本)

新版本使用 dladdr 检查加载路径——"DFLJ" 是 canary（陷阱标记）：

```c
dladdr(return_addr, &info);
strstr(info.dli_fname, "DFLJ");
// 找到 DFLJ → flag=0 (检测到篡改) → 5小时后下线
// 没找到 DFLJ → flag=1 (正常)
```

**绕过方法：直接加载 wrapper.node，不创建任何 symlink。**

### 6. 依赖库需求

wrapper.node 需要以下库才能加载：

| 库 | 来源 | 说明 |
|---|------|-----|
| libgnutls.so.30 | Linux 系统自带 | 加密库 |
| libsymbols.so | 自己编译 | 提供 qq_magic_napi_register 符号 |
| libbugly.so | QQ deb | Bugly 崩溃上报库 |
| libcrbase.so | QQ deb | 基础运行库 |

### 7. 关键符号

```c
// symbols.c - 只需这一个空函数
void qq_magic_napi_register(void* arg) { }
```

这是 wrapper.node 作为 Node.js native module 需要的初始化符号。

---

## 适配新版本步骤

1. 下载新版本 Lagrange.Milky 或 LinuxQQ deb
2. 解压提取 wrapper.node
3. 用 IDA 分析找新签名函数偏移：
   - **搜索 "MSFSign"** (不是 secSign)
   - 追踪到类似 sub_57E1131 的核心函数
   - 确认输出结构是 offset 0/256/512
4. 检查反逆向：
   - 搜索 dladdr 调用
   - 搜索路径检查字符串
5. 更新偏移值
6. **不要创建 DFLJ symlink**，直接加载即可（DFLJ 是 canary，strstr 匹配到反而触发检测）
7. 测试验证

---

## ⚠️ 重要提醒

1. **secSign JavaScript 方法不是签名函数**
   - 它是业务处理函数 (addBuddy 等)
   - 签名只是它的输入参数

2. **MSFSign 才是签名入口**
   - 但追踪它找到核心函数更简单

3. **新版本架构**
   - NAPI 包装层 → 业务逻辑层 → 核心签名函数
   - 直接调用核心函数最简单