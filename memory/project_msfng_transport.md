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
