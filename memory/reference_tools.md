---
name: tools-environment
description: IDA 和 uv 工具路径
type: reference
---

## IDA Pro 9.3

路径：`/Applications/IDA Professional 9.3.app/Contents/MacOS/`

设置 IDADIR：
```bash
export IDADIR=/Applications/IDA Professional 9.3.app/Contents/MacOS
```

可执行文件：`ida`, `idat`

## uv 包管理器

路径：`/Users/bemly/.local/bin/uv`

添加到 PATH（fish shell）：
```fish
source $HOME/.local/bin/env.fish
```

添加到 PATH（bash/zsh）：
```bash
source $HOME/.local/bin/env
```

## .NET SDK（重要）

- 系统旧 SDK 10.0.203（Roslyn 5.3）**编不过 LagrangeV2 分支**：上游 Milky.Generator 需要 CodeAnalysis 5.6
- 新 SDK **10.0.400** 已装在 `~/.dotnet`，使用前：
  ```bash
  export DOTNET_ROOT=~/.dotnet PATH="$HOME/.dotnet:$PATH"
  ```
- 建议把上面这行写进 ~/.zshrc，否则 dotnet 命令仍解析到旧 SDK