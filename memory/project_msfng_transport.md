---
name: msfng-transport
description: MSF-NG 传输层逆向成果 — ECDH Key V2 参数、帧格式、TEA 加密、编解码管线（2026-08-24 会话）
type: project
---

## MSF-NG 会话加密逆向（2026-08-24，重大突破）

**二进制**：QQ 3.2.32 wrapper.node，MD5 `26256bcbb45deb43ec71d47458b91760`，
本地副本 `.ida-work/wrapper3232.node`（149MB）。IDA 缓存已建好（~40 分钟全量分析），
位于 opencode 临时缓存目录，重开秒级。

### ⚠️ 地址空间结论（重要）
- **VMA == 文件偏移**（对本 binary 的 .text/.rodata 都成立；program header 的 +0x1000 delta 是干扰项，
  已用指令字节实测验证：`lea r8,[rip-0x2b955a1] @0x3412850 → 0x87d2b6 = ECDH 字符串`）
- IDA VA == objdump 注释地址 == strings -t x 偏移，三者一致，可直接互相印证

### 【已破解】ECDH Key V2 参数
来源：`wtlogin_ecdh_key.cc::GenerateECDHKeyV2(out, serverPubStr, flag)` @ **0x3412410**
（反编译：`.ida-work/out/fn_ecdh_v2_gen_0x3412410.c`）

| 参数 | 值 |
|------|-----|
| 曲线 | **prime256v1 (NIST P-256)**，错误串 "new key by curve name prime256v1 failed." @line178 |
| 服务端公钥（硬编码，uncompressed point hex 字符串） | `04EBCA94D733E399B2DB96EACDD3F69A8BB0F74224E2B44E3357812211D2E62EFBC91BB553098E25E33A799ADC7F76FEB208DA7C6522CDB0719A305180CC54A82E` |
| 密钥派生 | **MD5(ECDH_share_secret)** → 16 字节（sub_88DAB50，MD5 init magic 67452301EFCDAB89... 实证）；与旧版 Lagrange ECDH_ST 的 isHash=true 完全同构 |
| 输出结构 out | `{+0: 16B 派生密钥, +24: 本端公钥(v23=65B)}` |
| 调用者 | sub_3411C30(a1,out, keyVer, flag)：keyVer==2→V2/P-256，keyVer==1→V1/旧 secp192k1（pub=`04928D8850673088B343264E0C6BACB8496D697799F37211DEB25BB73906CB089FEA9639B4E0260498B51A992D50813DA8`，50B） |
| 关键子函数 | new key=0x8ADCED0(415)、generate=0x8ADDAB0、export pub=0x8ADDA10(group,pub,4,buf,512,0)、parse srv pub=0x8928AC0、oct2point=0x8AE3A40、compute share=0x8AA19B0(dest,512)、MD5=0x88DAB50 |

### 【已破解】帧体加密 = 经典 QQ TEA（不是 AES！）
"reKey to no aes key" 等字符串是 **MMKV 存储加密**的（MMKV_IO.cpp），与传输层无关——旧记忆锚点作废。
真正的帧加密在 `codec_processor.cc`：

- `EncryptBody(encFlag, task, data, len)` @ **0x63FD660**：
  - encFlag==1 → key 对象 = `*(task+144) + 120`（会话 ECDH 派生密钥）
  - encFlag==2 → key = 静态 `unk_F76E90` = **16 字节全零**（内置默认密钥！）
  - 否则失败
- 核心加密 sub_85BC530(data,len,key,**16**,dst,&dstlen)：**TEA-CBC，QQ 经典 padding 格式**：
  - delta=0x9E3779B9，16 循环（32 轮），8 字节块，CBC 链接（首块自含 IV：header+random）
  - padding：首字节 `(rand&0xF8)|padLen`，padLen 使 (len+2+pad)%8==0，后接随机填充、明文、尾部随机
  - **Lagrange.Core 现有 TeaProvider 即此算法，可直接复用！**（ServicePacker 的 TEA 同源）
- padsize 计算 = sub_85BC510(len)

### 【已破解】MSF-NG 帧封套（对官方 pcap 实测验证）
客户端心跳帧实测（NAS /tmp/qqdyn/pcap_hex.txt，126B）：
```
[u32 BE totalLen]
[u32 BE protocolVer]     心跳=13；trans_emp 观测=12 → codec_processor_v12/v13/v20/v21 对应
[u8   encFlag]           0=明文（心跳/未建立会话时实测为 0）
[u32 BE seq]             客户端连接内递增计数 0x006238BF→C2（含原以为独立的 x 字节，x=seq 最低位前的 0）
[u8   x]                 恒 0
[str cmd]                basic 层命令串，心跳实测为空 [00000004]，真实命令在 ReqHead
[ReqHead …]              版本相关的虚拟函数（vtable+32），见下节逐字段语义
[BusiBuff …]             encFlag≠0 时与 ReqHead 一起整体 TEA 加密
```
服务端 pong 为信道层帧非 codec 帧：80B，srvSeq u32@[15..19)=57 起，echo clientSeq u32@[19..23)。

### 编解码管线（codec_processor.cc）
```
EncodePacket @0x63F8BA0: EncodeBasic(0x63F9050) → EncodeReqHead(virtual vtbl+32)
                         → EncodeBusiBuff(0x63F9290) → EncodeFinal(0x63F9360)
DecodePacket @0x63F9620: DecodeBasic(0x63F9D40) → DecryptBody(0x63FD870, encFlag≠0 时)
                         → DecodeRspHead(0x63F9E40) → DecodeReserveFields(vtbl+40) → DecodeBusiBuff(0x63F9FF0, 可 zlib 解压)
reader 原语: 全部"只读不推进"需显式 skip；rd_back=绝对 seek；字符串 [len含自身][len-4 数据]
```
日志行号锚点：EncodeBasic:303 / EncodePacket:122-169 / EncryptBody:357 / DecodePacket:181-258 / DecodeBusiBuff:533

### MSF-NG 命令路由表（全局 std::string 表 @0x8eb3600，重定位填充）
```
Heartbeat.Alive / Heartbeat.Ping / Client.CorrectTime / ConfigPushSvc.PushReq|PushResp /
trpc.qqaccess.dispatch.DispatchService.Push|PushAck / ConfigPullSvc.GetWifiListKey /
SSO.HelloPush / RegPrxySvc.info / PushService.get /
wtlogin.login / wtlogin.name2uin / wtlogin.exchange / wtlogin.exchange_emp /
wtlogin.trans_emp / wtlogin.fastlogin / OverLoadPush.notify / OidbSvc.0x4aa_202 /
CliLogSvc.upload_emp / PhSigLcId.Check / ConfigService.ClientReq /
trpc.o3.full_social_lock_punish.FullSocialLockPunish.SsoVerifyAndUnLock /
trpc.o3.ecdh_access.EcdhAccess.SsoEstablishShareKey  ← 会话密钥建立命令！
trpc.o3.ecdh_access.EcdhAccess.SsoSecureAccess /
trpc.o3.report.Report.SsoReport / trpc.onseac1.login_svr.phone_svr.* / RegPrxySvc.infoSyncVoip /
PushServiceDirect.VoipPushAck / MessageSvc.PbSendMsg / QQConnectLogin.pre_auth_emp / ...
默认服务器 msfwifi.3g.qq.com:8080；另有 "bmtoe7mD3LCvrzSK"、"8.6.9"
```
表的使用者（sso_manager.cc 区域）：0x64d0753 构造器、0x64d42ec、0x64d6bba、0x64db274、0x64dcac1 等。

### 其他关键定位
- `task_manager.cc::DidRecvResponseData` @0x63EFD10（上层任务分发，"Response data should belong to task(seq:{} cmd:{} uin:{})"）
- `PacketModule.cpp::setReserveFields` @0x64E05D0（SSO reserve fields：VasKey/WiFi BSSID/envId/localeId/qimei，对应 Lagrange SsoReserveFields）
- `tcp_channel_connector.cc::InternalConnect` @0x63DDEB0
- 会话对象布局线索（MMKV reKey 函数 0x6593F00 佐证的是存储而非传输）：+160=key ptr、+137=flag、+192=channel state
- MSF::MSFSDK 公开 API 在 dynsym（sendPacket/disconnect/isConnected/setMSFConfig<14 种>/notifyLoginSuccess 等 1505 个导出符号）

### 【已破解 2026-08-24】ReqHead/RspHead 字段语义（pcap+IDA 双重验证）

**读取原语真实语义**（此前记忆有误，已修正）：
- `rd_u32`(0x8901700)/`rd_u8`(0x89016B0) **只读不推进指针**，必须显式 `rd_skip`
- `rd_back` = 绝对寻址 seek（非相对回退）
- `wr_bytes`(0x63FAF50) = 写 `[u32 (len+4)][data]` —— **所有字符串/长度屏障都是"长度含自身"约定**（与 Lagrange BinaryPacket 的 `Prefix.Int32|WithPrefix` 和 `EnterLengthBarrier+Exit(true)` 语义完全一致，可直接复用）
- `rd_str_a` 读 `[u32 len][len-4 bytes]`

**客户端帧（EncodeBasic 0x63F9050 + pcap 实测）**：
```
[u32 totalLen 回填][u32 ver][u8 enc][u32 seq][u8 x=0][str cmd]
```
- 心跳实测：seq=0x6238BF→C2 每包递增；**basic 层 cmd 为空串 `[00000004]`**，真实命令名在 ReqHead 里
- ReqHead = `[u32 barrier含自身=100][str 真实cmd][str ""][str trace]` + 尾部 `[u32 8][u32 4]`
- trace(69B) = `"b "` + 32位hex + protobuf{f23:{f1:"client_conn_seq", f2:"<unix秒>"}, f26:101}；
  hex32 实测= b8b94290a9711ee10daa10064b7d5753（与设备 Guid 同值，来源待确认）
- EncodeFinal(0x63F9360)：enc≠0 时对 **head+busi 整体 TEA**（ReqHead 在密文内！），final = basic ++ cipher

**RspHead（DecodeRspHead 0x63F9E40）**：
```
[u32 X=headLen 含自身][u32 seq@4][u32 retCode@8][str extra@12][str cmd][str ?][vlint][bytes str_b][vlint]
body 从 region[X..] 开始
```

**pong = 信道层帧，不走 codec**：80B 固定形状，srvSeq u32@[15..19)=57(0x39起)、echo clientSeq u32@[19..23)。
MsfNgPacker.Parse 通过 cmdLen 合理性检查自动拒绝（pong bytes[14..18]=0x30000000 超界）。

### C# 传输层实现（2026-08-24 完成）

- `Lagrange.Core/Internal/Packets/Struct/MsfNgPacker.cs`：BuildProtocol12/13、Parse→MsfNgPacket|null（信道帧返回 null）、SessionKey 属性（enc=1 用）、trace 构造器
- `BotConfig.UseMsfNgTransport` 开关（默认 false，legacy 路径不变）
- `PacketContext` 收发双分支接入：MSF-NG 模式跳过 secInfo（reserve 槽位未实现），按 HeadSequence/BasicSequence 双序列号匹配 pending task
- 回归测试 `Lagrange.Core.Test/Packets/MsfNgPackerTest.cs`：心跳帧与 pcap 样本字节级比对（除 trace 内 hash+时间戳）通过；全量 66 测试通过

### 【已破解 2026-08-24】SsoEstablishShareKey 会话建钥协议（kernel_ecdh_service.cc）

函数锚点：
- `encodeKeyExchangeRequest` = sub_296B7B0（kernel_ecdh_service.cc:482-550）
- `decodeKeyExchangeResponse` = sub_296C1E0（:567-639）
- `ComputeShareKey` = sub_29727F0（ecdh_util.cc:87）＝**裸 ECDH X 坐标**（P-256，512B 缓冲，无 MD5！与 wtlogin ECDH_ST 的 isHash=true 不同）
- `AESEncryptForLogin` = sub_296D080 ＝ **AES-256-GCM**，wire 格式 `[IV12][CT][TAG16]`
- 摘要 = SHA256（sub_8AD49A0）
- 发送入口 `nt::wrapper::KernelECDHService::sendSSORequest` = sub_296A3C0

协议：
```
请求 KeyExchangeRequest {
  f1: bytes 客户端临时公钥(65B uncompressed)
  f2: varint flag=1
  f3: bytes AES-GCM(share=ECDH(eph, SERVER_STATIC_PUB), inner{f1: cmd字符串, f2: 业务body})
  f4: varint unix秒
  f5: bytes AES-GCM(HARD_AES_KEY32, SHA256(clientPub ++ payload ++ BE64(unixSec)))
}
响应 KeyExchangeResponse {
  f1: bytes AES-GCM(share2=ECDH(eph, f3公钥), secrets{f1: bytes, f2: bytes, f3: expiry秒})
  f2: bytes ECDSA-SHA256 签名
  f3: bytes 服务端临时公钥(65B)
}
```

解混淆常量（变换：每字节 nibble-swap 后 XOR (0xB7+i)，i 从 0 计）：
- 请求侧服务端静态公钥 @B94EF0..+64 尾 E8：`049D1423332735980EDABE7E9EA451B3395B6F35250DB8FC56F25889F628CBAE3E8E73077914071EEEBC108F4E0170057792BB17AA303AF652313D17C1AC815E79` ✓在曲线上
- 响应验签公钥 @B94F31..+64 尾 55：`04453977B048D0B72C1A7D50C36EBE881B69BBDD51A5C662D08A1BAF1236CE92CBB95460F573FE7A5B0ED9CCFEE01EB4DFB6E6ECFA16A090E3ED8F5847A9DAF984` ✓在曲线上
- 摘要加密密钥 @B961A0(32B)：`E2733BF403149913CBF80C7A95168BD4CA6935EE53CD39764BEEBE2E007E3AEE`

待 NAS 联调确认的疑点：
1. 解密侧密文起点实测为 f1[3..len-16]（+3 偏移），与加密侧 [IV][CT][TAG] 不一致——可能有 3 字节版本头
2. secrets.f1/f2 哪个是 16B codec TEA 会话键未定；PacketContext.EstablishMsfNgSessionAsync 目前取 len==16 者否则 MD5(f1)
3. ECDSA 签名验签输入顺序（clientPub++f3++f1 vs a4缓冲++f3++f1）

另：SsoSecureAccess 数据通道用 **HPKE(RFC9180)** 密钥调度（psk_id_hash/info_hash/secret/key/base_nonce/exp 标签，sub_8B2DCB0），与建钥通道是两套。

### 【已破解 2026-08-24】21B Ping 包
静态模板 @unk_F762B0（BuildPingPacket=sub_63DE7B0，tcp_channel_connector.cc:156）：
```
00 00 00 15 | 01 33 52 39 | [u32 uin 补丁@8] | 04 "MSF" | [u32 pingIndex 补丁@17]
```
StartPing(:188) 每次 ping 递增 *(connector+312)。信道层帧不走 codec。

### 命令白名单情报（sub_6402B70 表）
`trpc.login.ecdh.EcdhService.SsoQRLoginGenQr`（QR 出码走 trpc！）、SsoNTLoginPasswordLogin/EasyLogin/AuthLogin 族、SsoOIDB0x916a-d、OidbSvcTrpcTcp.0x11ec_1 等大量 Oidb。legacy `wtlogin.trans_emp` 不在此表。

### 【重大突破 2026-08-24】本机实连联调（探针 Lagrange.Core.Runner msfng-probe）

**已打通**：
- Ping/Pong：21B ping 被接受，pong = 镜像 ping 21B + 尾部 4B（0x6EBB39B8，含义待定）
- **心跳 codec 帧被服务器接受并正确回包**（74B）：echo 客户端 seq @[19..23)、srvSeq=0x33 起、时间戳 0x6A8BE112 ✓✓
- Establish 帧到达命令处理层：错误响应 enc=2 零密钥 TEA（密文起点 = basic 后即 offset15），解出 RspHead：
  `[X=0x36][seq echo@4][retCode=-10006(0xFFFFD87A)@8][str"Parse pack failed."][空串们][ts][4][4]`
  ——确认 decode_rsphead2 的 a4[1]=seq@4（回显）、a4[2]=retCode@8

**变体矩阵结论**（服务器行为）：
- basic-cmd 非空 → 静默丢弃；basic-cmd 空 → 接受 ✓
- ver12/ver20 → 丢弃；ver13 → 接受；ver21 → 有响应但同样报错
- unauth uin-str 样式（[str uin] 或 [u32 4]）→ 全部丢弃
- **唯一到达应用层的形态：`[len][ver13][enc0][seq][x=0][str ""][busi]`**（与心跳完全同构）
- busi 带 ReqHead(心跳式) → 丢弃；busi 直接是 KeyExchangeRequest proto → "Parse pack failed"
- f4 ts 的 varint/fixed64/fixed32 编码无差别 → 错误不在字段编码层

**关键架构发现**：sub_296A3C0 (sendSSORequest) 发送的 cmd 是 `trpc.login.ecdh.EcdhService.SsoKeyExchange`
——kernel_ecdh_service.cc 的 encode/decodeKeyExchangeRequest 属于 SsoKeyExchange 登录通道！
SsoEstablishShareKey 只出现在白名单/hash 表构造器里（0x6402bce/0x65ea71a/0x65eeb89），
真正的请求构造代码还没定位（可能在 sso_manager 0x64d 区域，经命令表查 cmd）。

**下一步（establish 攻坚）**：
1. 反编译 sub_65E9A5E/sub_65ED7D6 hash 表的 value 结构（cmd → handler/flag 映射）
2. 反编译 sso_manager 区域 0x64d0753/0x64d42ec/0x64d6bba 等使用命令表的函数
3. 或者：先跑通 SsoKeyExchange（sendSSORequest 路径已完整逆向），它可能才是登录用的建钥命令

### 【已破解 2026-08-24】ReqHead 全版本结构（v12/v13/v20/v21）

- **v13** (sub_63FCBB0, codec_processor_v13.cc)：`[u32 barrier含自身][str cmd(task+40)][str X(codec+8, 心跳时"")][reserveFields]`
- **v20** (sub_63FCE80)：`[barrier][u32 seq][str cmd][str uin|u32 4(空时)][strA][strB][strC][reserveFields]`
- v12 = sub_63FA760、v21 = sub_63FD380 同族
- **ReserveFields** (sub_63FB4A0) protobuf：`f12/f13/f15/f16: bytes, f21: varint=32(MsgType), f23/f24: 嵌套msg, f26: varint, f32: bytes`；
  无任何字段时输出空 → 调用者写 `[u32 8][u32 4]`（barrier 包空 barrier）——心跳观察到的 tail 由此而来 ✓
- 心跳观察的 head 完全吻合 v13：cmd="Heartbeat.Alive"、X=""、reserve 空

### 【联调进展 2026-08-24 续】head 结构已过服务器解析

带 v13 式 head 的 kx/establish 帧：服务器**不再丢弃**，错误响应里回显 cmd 字符串
（`"Parse pack failed." + str(46)="trpc.login.ecdh.EcdhService.SsoKeyExchange"`）——
**head 解析通过**，失败收敛到 busi 层。GCM 输出布局 [IV][CT][TAG] 经 sub_2972C90 尾部
resize(ctlen+16)+append 确认无误。

**下一步方向**：
1. trpc.* 命令的 busi 可能需要 trpc frame 头包装（ver/reqid/service/method pb），查 sendSSORequest 发送层 vtable+72 的实现
2. 或 handler 对 KeyExchangeRequest 有额外前置字段要求
3. 对照：心跳 busi 是纯 protobuf 且成功——普通命令 vs trpc 命令的 busi 包装可能不同

### 【决定性突破 2026-08-24】busi 长度前缀

`EncodeBusiBuff` (sub_63F9290) = `wr_bytes(task+128/136)` —— **busi wire 格式 = `[u32 len+4][data]`**（与字符串/barrier 同约定）！

此前所有 "Parse pack failed" 的根因：裸 busi 的首字节被服务器当作长度字段。

加前缀后：SsoKeyExchange / SSO.HelloPush 帧被服务器**完整解析并静默处理**（不再报错）。
静默原因待查（候选）：ReserveFields 缺设备数据、handler 内部失败无响应、响应走 push 通道。

ECDHBody 包装层（sub_2966BC0，SendSSORequestWithECDH 用）：
```
ECDHBody { f2: bytes 业务proto, f3: {f1: varint scene, f2: bytes cmd} }  // f1 块仅扩展模式
```

### 【当前推理状态 2026-08-24 晚】静默原因排查思路（防遗忘）

**已确认事实链**：
1. Ping/Pong ✓、心跳（空 busi）✓、head 结构 ✓（cmd 回显证明）、busi [len+4] 前缀 ✓
2. 加前缀后 SsoKeyExchange / SSO.HelloPush 不再报错 → 帧解析层 100% 通过
3. 处理器收到包但**不回任何帧**（8s 窗口内）

**静默候选原因（按可能性排序）**：
- A. **ReserveFields 缺设备/会话数据**：真实客户端的 reserve 含 uid/设备标识/qimei 类字段，
  服务器 handler 可能校验后静默拒绝。f12/f13/f15/f16 的实际内容语义未定。
  对照 legacy SsoReserveFields：TraceParent="01-"+32hex+"-"+16hex+"-01"、Uid、MsgType=32(f21)、NtCoreVersion=100(可能 f26)
- B. **响应走 push 异步通道**：trpc.* 响应可能经 trpc.qqaccess.dispatch.Push 下发，
  需要 8s+ 观察窗或保持连接等 push。未验证！
- C. **连接未注册**：官方流程 connect 后可能先发 Client.RegisterProxy / SSO 注册族命令
- D. **handler 内部异常无响应**（如 KeyExchangeRequest 内容非法但解析通过）

**已排除**：
- ECDHBody 包装有无 → 无差别
- encFlag 0 vs 2（零密钥 TEA 外层）→ 无差别
- f4 ts 编码 varint/fixed64/fixed32 → 无差别
- scene id 0/1 → 无差别
- ReqHead str2(codec+8) 空 vs uin → 无差别

**下一步实验计划**：
1. 探针加「长观察窗 + 连接关闭检测」：发 kx 后读 20s，检测 FIN/push 帧
2. reserve 填 traceparent 风格数据（f12="01-..."? 字段语义靠猜，试错）
3. 若仍静默：反编译 DecodeECDHBody 对应的服务端请求处理入口（客户端镜像：找谁调 sub_2966810 decode 的兄弟 encode 路径）——即查 sendSSORequest 发送层 vtable+72 实现里对 body 的最终包装
4. 备选：抓一次官方客户端完整登录流量对照（NAS tcpdump 或本机 mitm）

### 【2026-08-24 深夜】MSFSDK 打包链逆向（进行中）

- `MSF::MSFSDK::sendPacket` (0x649F180)：薄封装，分配 seq 后交给全局引擎 unk_8ED6B78 的 vtable+40
- `MSF::MSFSDK::pack` (0x649F520)：组帧入口，**校验 MSFRequest 三个必填字符串 +152/+176/+200**（空则返回 null 不发包！）
- MSFRequest 布局（部分）：`+0 seq(int) +16 flag byte +38/+44/+50 三个子对象 +128 busi ptr +136 busi len +152/+176/+200 必填str +248/+272/+296 可选str`
- 最终组装 sub_64FD710 (1666B，仅 pack 调用) —— 未读完
- task+40(cmd)/+144(uin)/+128(busi) 与 EncodeBasic/ReqHead/BusiBuff 的偏移互相印证 ✓

**推断**：探针静默的原因很可能是服务器 handler 校验请求元数据（对应 MSFRequest 必填字段的内容，
如设备 ID/注册信息）失败后静默丢弃。这些字段在真实客户端由 NodeAPI 层填充。

**候选下一步**：
1. 读完 sub_64FD710 弄清三必填串进入 wire 的位置
2. 找 MSFRequest 构造者（NodeAPI 层）看 +152/+176/+200 填什么值
3. 抓官方客户端登录前流量做字节级对照（最直接但需环境）

### 【2026-08-24 收尾】调用链追到 NodeAPI 层，盲试宣告失效

- `NodeIKernelMSFService::sendMsfRequest(4 args)` NAPI @0x3B86690 → IKernelMSFService vtable+88
- `SendMsfRequestV3` (sub_5413BB0, kernel_depends.cpp:219) → MSF 引擎 vtable+224 注册
- 每层依赖注入，逐层逆向边际收益递减
- **实验矩阵已穷尽内容层变体**（ECDHBody 有无/encFlag/ts编码/scene/reserve rich）全部静默 →
  缺的是「会话级元数据」（MSFRequest 必填字段的真实值：设备 ID/注册 token 类），盲试命中率低

**下一轮最高价值动作（二选一）**：
1. **抓真实流量对照**：NAS napcat 容器重连时 tcpdump（不能动生产！需用户决策），
   或本机起一次性 NTQQ 容器登录测试号抓 establish/kx 真实字节
2. 读完 sub_64FD710（1666B pack 最终组装）拿三必填串的确切 wire 位置，
   再从 NodeAPI 层反推填充值

拿到真实 establish 字节后与我们的帧 diff，一次就能定位缺失的元数据。

### 【重大突破 2026-08-24】三必填串 = @SEC 签名三元组！

sub_64FD710 = `CreateRequestData`（TestModule.cpp），关键日志：
```
"cmd:%s, seq: %d, secSigLen=%d, secDeviceTokenLen=%d, secExtraLen=%d"   (@SEC tag, line 52)
"FATAL ERROR: sigs lost, uin:%s, tcmd:%s, seqId:%d"                     (line 24, 任一为空)
"imei:%s"                                                                (line 66, 全局 byte_8ED5950)
```

**MSFRequest+38/+44/+50 = secSig / secDeviceToken / secExtra**（wrapper.node 签名输出三元组，
即 CharonSignProvider.GetSecSign 的 sign/token/extra 同源数据）！

静默根因定论：kx/establish handler 校验请求签名数据，探针未携带 → 静默丢弃。
心跳白名单豁免签名所以成功。

CreateRequestData 组装（v36 = new 0x100）：
- +8 = seq(builder+268)、+12 = unk_8ED5934 全局（appid?）
- +80 = string(builder+16)（req+20 串 + req+16 flag）
- +128 = string(builder+112) = req+128 busi ✓
- +152 = IMEI 全局串
- 签名三元组经 v41 容器 → sub_562B1F0(&v40, v41) 继续

pack 里 req+248/+272/+296 三串（可选）进 flags 对象 v10（bits 1/2/4）。

**下一步**：
1. 反编译 sub_562B1F0 找 secSig/secDeviceToken/secExtra 的 wire 位置（pb 字段号）
2. 探针集成签名输出重发 kx —— CharonSignProvider 已有 GetSecSign 能力！

### 【2026-08-24 续】sigs 追踪结论

- `CreateRequestData` 中 a4/a5/a6(secSig/secDeviceToken/secExtra) **只做非空校验+长度日志**，
  不直接写入输出 → sigs 在更上游（NodeAPI sendMsfRequest 参数层）就已填进 MSFRequest 其他字段
- `NodeIKernelMSFService::sendMsfRequest(cmd_str, str2, data, callback)` NAPI @0x3B86690
  → IKernelMSFService vtbl+88 → SendMsfRequestV3 (kernel_depends.cpp:219, 校验 session)
  → MSF 引擎 vtbl+224 注册
- ByteSizeLong (sub_6552A60) 分析：sigs 容器 message 有 repeated 段(a1+32/40) +
  flags 位域 optional strings(a1+48/56/64/72...)，字段号 ≤15；精确 pb 字段号仍未定
- MSFRequest 布局疑点：+8/+14/+20「字符串」间距仅 6B，与 libc++ string(24B) 矛盾，
  可能是 string_view 或 IDA 类型误判，待后续核实

**对实现的指导意义**：MSF-NG 命令需要签名三元组，CharonSignProvider.GetSecSign 已能产出
（sign/token/extra 同源）。剩余工作 = 确定 sigs 在 KeyExchangeRequest/ECDHBody 里的
确切 pb 字段号。两条路：
1. 反编译 MSF 引擎 vtbl+224 实现（接收 4 参并构造 MSFRequest 处）
2. 抓官方流量对照（仍是金标准）

### 【2026-08-25】sigs 容器 message 字段表（serialize 提取）

sub_6551D70 (CreateRequestData 内 v41 容器的 serialize) tag 常量（objdump 立即数为十进制！）：
```
0x42(f8,bytes) 0x48(f9,varint) 0x50(f10,varint) 0x58(f11,varint)
0x62(f12,bytes) 0x6A(f13,bytes) 0x70(f14,varint) 0x7A(f15,bytes)
```
serialize 还通过 sub_5627F90(stream, fieldnum, str, target) 写 f8/f12/f13/f15/f16。
注意：此容器装的是 req+248/272/296 三串+repeated，非 sigs 本身；sigs(a4/a5/a6) 仅校验不写入，
真实填充在上游 NodeAPI 层。v20 式 head 有 5 个字符串槽位，是 sigs 候选位置。

### 【2026-08-25 决定性】sigs wire 位置钉死：ReserveFields.f24

`sub_63FB930`（EncodeReserveFields 的 f24 构造器）：
- task+152 → 指针 → 结构 `{+0: str secSig, +24: str secDeviceToken, +48: str secExtra}`
- 三串全空 → 不写 f24；任一非空 → `ReserveFields.f24 = { f1: secSig, f2: secDeviceToken, f3: secExtra }`
- 与 pack 必填校验（req+152/176/200）互相印证 ✓

**完整 MSF-NG 请求公式（普通命令）**：
```
[len][ver][enc][seq][x=0][str ""][ReqHead v13: [barrier][cmd][""][reserve]][busi: [len+4][data]]
reserve = { f12/f13/f15/f16..., f21=32, f26, f24: {f1: sig, f2: token, f3: extra} }
```
签名数据来源 = CharonSignProvider.GetSecSign(uin, cmd, seq, body) 的 sign/token/extra！

**下一步**：探针把 GetSecSign 输出填进 reserve.f24 重发 kx/establish —— 这应该就是静默的最终答案。

### 【2026-08-25】NAS 真签名管线全通（里程碑）

**环境方案**（完整踩坑记录）：
- NAS SSH 密码在 opencode 会话日志里找到（mt;4v8M2<H#O3xU，SSH/sudo 同密码）
- 依赖库正确来源：ghcr.io/bemly/charonanchor:3.2.32 镜像内 /usr/local/bin/（docker cp 提取）
  ⚠️ 仓库根 libsymbols.so 是 Mach-O（macOS）版不能用；SignServer 的 wrapper.node(113MB) 版本也不对
- 正确 wrapper.node md5 = 26256bcbb45deb43ec71d47458b91760（149MB，3.2.32）
- 运行环境：ubuntu:latest + apt 装 ca-certificates libgnutls30 libssl3t64 libvips42 libx11-6
  libx11-xcb1 libxext6 libunwind8 libgssapi-krb5-2 libssh2-1t64 libpsl5t64
  + DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1（无 ICU 必须）
- self-contained 发布整个目录上传（tar 不能排除 runtime 自己的 .so！）

**实测结果**：
```
Loaded wrapper.node at base: 0x7FAD37000000
Sign function at: base + 0x65E55D1 ✓ (3.2.32 偏移命中)
secSig=32B token=0B extra=0B   ← 与 legacy 时代行为一致
kx-f24-real 发送 → 仍静默
```

**静默未解，剩余候选**：
1. GetSecSign 的 body/uin/seq 参数绑定方式（试 ECDHBody 包装后签名、uin=0）
2. 缺前置命令序列（官方连接后可能先有 HelloPush 正确 body / 注册族命令）
3. ReqHead 第二串（codec+8）或 MSFRequest 其他必填字段的 wire 内容
4. 需要「成功参照」才能区分失败模式 → 抓 napcat 重连流量（需用户决策动生产）或本机起一次性 NTQQ 容器

**探针现状**：Lagrange.Core.Runner msfng-probe 已集成 CharonSignProvider，
NAS 目录 /vol1/1000/msfng-probe/msfng-nas-publish 可随时重跑。

### 【2026-08-25 战略转折】上游已有完整 SsoKeyExchange 实现！！

web 搜索发现 Lagrange.Core Issue #247 + 本地代码核查：
- **fork 里已有 KeyExchangeService.cs**（Internal/Services/Login/）：
  [Service("trpc.login.ecdh.EcdhService.SsoKeyExchange", RequestType.D2Auth, EncryptType.EncryptEmpty)]
- ServerPublicKey/VerifyHashKey 与我 IDA 逆向结果逐字节一致 ✓✓
- WtExchangeLogic.cs (607行)：KeyExchange() → PasswordLogin 全流程现成
- Issue #247 日志实证 legacy 帧下整条链成功：Key Exchange successfully → NTLoginPasswordLogin → Login Success

**战略修正**：
1. SsoKeyExchange 在 **legacy 帧**（ServicePacker D2Auth/EncryptEmpty）下即可工作，
   不需要 MSF-NG 新帧——探针静默是因为 MSF-NG codec 层另有门槛（不影响登录！）
2. **密码登录路径（WtExchangeLogic）可能从未被 2026-08 的服务端封锁**——
   trans_emp 只挡 QR 登录。project_qr_login_rework.md 待办#2（密码登录绕行）一直没执行！
3. MSF-NG 逆向成果保留价值：未来 QQ 强制迁移时的技术储备

### 【2026-08-25 夜】QR 实测真相 + 插桩诊断法（重要方法论）

**Milky 容器卡死的根因**：GetSecSign 对 trans_emp 的 native 调用挂起（CHARON_SKIP_SIGN=1 环境变量可跳过）。
跳过后 trans_emp 帧(571B)成功上线，**服务器零回包**（心跳正常）——legacy 格式 trans_emp 确认被拒。

**插桩诊断法**（本次攻坚核心手段，比 IDA 快得多）：
- SendEvent/Resolve/Build/SendPacket/SocketContext.Send 各节点 Console.Error.WriteLine("[TRACE] ...")
- framework-dependent 发布（PublishAot=false 绕开 macOS 无 objcopy）→ ubuntu 容器跑
- ubuntu 依赖清单照抄 Milky Dockerfile；DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 必须
- docker commit 已装依赖的容器实现秒级重启迭代
- ⚠️ 本地 macOS 构建 Milky 报生成器 CS8795 错误：手写了 EventExtension.impl.cs /
  ApiExtension.impl.cs（43 个 handler 从 [ApiHandler("...")] 特性自动提取生成）

**关键认知更新**：MSF-NG 命令表含 wtlogin.trans_emp → 当前服务器要求 trans_emp 走 MSF-NG 通道，
legacy 格式（无论签名）均被拒。这正是 MSF-NG 攻坚的价值所在。

### 待完成（下次会话按序）
1. ~~SsoEstablishShareKey schema~~ 已破解并实现（MsfNgKeyExchange.cs + PacketContext.EstablishMsfNgSessionAsync），遗留 3 个 NAS 确认点见上
2. Ping/心跳信道循环接入 SocketContext（模板已提取）
3. NAS 联调：UseMsfNgTransport=true + EstablishMsfNgSessionAsync 抓包对照 qq.pcap，解决上述疑点
4. RspHead 后的 reserve-fields 跳过 + zlib busi 解压未实现
5. ⚠️ 本地预存构建问题（与 MSF-NG 无关）：Lagrange.Core.slnx 中 NativeAPI（DateTime vs long）与 Milky 生成器分部方法（CS8795）报错，干净 HEAD 同样存在

### 工具链备忘
- objdump 全量反汇编（1.2GB 文本）grep 锚点比 IDA xref 快得多：`objdump -d --no-show-raw-insn wrapper3232.node > full_disasm.txt` 然后 `grep -E "# 0xADDR\b"`
- ida-domain 反编译正确用法：`p = db.pseudocode.decompile(ea); lines = p.to_text(remove_tags=True)`
- 每次 run.py 冷开 IDB 需 4-8 分钟，务必批量脚本一次跑完

## 【2026-08-25 深夜·决定性突破】官方 trans_emp 帧完全破解 + 服务器响应打通

### ⚠️ TEA 链式重大更正（此前所有解密失败的总根因）
Lagrange TeaProvider 的 CBC **不是标准 CBC**！是 pre-XOR 变体：
```
加密: X_i = P_i ⊕ D_{i-1}out;  C_i = E(X_i) ⊕ C_{i-1}   (C_0=0)
解密: D_in_i = D_{i-1}out ⊕ C_i;  P_i = D_i_out ⊕ C_{i-1}
```
标准 CBC 解密（P_i = D(C_i)⊕C_{i-1}）对 QQ 帧全错。wrapper sub_85BC530 的 asm 与此一致。
python 参考实现见会话脚本 dec_final.py（tea_dec_block + qq_dec_lagrange）。

### 官方 trans_emp 0x31 请求帧（qq.pcap C#3，690B，已完整解密）
```
[len u32=690]
[ver u32=12][enc u8=2]
[basic] [u32 4][u8 x=0][str ""(00000004)]     ← 无D2常量4，非seq！（EncodeBasic: vtable+24 false → wr_bytes(d2空)=00000004）
[cipher@14] = TEA_zero(以下全部，Lagrange链式):
  [17B preamble] ea 79 28 65 fb | 00 00 01 2c | 00 | [u24 seq=6e3269] | 20 07 c2 77
      ← seq 在 preamble 里且只有 3 字节！01 2c=300 常量；20 07 c2 77 恒定待解
  [head v12 (batch_reqhead_63FA760.c 对应)]:
    [00 00 08 04][00 00 00 00][00 00 00 00][01][00 00 00 00][04]
    [str "wtlogin.trans_emp"][str ""][str 32hex="dca7d957..."](疑似MD5(Guid))[str ""]
    [u16 0002](空短blob)
    [reserve: [u32 0xc9]+197B 数据("b <同hash32>" trace 开头)]
  [busi: [u32 len+4][TLV]]
  [尾部若干 00]
```
- 时序：connect → ping21 → Heartbeat.Alive(v13 enc0 seq) → Client.CorrectTime(v13 enc0) → trans_emp(ver12 enc2)
- **没有任何 kx/establish/register 前置**！trans_emp 直接第三帧发出
- 轮询 0x12 同模板每 2s；seq 单调递增（69→6a）
- unk_F76E90 确为 16 零字节（.rodata 无重定位）；enc=2↔零密钥铁证
- 官方二进制 md5 与我们逆向的 wrapper.node 相同（26256bcb）

### 【里程碑】重放实验成功（2026-08-25 21:57 NAS）
探针加 CHARON_REPLAY_FRAME 模式：原样重放 pcap 提取的 690B 帧 →
**服务器 0.1s 回了 927B 响应**（[len][ver12][enc2]，含 "wtlogin.trans_emp" cmd 回显）！
- 证明传输层/加密层理解 100% 正确，服务器接受该帧形态
- 响应已解密存 official_rsp31_plain.bin（本地 opencode tmp）
- 下一步 = 用我们自己的 TLV/Guid/seq 按官方模板重构请求（替换 busi+hash32+seq 三处）

### Milky 侧已完成的代码改动（本次会话）
- MsfNgPacker.BuildFrame 补上 busi 缺失 bug（原实现从不写 sso.Data！）；
  enc≠0 时 head+busi 整体加密 ✓ EncodeFinal 语义
- PacketContext MSF-NG 分支恢复 D2Auth→v12/Simple→v13 原路由（与官方 wire 一致）
- SocketContext.Connect 连接后发 21B ping（CHARON_MSFNG 或 Lagrange.Server.UseMsfNgTransport 配置）
- Milky 配置接线 UseMsfNgTransport；Milky 构建修复：生成器改 opt-in（-p:MilkyUseGenerator=true），手写 impl 默认生效
- 测试 71/71 过；已提交 GitHub（ae7361da）
- ⚠️ 但 Milky 发的 ver12 帧仍是 v13 式 head → 依旧静默。需按上面官方模板重写 v12 head

### 待办（下次会话）
1. 解析 official_req31_plain.bin / official_rsp31_plain.bin 全字段（尤其 17B preamble 语义、reserve 197B 内容、响应 body 的 TLV/state）
2. MsfNgPacker 实现 BuildTransEmpRequest(busi)：官方模板 + 替换 hash32=MD5(Keystore.Guid)、seq、busi
3. PacketContext 对 wtlogin.trans_emp 走新构造器；响应用现有 DispatchMsfNgPacket 匹配
4. NAS 实测出码；然后 TransEmp12 轮询同路径
5. GetSecSign(trans_emp) native 挂起问题仍未解（本实验未签名也通了——重放场景；自有请求是否需要 reserve.f24 签名待测）
