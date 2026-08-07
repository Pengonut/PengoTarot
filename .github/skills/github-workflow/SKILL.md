---
name: github-workflow
description: 'PengoTarot 的 GitHub 提交/更新/发布流程：仓库 Pengonut/PengoTarot（公开、MIT、main 分支、origin 已配置）、日常提交推送（VS Code 图形化优先，终端 git add+commit+push）、commit message 中文规范与 PowerShell 5.1 乱码规避（-F UTF-8 文件 / 先设 Console 输出编码）、.gitignore 排除清单（.venv/dist/*.pck/*.bak/*.psd 等）与必须提交项（.github/、*.uid、export_presets.cfg）、路径含 [Tool] 方括号的 PowerShell 坑（-LiteralPath / git -C）、Release 发布（玩家下载走 Steam 创意工坊，GitHub 仅代码开源，勿分发 pck/dll）。Use when: 帮用户提交/推送代码、检查或完善 .gitignore、准备 Release、排查 git 中文乱码或路径报错。'
argument-hint: '如：帮我提交今天的改动并推送、检查 .gitignore 有没有漏掉大文件、提醒我发布 v1.0 的 Release'
user-invocable: true
---

# github-workflow — PengoTarot 的 GitHub 提交与更新

PengoTarot 的 GitHub 仓库：**Pengonut/PengoTarot**（公开，MIT 协议）。玩家**下载走 Steam 创意工坊**，GitHub 仅用于**代码开源分享**（README 已明确不提供下载）。日常维护 = 改代码 → 提交 → 推送 →（可选）打 tag 发 Release。

> ⚠️ 改 README 前先想清楚定位：GitHub 是源码仓库不是下载站，别在 README 放玩家下载/安装步骤，要引导玩家去创意工坊。

## 何时使用

- 帮用户提交 / 推送代码改动
- 检查、完善 `.gitignore`（防止大文件/构建产物进版本库）
- 准备 Release 版本（打包、tag、发布说明）
- 排查 git 中文乱码、路径含方括号报错、认证问题

## 仓库基本盘

| 项 | 值 |
|---|---|
| 远程 | `origin` = `https://github.com/Pengonut/PengoTarot.git` |
| 分支 | `main`（本地与远程同名，已设上游跟踪） |
| 身份 | git 全局已配 `user.name=Pengonut` / `user.email=Pengonut@users.noreply.github.com` |
| 认证 | Git Credential Manager（HTTPS）。首次 push 会弹系统/浏览器登录，**必须用户本人操作**，助手无法代办 |
| 许可证 | MIT（`LICENSE`） |

## 提交与推送流程

1. 查看改动：`git -C "<仓库>" status --short`（本地 HEAD 与 `origin/main` 一致即已同步）
2. 提交：**优先让用户在 VS Code「源代码管理」面板**填中文说明后提交（图形界面中文不会乱码，最适合新手）
3. 终端方式：`git add .` → `git commit -m "..."` → `git push`
4. 推送后验证：`git status --short --branch` 显示 `## main...origin/main`（无 ahead/behind）即成功

## 关键坑（必读）

- **PowerShell 5.1 中文乱码**：`git commit -m "中文"` 会把参数按 GBK 编码传给 git，commit message 变乱码。规避：
  - 优先让用户在 VS Code 图形界面提交
  - 或把 message 写入 **UTF-8 文件** 再 `git commit -F <file>`（用 create_file 生成）
  - 查看 git 中文输出前先设 `[Console]::OutputEncoding = [System.Text.Encoding]::UTF8`
- **路径含 `[Tool]` 方括号**：PowerShell 把 `[` `]` 当通配符，`Set-Location`/`Get-Item` 会失败。改用 `Set-Location -LiteralPath` 或 `git -C "..."`；终端里 `Get-Item` 对含方括号路径还有 5.1 已知 bug，统计文件大小用 Python 更稳。
- **`.gitignore` 关键项**（已配置好，勿删）：
  - 排除：`.godot/`、`.venv/`、`dist/`、`bin/ obj/`、`*.pck`、`*.zip`、`*.psd`、`*.bak`、`*.orig`、`*~`、`*.log`、`.idea/`、`.vscode/`
  - **必须提交**：`.github/`（技能文档 SKILL.md）、`*.uid`、`export_presets.cfg`、`project.godot`、`*.tscn`、`*.gdshader`
- **备份文件一律不提交**：`.cs.bak`、`*.orig` 等（版本库本身就是备份；历史里的垃圾文件极难清除）。
- **提交前查大文件**：GitHub 单文件上限 100MB（>50MB 有警告）。用 Python 脚本统计 `git ls-files --others --exclude-standard` 的总大小与最大文件。
- **CRLF 警告无害**：`.gitattributes` 的 `* text=auto eol=lf` 会让 git 提示"CRLF 将被替换为 LF"，属正常规范化，忽略即可。

## Release 发布（提醒用户操作）

- 定位：玩家下载走 **Steam 创意工坊**；GitHub Release 主要用于源码 tag 与版本里程碑，**不要**把 `pck/dll` 挂到 Releases 误导玩家在此下载。
- 流程：本地 `pack.ps1` 打包（产物在 `dist/`，勿提交）→ 网页 `Create a new release` → 填版本号（如 `v1.0.0`）与更新说明 → 打 tag。
- 发布说明用中英双语、列出新增/修复要点。

## 新手沟通要点

- 解释要**图形化、步骤化**（VS Code 面板 / 网页），避免抽象概念堆砌。
- 登录、认证、在网页创建仓库等**必须用户本人完成**的步骤要明确告知，不要替他操作或索要密码/令牌。
- commit message 用中文、说明清楚本次改动；一个提交只做一件事。
