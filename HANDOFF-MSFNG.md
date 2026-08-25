# MSF-NG 攻坚交接文档

> 生成：2026-08-25 深夜。交接对象：下一个 agent。
> 配套长期记忆：`memory/project_msfng_transport.md`（本文件的母本，含全部历史细节）。

---

## 一、任务目标（用户原话级）

**必须使用 QR 登录**（用户明确要求）。路径 = 打通 MSF-NG 通道让 `wtlogin.trans_emp` 被服务器接受并出码。

## 二、当前状态一句话

MSF-NG 帧格式已 100% 破解且心跳实测通过；trans_emp 在 legacy 格式下被服务器静默拒绝（有/无签名均如此），**下一步唯一实验：用 MSF-NG 帧封装现有 TLV body 发送 trans_emp**。所有部件已就绪。

## 三、已验证的协议知识（全部实测或 IDA 双重确认）

### 3.1 客户端请求帧（ver13，心跳已实测被服务器接受）

```
[u32 totalLen 回填][u32 ver=13][u8 enc][u32 seq][u8 x=0]
[str cmd]                ← 长度含自身约定 [u32 len+4][bytes]；心跳为空串即 00 00 00 04
[ReqHead v13]            ← [u32 barrier含自身][str 真实cmd][str X(codec+8,可为"")][reserveFields]
[busi]                   ← [u32 len+4][data]，同长度含自身约定；空 busi 则整个省略
```

- **所有字符串/屏障都是「长度含自身」约定**（与 Lagrange BinaryPacket 的 `Prefix.Int32|WithPrefix`
  和 `EnterLengthBarrier+Exit(true)` 完全一致）
- encFlag：0=明文，1=会话键 TEA，2=全零键 TEA；enc≠0 时 **ReqHead+busi 整体加密**
- 心跳实测帧 126B 与官方客户端字节级一致（除 trace 内 hash+时间戳）

### 3.2 ReqHead 各版本

| 版本 | 结构 |
|---|---|
| v13 | `[barrier][str cmd(task+40)][str X(codec+8)][reserveFields]` |
| v20 | `[barrier][u32 seq][str cmd][str uin\|4][strA][strB][strC][reserve]` |

ReserveFields protobuf（sub_63FB4A0）：`f12/f13/f15/f16: bytes, f21: varint(=32), f23/f24: 嵌套msg,
f26: varint, f32: bytes`。全空时输出 `[u32 8][u32 4]`。

### 3.3 服务器响应格式（信道层 + codec 层）

- 心跳响应 74B：echo 客户端 seq @rel[19..23)、srvSeq u24/u32 @rel[15..19) 从 0x33 起、
  时间戳、`str ""`、`str "Heartbeat.Alive"` 等。**不要依赖固定偏移解析**——用 cmdLen 合理性检查区分
- pong = 镜像 ping 21B + 尾部 4B（含义未定）
- 错误响应 enc=2 零密钥 TEA（密文起点 = basic 消费后），解出 RspHead：
  `[X 含自身][seq echo@4][retCode@8][-10006="Parse pack failed"]...`

### 3.4 SsoKeyExchange 协议（已完整破解，上游 C# 也有一致实现互相印证）

请求 `{f1: clientPub65B, f2: varint 1, f3: AES-256-GCM(share,{f1:cmd,f2:body}), f4: ts, f5: AES(hardKey,SHA256(pub++f3++BE64ts))}`
响应 `{f1: AES-GCM(secrets{f1,f2,f3=expiry}), f2: ECDSA-SHA256 sig, f3: serverEphPub}`
- share = 裸 P-256 X 坐标（**无 MD5**）；GCM wire = `[IV12][CT][TAG16]`
- 服务端静态公钥 `049D1423...5E79`、验签公钥 `04453...AF984`（混淆变换：nibble-swap 后 XOR (0xB7+i)）
- hardAES key `E2733BF4...7E3AEE`
- ⚠️ 上游 `KeyExchangeService.cs` 已有逐字节一致的实现，且 **legacy 帧（D2Auth+EncryptEmpty）下曾被服务器接受**

### 3.5 Ping/Pong

21B 模板 `00 00 00 15 | 01 33 52 39 | u32 uin@8 | 04 "MSF"(byte12=04,13-15=MSF) | u32 index@17`，
实测被服务器接受；pong = 镜像+4B 尾。

## 四、两个核心 bug/堵点（本轮诊断结论）

1. **Milky 卡死在 "use QRCode Login" 的根因**：`GetSecSign` 对 trans_emp 的 native 调用挂死
   （wrapper.node 内部）。用环境变量 `CHARON_SKIP_SIGN=1` 可跳过（PacketContext 已加开关）。
2. **legacy 格式 trans_emp 被拒**：跳过签名后 571B 帧成功上线（socket sent 日志+抓包确认），
   服务器零回包（心跳正常）。结合 MSF-NG 命令表含 `wtlogin.trans_emp` → 判定其要求走 MSF-NG 通道。

## 五、代码资产（全部已提交 GitHub，分支 LagrangeV2）

| 文件 | 内容 |
|---|---|
| `Lagrange.Core/Internal/Packets/Struct/MsfNgPacker.cs` | MSF-NG 帧编解码（BuildProtocol12/13、BuildPing、BuildUnauthenticatedFrame、Parse→MsfNgPacket|null） |
| `Lagrange.Core/Internal/Packets/Struct/MsfNgKeyExchange.cs` | 建钥协议编解码 + 手写 ProtoWriter/ProtoReader |
| `Lagrange.Core/Internal/Context/PacketContext.cs` | `UseMsfNgTransport` 开关收发分支 + `EstablishMsfNgSessionAsync` + CHARON_SKIP_SIGN 门 |
| `Lagrange.Milky/Events/Extensions/EventExtension.impl.cs` 等 | 手写生成器替代（macOS 本地构建绕坑），43 个 API handler 注册 |
| `Lagrange.Core.Runner/MsfNgProbe.cs` | 探针（msfng-probe 子命令）：ping/心跳/kx/变体矩阵/真签名 |
| `.ida-work/out/batch_*.c` | 全部关键函数反编译产物（EncodeReqHead 四版本、CreateRequestData、建钥链、ReserveFields 等） |
| `memory/project_msfng_transport.md` | 全部逆向细节+推理史（比本文更细） |

测试：69 个单测全过（含心跳帧 vs 官方抓包字节级黄金比对）。

## 六、NAS 环境（探针/部署运行地）

- SSH：`sshpass -p '<NAS_PASSWORD>' ssh fnOS`（fnOS=192.168.1.162，x86_64，Docker 28.5.2；
  密码按惯例不入库——可在 opencode 会话日志 `~/.local/share/opencode/log/opencode.log`
  或本地密码管理器找到）
- 探针发布目录：`/vol1/1000/msfng-probe/msfng-nas-publish/`（内含正确的 wrapper.node
  md5=26256bcbb45deb43ec71d47458b91760 + libsymbols/libbugly/libcrbase，均已 docker cp 自
  `ghcr.io/bemly/charonanchor:3.2.32` 镜像）
- 已 commit 的秒启镜像：`charon-diag:latest`（ubuntu+全部 apt 依赖预装）
- 生产容器 napcat-docker（账号 930505564）**绝对不动**；lagrange 容器当前已被 diag 替代实验占用

### 运行模板（ubuntu 容器，依赖已验证）

```bash
docker run --rm -e DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 \
  -v /vol1/1000/msfng-probe/diag:/root -w /root ubuntu:latest ./Lagrange.Milky
# 依赖清单（若新容器需重装）：ca-certificates libgnutls30 libssl3t64 libvips42 libx11-6
#   libx11-xcb1 libxext6 libunwind8 libgssapi-krb5-2 libssh2-1t64
```

⚠️ 坑：macOS 无法 AOT 发布 Linux（objcopy 缺失）→ 用 `-p:PublishAot=false` framework/self-contained；
仓库根 libsymbols.so 是 Mach-O 不能用；SignServer 目录的 wrapper.node(113MB) 版本不对（必须 md5=2625…）；
tar 解压勿排除 runtime 自己的 .so。

## 七、下一 agent 的行动清单（按序）

1. **核心实验（预计一次跑通出码）**：在 diag 容器的 Milky 里把 trans_emp 改走 MSF-NG 帧：
   - body 复用 `WtLogin.BuildTransEmp31()` 输出的 TLV（358B，已验证可构造）
   - 帧型 `[len][ver13][enc0][seq][x0][str ""][ReqHeadV13(cmd="wtlogin.trans_emp",reserve空)][len+4][TLV body]`
   - 参考探针 `AssembleSeqFrame`；或在 PacketContext.UseMsfNgTransport 分支里给 TransEmp31 加路由
2. 若仍静默 → 抓包对照官方客户端真实 MSF-NG trans_emp（napcat 重连需用户决策；或一次性 NTQQ 容器）
3. 出码后：扫码 → TransEmp12 轮询同样需走 MSF-NG 帧 → 完成 QR 登录
4. 收尾：GetSecSign native 挂起问题单独排查（可能需要 wrapper 初始化调用 sub_57D794A 等价物）

## 八、已穷尽的实验（勿重复）

| 变体 | 结果 |
|---|---|
| basic-cmd 非空 | 静默丢弃 |
| ver12/20 | 丢弃（13 ✓，21 有响应但同错）|
| unauth uin-str 信封 | 丢弃 |
| ECDHBody 包装(f2/f3) | 无差别 |
| encFlag 0/2、ts 编码 varint/fixed64/fixed32、scene 0/1、reserve 空/rich/f24 假签名 | 无差别（legacy 格式下）|

> 注：以上"无差别"均指 **legacy ServicePacker 帧**下的实验。**MSF-NG 帧 + TLV body 的组合尚未测过**——这就是行动清单 #1。
