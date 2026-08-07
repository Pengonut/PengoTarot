# PengoTarot

杀戮尖塔2（Slay the Spire 2）的 Mod，添加 **44 张全新塔罗牌** 与 **40 个全新附魔**。

基于 Godot 4.5.1 + C#（net9.0）开发，通过 Harmony 补丁实现，支持多版本游戏 API（0.107.0 / 0.110.0）。

## ✨ 功能特色

- **44 张全新塔罗牌**
- **40 个全新附魔**
- **卡牌视觉特效系统**（balatroeffect）：Foil / 负片 / 多彩 / 镭射 / 各向异性虹光 / VHS / CRT 等 shader 特效，以及 3D 透视倾斜
- **占卜标记系统**：地图节点标记、6 种标记占卜战斗效果、塔罗奖励
- **塔罗卡包商人机制**：商店购买与多人购买同步
- **多人联机同步**：卡牌 / 附魔运行逻辑实时同步
- **多语言本地化**：简体中文 / 日本語 / 한국어 / English

## 📦 安装

> 从 [Releases](https://github.com/Pengonut/PengoTarot/releases) 下载最新版本。

1. 下载 Release 中的 `PengoTarot.pck` 与对应版本的 `PengoTarot.dll`
2. 解压到游戏的 Mod 目录（`SlayTheSpire2/mods/` 下）
3. 启动游戏，在 Mod 列表中启用 PengoTarot

## 🔧 开发与构建

```powershell
# 单版本构建（0.107.0 或 0.110.0）
dotnet build -c Release /p:Sts2ApiCompat=0.107.0

# 完整打包（构建 Loader + 两个版本 DLL + 生成 manifest）
powershell -ExecutionPolicy Bypass -File pack.ps1

# 工具脚本
python tools/<脚本名>   # 用法见 tools/README.md
```

构建产物输出到 `dist/PengoTarot/`，`.pck` 需在 Godot 编辑器中导出。

## 🏗️ 目录结构

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

## 📄 许可证

本项目使用 [MIT License](LICENSE)。
