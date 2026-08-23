---
name: git-workflow
description: 每次修改后自动提交到 GitHub
type: feedback
---

每次修改完成后，立即提交并推送到 GitHub。

**Why:** 用户希望改动及时同步，不想手动提醒。

**How to apply:** 完成文件修改后，执行 `git add` + `git commit` + `git push`。

---

## workflow 触发规则

只有最新版本的 workflow 可以 push 触发，旧版本的只能手动触发。

**Why:** 避免每次 push 触发多个版本的重复构建，节省 Actions 额度。

**How to apply:**
- 构建 package 的工作流优先级最高
- 当前最新是 3.2.29：`milky-build-3_2_29.yaml` = push + 手动，`milky-build-3_2_28.yaml` = 仅手动，`milky-build-3_2_19.yaml` = 仅手动
- 未来如果新增更高版本，新版本的设为 push 触发，旧版本改为仅手动