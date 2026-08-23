# CharonAnchor Memory

## 项目状态

- [Project Status](project_status.md) — Fork LagrangeV2 + CharonSignProvider 嵌入方案
- [Sign Function Analysis](project_sign_function_analysis.md) — sub_57E1131 完整逆向，DLFJ 检查逻辑，环境检测
- [QR Login Rework](project_qr_login_rework.md) — 3.2.32 登录流程重构逆向：trans_emp 静默丢弃根因、UnusualDeviceMgr、TLV 0x3F
- [MSF-NG Transport](project_msfng_transport.md) — MSF-NG 传输层逆向：ECDH Key V2（P-256+MD5）、帧格式、QQ TEA 加密、编解码管线

## 实现过程

- [Implementation Process](project_full_process.md) — P/Invoke wrapper.node 嵌入 Lagrange.Milky
- [New Version Analysis](project_new_version_analysis.md) — 签名函数逆向分析（偏移仍有效）
- [Detection Vulnerabilities](project_detection_vulnerabilities.md) — 协议层检测漏洞：ApkSignatureMd5、SdkBuildTime、Qimei、Tlv52D、keystore 持久化

## 关键经验

- [Key Learnings](feedback_key_learnings.md) — P/Invoke 嵌入方案的经验教训
- [Version Sync](feedback_version_sync.md) — BotAppInfo 版本必须与 wrapper.node 源码版本一致
- [README Style](feedback_readme_style.md) — 特殊命名风格保留规则
- [Git Workflow](feedback_git_workflow.md) — 每次修改后自动提交 GitHub
- [AOT Linux Only](feedback_docker_only.md) — AOT 编译需要在 Linux 环境

## 环境配置

- [Server Info](reference_server.md) — NAS 服务器连接和测试环境
- [Tools Environment](reference_tools.md) — IDA Pro 和 uv 工具路径配置
- [IDA Methodology](reference_ida_methodology.md) — wrapper.node IDA 逆向分析方法
