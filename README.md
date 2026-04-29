# CharonAnchor

Lagrange.Milky 签名服务 - 纯 C++ 实现，最小占用。

## 编译

```bash
./build.sh
```

## 使用

1. 提取 `wrapper.node` 和 `libgnutls.so.30`
2. 放到本项目目录
3. 运行 `./charon`

## API

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

## 适配新版本

修改 `src/main.cpp` 中的 `SIGN_OFFSET` 值。