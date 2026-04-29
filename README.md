# CharonAnchor

<img src="Charon.png" width="200" height="200" align="right">

Lagrange.Milky 签名服务 -
（卡戎：失败的作品）拉格兰的现实锚定点

创造
============
```bash
./build.sh
```

锚定
============
1. 提取 `wrapper.node` 和 `libgnutls.so.30`
2. 放到本项目目录
3. 运行 `./charon`

人为构造
============
```
POST http://127.0.0.1:8080/

请求:
{
    "cmd": "xxx",
    "src": "hex字符串",
    "seq": 123
}

响应:
{
    "token": "hex字符串",
    "extra": "hex字符串",
    "sign": "hex字符串"
}
```

自我投影
============
修改 `src/main.cpp` 中的 `SIGN_OFFSET` 值。


------------
真相是这样的：一个脆弱的灵魂打造了这个破碎又诡异的牢笼，
而在这个牢笼之中，一切行动都被赋予了"理由"。
拉格兰伸出双手，紧紧抱住卡戎。它的眼睛闪着光芒。
看到这一幕，她问道："……你就是我那时候看见的指引之光吗？"