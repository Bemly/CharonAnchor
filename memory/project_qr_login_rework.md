---
name: qr-login-rework
description: 2026-08 QQ 3.2.32 登录流程重构逆向 — trans_emp 静默丢弃的根因与新包结构调查
type: project
---

## 登录流程重构结论（2026-08-22 逆向）

**根因确认：腾讯在 3.2.32 重构了 QR 登录协议，服务端静默丢弃旧格式 trans_emp 包。**

### 证据链
1. 新旧镜像（5月能登录的 3.2.29 生产镜像）今天同样卡死 → 服务端行为，非代码回归
2. 探针实证：签名正常（sign=32B token=0 extra=0）→ 611B 帧发出 → 无响应；心跳正常
3. 命令未变：新旧 wrapper.node 都含 `wtlogin.trans_emp`（`wtlogin64.*` 家族两版都有，非新增）
4. **旧 deb 已从腾讯 CDN 下架**（NoSuchKey），强制升级模式

### wrapper.node 内登录架构重构（登录逻辑就在 wrapper.node！）
```
旧 3.2.29：wtlogin_codec.cc（单体） + qrcode/qrcode_login_codec.cc
新 3.2.32：codec/login_codec.cc（重构）
      + base/base_login_mgr.cc、password_nt/password_nt_mgr.cc
      + sms/sms_login_mgr.cc、unusual_device/unusual_device_mgr.cc【新】
      + util/nt_login_sig_util.cc、third_party_sig/codec/exchange_sig_codec.cc【新】
```

### 新版流程关键点
- `UnusualDeviceMgr::SendQRCode31Request` 接管 QR 0x31（重试上限 30 次）
- 新 TLV 类 `nt::login::QRCodeTlv_63`（仅新版有；类名数字为十进制 → tag 0x3F）
- 官方 QRCodeTlv 类族（十进制→hex）：_17→0x11、_22→0x16、_102→0x66、_209→0xD1 与 Lagrange 现有重叠；_20/_23/_24/_25/_101/_104（0x14/0x17/0x18/0x19/0x65/0x68）为 Lagrange 缺失；**_63→0x3F 全新**
- 响应新增解析：qr_sig/qr_url/expire/push_result/fail_tips；0x12 阶段 a1_sig/a1_key/no_pic_sig/tgt_qr/**one_click_succeed_sig**
- ProcessLoginResponse 新增：new_device_check_sig/unusual_device_check_sig/unusual_device_qr_sig/uin_token/**skip_old_qr**（服务端可强制跳过旧 QR 流程的字段！）
- AuthNewDevice Request/Response 编解码新增

### 关键地址（3.2.32 wrapper.node，IDA 缓存已建）
| 函数 | 地址 | 说明 |
|------|------|------|
| DoOnQuickLoginResponse | 0x340C270 | UnusualDeviceMgr 0x31 响应处理（unusual_device_mgr.cc:177）|
| DoOnPollingResponse | 0x340CB10 | 0x12 轮询响应处理（unusual_device_mgr.cc:308/324）|
| SendWTLoginTransPayload | 0x340D820 | wtlogin 载荷通用发送器（unusual_device_mgr.cc:381）|
| SendQRCode31 发送器 | 0x340D330 | 重试上限 30 次；构建后经 SendWTLoginTransPayload(a1,&payload,**2**) 发送 |
| 0x12 轮询发送器 | 0x340B7E0 | SendWTLoginTransPayload 的另一调用者 |
| 载荷构建链 | 0x3410220 → 0x33853B0 | 薄封装 → 真正序列化器 |
| 0x12 响应解析链 | 0x34103A0 → 0x33856A0 | 薄封装 → 真正解析器 |
| 大端写助手 | 0x85F1E80(u16)/0x85F20D0(u32)/0x85F2030(u8)/0x85F2200(q64)/0x85F24E0/0x85F2810 | flag=1 时 ROL2 大端转换 |
| 大端读助手 | 0x85F2AD0(u8)/0x85F2C50(u16)/0x85F2FF0(u32)/0x85F3330(buffer) | |

### 【已破解】wrapper 内部载荷表示（⚠️ 非最终线格式！）
```
offset 0:     byte 0x02                起始标记
offset 1:     u16 BE 总长度            （构建完成后由 0x85F1FB0 回填）
payload:      组A字段(sub_3387CA0) + 组B(0x3387ED0) + 组C含vector<u16>(0x337C170)
末尾:         byte 0x03                结束标记
```
- 解析端（sub_33856A0）对称校验：首字节==2、头内 u16==18、尾字节==3
- ⚠️ **修正（2026-08-22 深夜）**：此结构经 SendWTLoginTransPayload 异步派发（sub_88C67D0 任务队列），消费者未定位到——它是 wrapper 内部表示，**发送到网络的最终 wire 格式是否就是这个、还是被传输层再包装成经典 TLV，尚未验证**
- 四个引用 "wtlogin.trans_emp" 的大函数（两版字节数相同）实为**命令名表函数**（返回字符串数组给 JS 层，见 sub_3437830），不是编解码器——此前判断有误已纠正
- 确定线格式的唯一可靠途径：**动态分析**——运行官方 QQ 3.2.32 客户端并抓登录时的 socket 流量对比

### 决定性实验（2026-08-23 深夜）：官方请求体 + 旧传输 = 仍然被丢弃
将 gdb 转储的官方 249B 0x31 请求体**逐字节原样**通过 Lagrange 现有传输（SSO 帧+ECDH_ST+GetSecSign）发送：
- 帧成功发出（499B），心跳照常回应，但 trans_emp **依旧零响应**
- **结论：仅改 body 格式不够，服务端强制要求 MSF-NG 新传输层会话**。完整适配必须实现：
  1. MSF 握手（21B ping：`[len=21][01]["3R9\0"][00000004]["MSF"][05...]` 含 uin 派生字段，见 tcp_channel_connector.cc BuildPingPacket:156 / StartPing:188）
  2. 会话密钥建立（"ECDH Key V2"，HKDF+SHA256 栈，细节待逆向）
  3. type=0x0C 帧加密（AES 类，参数待逆向）
- 已捕获资产：官方 249B/89B 请求体模板（`Lagrange.Core/Internal/Packets/Login/WtLoginNewTemplates.cs`）、NAS `/tmp/qqdyn/qq.pcap`（1MB 完整会话含握手）

### 动态抓包定论（2026-08-23，官方 3.2.32 客户端 vs Lagrange 同 NAS 同 IP）
官方客户端在 Docker+xvfb 下成功运行并进入 QR 轮询状态：
| 阶段 | 官方（能工作） | Lagrange（被丢弃） |
|------|------|------|
| 握手 | 21B→25B | 类似 |
| trans_emp 0x31 | **690B → 响应 77+927B** | 611B → 无响应 |
| 0x12 轮询 | **530B ↔ 183B 每 2 秒** | 从未到达 |

**决定性结论：**
1. **排除风控/IP 因素** —— 官方客户端在同一台 NAS、同一 IP 上正常完成 QR 流程
2. **新传输协议实锤**：官方 8080 流量中 `wtlogin` 明文出现次数为 **0**（pcap strings 全扫）；帧形态 `[u32 len][u32 type=12][加密blob]`；Lagrange 的经典 SSO 帧含明文命令名 → 服务端对新格式才响应
3. 修复 = 实现**新传输层+协议**（不只是包内容差异），工程量大；pcap 样本存于 NAS `/tmp/qqdyn/qq.pcap`（1MB）
4. 官方客户端容器化方法已验证：ubuntu:24.04 + xvfb + gtk3 等依赖，`xvfb-run ./qq --no-sandbox`，XAUTHORITY 指向 /tmp/xvfb-run.*/Xauthority；镜像 qq-official 在 NAS

### 旧版对照记录
- 旧版 0x31 TLV 集（从旧二进制 RTTI 对象提取）：{17,22,27,29,51,53,102,209}（十进制）={0x11,0x16,0x1B,0x1D,0x33,0x35,0x66,0xD1} —— 与 Lagrange 实现完全一致 ✓
- 四个核心编解码大函数两版字节数完全相同（6491/5411/9220/12650）→ 底层未变
- 变化全在外层：请求组装从"TLV 对象列表"换成"平铺定长结构 + 标记字节"

### 修复路径（按可行性排序）
1. **实现 MSF-NG 传输层**（核心剩余工作）：从 msf_session_impl.cc / tcp_channel_connector.cc 逆向 ECDH Key V2（曲线/KDF/密钥协商时机）与帧加密算法（定位锚点：字符串 "Generate ECDH Key V2 Succeed" @VA 0x87D2B6、"reKey to no aes key"、"ECDH pub key info: cipher_ver:{}, key_ver:{}"；注意 gdb 动态调试会被 wrapper 反调试掐断发送步骤——**只能静态逆向或磁盘补丁**）
   - **⚠️ 2026-08-24 更正**："reKey to no aes key" 是 MMKV 存储加密的日志，不是传输层的！ECDH Key V2 与帧加密已破解，见 [project_msfng_transport.md](project_msfng_transport.md)
2. **密码登录绕行测试**：config 设 Password，走 wtlogin.login/SsoNTLogin 路径；但若 wtlogin 命令全部强制走 MSF-NG 会话则同样被阻
3. **等社区**：协议变更影响所有纯协议第三方实现（NapCat 用官方二进制不受影响）；上游适配后对照移植
4. 已完成部分可直接复用：新信封 body 模板、TLV 集合验证、传输帧格式、完整地址表
