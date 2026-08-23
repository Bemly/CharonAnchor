---
name: ida-analysis-methodology
description: 如何使用 IDA Pro 分析 wrapper.node 签名模块
type: reference
---

## wrapper.node IDA 逆向分析方法

### 提取 wrapper.node
```bash
cd /tmp/qq_extract
python3 << 'PYEOF'  # 从 deb 提取 data.tar.xz
with open('linuxqq_3.2.28-48517_amd64.deb', 'rb') as f:
    # 跳过 ar header + debian-binary + control.tar.gz
    # ... 提取 data.tar.xz ...
PYEOF
tar xf data.tar.xz ./opt/QQ/resources/app/wrapper.node
```

### IDA Domain 分析
```bash
export IDADIR="/Applications/IDA Professional 9.3.app/Contents/MacOS"
SKILL_DIR="/Users/bemly/.claude/plugins/marketplaces/ida-claude-plugins/plugins/code-eval-ida-domain/skills/ida-domain-scripting"
cd "$SKILL_DIR" && uv run python run.py script.py -f wrapper.node --timeout 0
```

### 已知关键地址
| 函数 | 偏移 | 大小 | 说明 |
|------|------|------|------|
| sub_5BD3EA1 | 0x5BD3EA1 | 1405 bytes | 签名函数 (3.2.29-260528)，调用 dladdr+strstr |
| sub_57E1131 | 0x57E1131 | 1405 bytes | 签名函数 (3.2.28-48517)，调用 dladdr+strstr |
| sub_57D794A | 0x57D794A | ~100 bytes | 初始化，返回 &unk_7EFE9D8 |
| sub_57CD620 | 0x57CD620 | - | 计算 sign (MD5) |
| sub_57CBF68 | 0x57CBF68 | - | 计算 extra |
| sub_57BD1A4 | 0x57BD1A4 | - | 计算 token |

### 133MB 二进制注意事项
- IDA 加载需 10+ 分钟，用 `--timeout 0`
- 使用缓存 `.i64` 文件避免重复分析
- 优先用 indexed access 而非遍历全部函数/字符串
