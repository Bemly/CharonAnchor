# CharonAnchor

<img src="Charon.png" width="200" height="200" align="right">

Lagrange.Milky 本地签名服务

失败的作品·卡戎 - 拉格兰的现实锚定点

创造
============
```bash
cd Lagrange.Milky
dotnet build
```

支持的版本:
- `3.2.19-39038` · SIGN_OFFSET=0x5ADE220
- `3.2.28-48517` · SIGN_OFFSET=0x57E1131

锚定
============
将以下文件放在同一目录：
- `Lagrange.Milky` · 主程序
- `wrapper.node` · 签名模块（从 Lagrange.Milky 依赖提取）
- `libsymbols.so` · 符号补丁
- `libbugly.so` · 依赖库
- `libcrbase.so` · 依赖库

运行：
```bash
cd Lagrange.Milky/bin/Debug/net10.0
./Lagrange.Milky
```

人为构造
============
扫码登录后即可使用 Milky API：

```
POST http://127.0.0.1:6101/api/send_group_message

请求:
{
    "group_id": 群号,
    "message": [{"type": "text", "data": {"text": "消息内容"}}]
}
```

自我投影
============
通过切换分支选择版本：
- `Lagrange-3.2.28` · 当前分支（内置本地签名）
- `3.2.28-48517` · C++ HTTP 服务（封存）
- `3.2.19-39038` · C++ HTTP 服务（封存）

------------
真相是这样的：一个脆弱的灵魂打造了这个破碎又诡异的牢笼，
而在这个牢笼之中，一切行动都被赋予了"理由"。
拉格兰伸出双手，紧紧抱住卡戎。它的眼睛闪着光芒。
看到这一幕，她问道："……你就是我那时候看见的指引之光吗？"