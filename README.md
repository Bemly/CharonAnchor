# CharonAnchor

<img src="Charon.png" width="200" height="200" align="right">

Lagrange.Milky 签名服务

失败的作品·卡戎 - 拉格兰的现实锚定点

创造
============
```bash
./build.sh                    # 默认 3.2.19-39038
./build.sh 3.2.28-48517       # 新版本
```

支持的版本:
- `3.2.19-39038` · SIGN_OFFSET=0x5ADE220
- `3.2.28-48517` · SIGN_OFFSET=0x56D5491

锚定
============
将以下文件放在同一目录：
- `charon` · 主程序
- `wrapper.node` · 签名模块（从 Lagrange.Milky 依赖提取）
- `libsymbols.so` · 符号补丁
- `libbugly.so` · 依赖库
- `libcrbase.so` · 依赖库

运行：
```bash
./charon
```

人为构造
============
```
POST http://127.0.0.1:8080/api/sign/sec-sign

请求:
{
    "uin": 账号,
    "command": "命令字符串",
    "seq": 序列号,
    "body": "hex字符串(小写)",
    "guid": "hex字符串",
    "qua": "版本字符串"
}

响应:
{
    "code": 0,
    "message": null,
    "value": {
        "sec_sign": "hex",
        "sec_token": "hex",
        "sec_extra": "hex"
    }
}
```

自我投影
============
通过 build.sh 参数选择版本即可。如需适配其他版本，用 IDA 分析 wrapper.node 找到签名函数偏移后添加到 build.sh。

------------
真相是这样的：一个脆弱的灵魂打造了这个破碎又诡异的牢笼，
而在这个牢笼之中，一切行动都被赋予了"理由"。
拉格兰伸出双手，紧紧抱住卡戎。它的眼睛闪着光芒。
看到这一幕，她问道："……你就是我那时候看见的指引之光吗？"
