---
description: "开始一次 PengoTarot 会话：加载项目约定（AGENTS.md）与仓库记忆，按主题加载专项知识（balatroeffect skill / 游戏源码 / RitsuLib），确认双版本构建机制后进入具体任务"
name: "开始对话"
argument-hint: "本次要处理的主题，如：新增附魔、修改卡牌、调试特效、构建打包"
agent: "agent"
---

# 开始对话 — PengoTarot 会话初始化

在开始任何工作之前，先完成以下上下文初始化（保持简洁，不要长篇复述）：

## 1. 加载项目约定

- 读取项目根 [AGENTS.md](../../AGENTS.md)，掌握：构建命令、多版本兼容机制（`Sts2ApiCompat` + 条件编译符号）、目录结构、关键陷阱（Loader 脚本注册、csproj 排除项、.tscn Unicode、资源打包、本地化、多人同步、Harmony 惯例）
- 记住：代码库使用中文注释/文档，**回复与代码注释保持中文**

## 2. 加载仓库记忆

- 读取仓库记忆 `/memories/repo/PengoTarot.md`，注意开发笔记（已知版本差异、资源依赖步骤、未完成任务等）

## 3. 按主题加载专项知识

若提供了主题（${input:topic:未指定}），按需加载对应知识（不相关则跳过）：
- 卡牌视觉特效（shader / Tilt / 检查界面 / 效果注册）→ 调用 `balatroeffect` skill：`.github/skills/balatroeffect/SKILL.md`（注意 `balatroeffect/ARCHITECTURE.md` 部分过时，以 skill 为准）
- 游戏 API / 多版本签名差异 → 对照参考源码 `d:\[Tool] Godot\STS2v0.107\` 与 `d:\[Tool] Godot\STS2v0.109\`（只读，勿改）
- 依赖框架 API → `d:\[Download] Edge\STS2-RitsuLib-0.4.62\`（README.md 与 docs/）

## 4. 输出会话摘要（一段话即可）

确认以下要点（不要展开长文）：
- 项目：PengoTarot（44 塔罗牌 + 40 附魔，Godot 4.5.1 + C# net9.0 + Harmony 补丁）
- 目标 API：0.107.0 / 0.110.0（双版本变体 DLL + Loader 运行时选择）
- 构建：`dotnet build -c Release /p:Sts2ApiCompat=<版本>`；完整打包用 `pack.ps1`

## 5. 进入任务

- 如果用户提供了主题，先简要复述你对任务的理解，确认具体范围/目标后再动手
- 如果未提供主题，主动询问用户本次要处理的具体任务
