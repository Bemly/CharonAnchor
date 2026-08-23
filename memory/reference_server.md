---
name: server-info
description: NAS 服务器和测试环境
type: reference
---

## NAS 服务器连接

SSH 连接：
```bash
sshpass -p '<NAS_PASSWORD>' ssh fnOS   # 密码不入库，见本地密码管理器
```

sudo 密码：同上（不入库）

---

## 测试环境

**目录：** `/vol1/1000/Lagrange/`

**⚠️ NAS 上有其他生产容器，不要动：** `napcat-docker`（QQ 账号 930505564，与本项目账号 3156037162 不同，配置在 `/vol1/1000/NapCat/`）、`astrbot`、`chromium` 等。

**配置文件名随版本变化：**
- 3.2.29 及更早：读 `appsettings.jsonc`（旧结构：Core/Milky 扁平节）
- 3.2.32+（LagrangeV2 分支合并上游后）：读 `appsettings.json`（新结构：`Lagrange`/`Milky` 分节，`HttpServer.Host/Port`；DeviceName 在 `Lagrange.Login.DeviceName`）
- 3.2.32 的 Program.cs 硬编码找 `appsettings.json`，找不到会生成默认配置——部署时必须先放好新结构配置

**当前卷内备份（2026-08-22 排查所留）：**
- `appsettings.jsonc.bak-3.2.29` — jsonc 原件副本
- `3156037162.keystore.bak` + `3156037162.ks.stale-20260822` — 旧 keystore（5 月会话已过期，勿直接复用）
- 新格式 `appsettings.json` 已按 3.2.32 结构写好（Uin/DeviceName=localhost/Port 616/WebHook 均保留）

**依赖文件：**
- wrapper.node（签名模块）
- libsymbols.so
- libbugly.so
- libcrbase.so

**更新 Lagrange 容器：**
```bash
sudo docker stop lagrange
sudo docker rm lagrange
sudo docker pull ghcr.io/bemly/charonanchor:<TAG>
sudo docker run -d --name lagrange \
    -v /vol1/1000/Lagrange:/root \
    --network bridge \
    --add-host=host.docker.internal:host-gateway \
    -p 616:616 \
    ghcr.io/bemly/charonanchor:<TAG>
```
- 2026-08-22 现状：回退运行 **3.2.29** 镜像供用户自行测试 QR 登录；3.2.32 镜像已发布但被 trans_emp 服务端阻塞挡住

**API 端点：** `http://127.0.0.1:6101/api/...`

---

## 本地测试

```bash
# 构建
cd /Users/bemly/cchaha/CharonAnchor
dotnet build Lagrange.Milky

# 运行（开发模式）
cd Lagrange.Milky/bin/Debug/net10.0
./Lagrange.Milky
```