# CharonAnchor

## 项目记忆（迁移自 Claude Code）

本项目的长期记忆文件位于 `memory/` 目录，由 Claude Code 记忆（原路径
`~/.claude/projects/-Users-bemly-cchaha-CharonAnchor/memory/`）于 2026-08-22 迁移而来。

**注意：这些记忆是 78~114 天前的快照。引用其中的偏移量、版本号、路径等信息前，
必须先对照当前代码核实，不要直接采信。**

索引见 [memory/MEMORY.md](memory/MEMORY.md)，分类如下：

### 项目状态
- [project_status.md](memory/project_status.md) — Fork LagrangeV2 + CharonSignProvider 嵌入方案
- [project_sign_function_analysis.md](memory/project_sign_function_analysis.md) — sub_57E1131 完整逆向，DLFJ 检查逻辑，环境检测
- [project_qr_login_rework.md](memory/project_qr_login_rework.md) — 3.2.32 登录流程重构逆向：trans_emp 静默丢弃根因、UnusualDeviceMgr、TLV 0x3F

### 实现过程
- [project_full_process.md](memory/project_full_process.md) — P/Invoke wrapper.node 嵌入 Lagrange.Milky
- [project_new_version_analysis.md](memory/project_new_version_analysis.md) — 签名函数逆向分析（偏移仍有效）
- [project_detection_vulnerabilities.md](memory/project_detection_vulnerabilities.md) — 协议层检测漏洞：ApkSignatureMd5、SdkBuildTime、Qimei、Tlv52D、keystore 持久化

### 关键经验
- [feedback_key_learnings.md](memory/feedback_key_learnings.md) — P/Invoke 嵌入方案的经验教训
- [feedback_version_sync.md](memory/feedback_version_sync.md) — BotAppInfo 版本必须与 wrapper.node 源码版本一致
- [feedback_readme_style.md](memory/feedback_readme_style.md) — 特殊命名风格保留规则
- [feedback_git_workflow.md](memory/feedback_git_workflow.md) — 每次修改后自动提交 GitHub（用户偏好）
- [feedback_docker_only.md](memory/feedback_docker_only.md) — AOT 编译需要在 Linux 环境

### 环境配置
- [reference_server.md](memory/reference_server.md) — NAS 服务器连接和测试环境
- [reference_tools.md](memory/reference_tools.md) — IDA Pro 和 uv 工具路径配置
- [reference_ida_methodology.md](memory/reference_ida_methodology.md) — wrapper.node IDA 逆向分析方法

## 可用技能

已迁移到 opencode 全局技能目录 `~/.config/opencode/skills/`：

- `ida-domain-scripting` — IDA Domain API 逆向脚本。逆向 wrapper.node 时使用：

  ```
  cd ~/.config/opencode/skills/ida-domain-scripting && uv run python run.py script.py -f wrapper.node --timeout 0
  ```

  （首次运行 `uv` 会自动重建 `.venv`；API 细节见该目录下 `API_REFERENCE.md`）
- `frontend-design` — 前端视觉设计指导

Claude Code 内置的 `simplify`、`update-config` 技能无独立源文件，未迁移。
