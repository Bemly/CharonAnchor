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

首次运行会生成 `appsettings.jsonc`，编辑后重启生效。

```jsonc
{
    // 日志级别
    "Logging": {
        "LogLevel": { "Default": "Information" }
    },
    // 核心设置
    "Core": {
        // 服务器连接
        "Server": {
            "AutoReconnect": true,           // 断线自动重连
            "UseIPv6Network": false,         // 是否使用 IPv6
            "GetOptimumServer": true         // 自动选择最快服务器
        },
        // 签名服务（本地签名无需修改）
        "Signer": {
            "Url": ""
        },
        // 登录设置
        "Login": {
            "Uin": 0,                        // 账号，0 则扫码登录
            "Password": null,                // 密码，null 则扫码
            "DeviceName": "BemlyCharon",     // 设备名称
            "AutoReLogin": true,             // 断线自动重登
            "CompatibleQrCode": false,       // ASCII 兼容二维码
            "UseOnlineCaptchaResolver": true // 在线验证码识别
        }
    },
    // HTTP 服务设置
    "Milky": {
        "Host": "*",                         // 监听地址，* 为所有网卡
        "Port": 616,                         // 监听端口（容器内 616，-p 映射）
        "Prefix": "/",                       // URL 路径前缀
        "AccessToken": "your-token",         // API 令牌，null 则不验证（生产务必设置）
        "EnabledWebSocket": false,           // 是否启用 WebSocket
        // WebHook 回调 URL：
        //   bridge 模式 → host.docker.internal:6160（访问宿主机映射端口）
        //   host 模式   → 127.0.0.1:6160
        //   详见下方 Docker 神授说
        "WebHook": { "Url": "http://host.docker.internal:6160/cgi-bin/router.sh/qq" }
    }
}
```

**API 调用示例：**

```bash
curl -X POST http://127.0.0.1:616/api/send_private_message \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer your-token" \
  -d '{"user_id": 10000, "message": [{"type": "text", "data": {"text": "Hello"}}]}'
```

消息段类型：`text`, `image`, `face`, `reply`, `record`, `video`, `file`, `mention`, `mention_all`, `forward`, `market_face`, `light_app`, `xml`

Docker 神授说
============

**bridge 模式（推荐）** — 端口映射 + `host.docker.internal` 访问宿主机：

```bash
# 启动（需 --add-host 支持 host.docker.internal）
docker run -d --name Lagrange \
  --add-host host.docker.internal:host-gateway \
  -p 616:616 \
  -v /vol1/1000/Lagrange:/root \
  ghcr.io/bemly/charonanchor:3.2.28 Lagrange.Milky

# WebHook 配置（appsettings.jsonc）：
# "WebHook": { "Url": "http://host.docker.internal:6160/cgi-bin/router.sh/qq" }

# 外部容器访问 API：
# http://host.docker.internal:616/api
```

**host 模式** — 共享宿主机网络，`127.0.0.1` 直通：

```bash
docker run -d --name Lagrange \
  --network host \
  -v /vol1/1000/Lagrange:/root \
  ghcr.io/bemly/charonanchor:3.2.28 Lagrange.Milky

# WebHook 配置（appsettings.jsonc）：
# "WebHook": { "Url": "http://127.0.0.1:6160/cgi-bin/router.sh/qq" }

# 外部容器访问 API：
# http://127.0.0.1:616/api
```

> `--add-host host.docker.internal:host-gateway` 仅在 Linux 需要，Docker Desktop（Mac/Windows）内置支持。

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
