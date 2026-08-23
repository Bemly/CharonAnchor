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
[u32 BE connSeq]         客户端连接内递增计数（0x6238bf→c0→…，服务端响应回显此值）
[u8   x]                 观测=0
[ReqHead …]              版本相关的虚拟函数（vtable+32）
[BusiBuff …]             若 encFlag≠0 则整体 TEA 加密
```
服务端帧同构：seq 为服务端自己的计数器（0x39 起），多一个 echo=客户端 seq 的 u32。

### 编解码管线（codec_processor.cc）
```
EncodePacket @0x63F8BA0: EncodeBasic(0x63F9050) → EncodeReqHead(virtual vtbl+32)
                         → EncodeBusiBuff(0x63F9290) → EncodeFinal(0x63F9360: concat basic+(head+busi)，busi 按 encFlag TEA)
DecodePacket @0x63F9620: DecodeBasic(0x63F9D40) → DecryptBody(0x63FD870, encFlag≠0 时)
                         → DecodeRspHead(0x63F9E40) → DecodeReserveFields(vtbl+40) → DecodeBusiBuff(0x63F9FF0, 可 zlib 解压)
EncodeBasic 输出: [u32 len 占位回填][u32 ver][u8 enc][u32 seq][u8 x][cmd string]
reader 原语: rd_u32 不推进指针需显式 skip(4)；rd_back=绝对 seek；rd_str_a/b 读长度前缀字符串
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

### 待完成（下次会话按序）
1. ReqHead/RspHead 逐字段语义（用 pcap 样本 + 已知管线迭代验证最快，不必再啃 IDA）：
   客户端 head 区实测 `00 00 00 04 | 00 00 00 64`；body 区 `[u32 0x13]"Heartbeat.Alive"+[00000004][TLV 0x49]73B[protobuf tail][00000008][00000004]`
2. `SsoEstablishShareKey` 请求/响应 protobuf schema（DecodeECDHBody/ComputeShareKey 字符串 @0x8ca739/0x8ca763 附近；base_nonce @0x8ca175）
3. 21B Ping 包构造（BuildPingPacket tcp_channel_connector.cc:156 / StartPing:188）
4. C# 传输层：新建 MsfNgPacker 对位 ServicePacker/SsoPacker，SocketContext 策略切换点已勘察（IClientListener seam）
5. 联调：NAS 上对照 qq.pcap

### 工具链备忘
- objdump 全量反汇编（1.2GB 文本）grep 锚点比 IDA xref 快得多：`objdump -d --no-show-raw-insn wrapper3232.node > full_disasm.txt` 然后 `grep -E "# 0xADDR\b"`
- ida-domain 反编译正确用法：`p = db.pseudocode.decompile(ea); lines = p.to_text(remove_tags=True)`
- 每次 run.py 冷开 IDB 需 4-8 分钟，务必批量脚本一次跑完
