# PengoTarot

杀戮尖塔2（Slay the Spire 2）Mod：**44 张塔罗牌 + 40 个附魔**。
A Slay the Spire 2 mod: **44 tarot cards + 40 enchantments**.

## 🎮 下载 Download

玩家请通过 **Steam 创意工坊** 订阅下载本 Mod，享受自动更新与反馈。
> Players: please subscribe via **Steam Workshop** for downloads & updates.

本仓库为 **MIT 开源代码**，不提供 Mod 下载。
> This repo hosts open-source code (MIT) only — no mod downloads.

## ✨ 功能 Features

- **44 张全新塔罗牌** / 44 brand-new tarot cards
- **40 个全新附魔** / 40 brand-new enchantments
- **卡牌视觉特效**：Foil / 负片 / 多彩 / 镭射 / VHS / CRT 等 shader 特效与 3D 倾斜 / card visual effects
- **占卜标记系统** / divination marker system
- **塔罗卡包商人** / tarot pack merchant
- **多人同步** / multiplayer sync
- **多语言**：简体中文 / 日本語 / 한국어 / English

## 🔧 面向开发者 For Developers

```powershell
# 单版本构建（0.107.0 或 0.111.0） / build for one API version
dotnet build -c Release /p:Sts2ApiCompat=0.107.0

# 完整打包（Loader + 两个版本 DLL + manifest） / full packaging
powershell -ExecutionPolicy Bypass -File pack.ps1
```

构建产物在 `dist/PengoTarot/`；`.pck` 需在 Godot 编辑器导出。

## 🏗️ 目录结构 Structure

| 目录 | 作用 |
|---|---|
| `src/` | 卡牌 / 附魔 / Power / 遗物模型 |
| `Patch/` | Harmony 补丁 |
| `balatroeffect/` | 卡牌视觉特效系统（shader + 3D 倾斜） |
| `loader/` | 多版本 DLL 加载器 |
| `network/` | 多人同步 |
| `local/` | 本地化 |
| `Data/` | 数据定义（TarotDef / TarotDeck 等） |
| `configFW/` | 难度配置浮动面板 |
| `tools/` | Python 资产生成脚本 |

## 📄 许可证 License

本项目使用 [MIT License](LICENSE)。
