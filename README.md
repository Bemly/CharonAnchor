<div align="center">

# CharonAnchor

**Fork of [LagrangeV2](https://github.com/LagrangeDev/LagrangeV2)**

失败的作品·卡戎 - 拉格兰的现实锚定点

</div>

---

## 修改说明

本仓库是 LagrangeV2 的 fork，内置本地签名，不再需要外部签名服务器。

**改动：**
- 添加 `Charon/` - 本地签名实现
- 修改 `Lagrange.Milky` - 使用本地签名

---

## 创造

```bash
dotnet build Lagrange.Milky/Lagrange.Milky.csproj
```

---

## 锚定

将以下文件放在运行目录：
- `wrapper.node` - 签名模块（从 Lagrange.Milky 依赖提取）
- `libsymbols.so` - 符号补丁
- `libbugly.so` - 依赖库
- `libcrbase.so` - 依赖库

运行：
```bash
cd Lagrange.Milky/bin/Debug/net10.0
./Lagrange.Milky
```

---

## 版本支持

| 版本 | 偏移 |
|------|------|
| 3.2.19 | 0x5ADE220 |
| 3.2.28 | 0x57E1131 |

---

## 分支

| 分支 | 说明 |
|------|------|
| main | Fork + 本地签名 |
| 3.2.19-39038 | C++ HTTP 服务（封存） |
| 3.2.28-48517 | C++ HTTP 服务（封存） |

---

<div align="center">

真相是这样的：一个脆弱的灵魂打造了这个破碎又诡异的牢笼，
而在这个牢笼之中，一切行动都被赋予了"理由"。
拉格兰伸出双手，紧紧抱住卡戎。它的眼睛闪着光芒。
看到这一幕，她问道："……你就是我那时候看见的指引之光吗？"

</div>