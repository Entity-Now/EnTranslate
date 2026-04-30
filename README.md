# 🚀 EnTranslate - 让 Visual Studio 变身翻译神器的划词插件

[![Visual Studio Marketplace](https://img.shields.io/visual-studio-marketplace/v/Entity-Now.Translate?color=blue&label=Marketplace&logo=visual-studio)](https://marketplace.visualstudio.com/items?itemName=Entity-Now.Translate)
[![Downloads](https://img.shields.io/visual-studio-marketplace/d/Entity-Now.Translate?color=brightgreen)](https://marketplace.visualstudio.com/items?itemName=Entity-Now.Translate)
[![Stars](https://img.shields.io/github/stars/Entity-Now/EnTranslate?style=flat&color=yellow)](https://github.com/Entity-Now/EnTranslate/stargazers)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://opensource.org/licenses/MIT)

**EnTranslate** 是一款专为 Visual Studio 开发者打造的轻量级、智能划词翻译插件。无论是阅读源码、变量命名还是查阅文档注释，它都能让你告别切换浏览器的繁琐，留在代码编辑器中完成一切。

[立即从 VS Marketplace 下载](https://marketplace.visualstudio.com/items?itemName=Entity-Now.Translate) | [提交 Issue 反馈](https://github.com/Entity-Now/EnTranslate/issues)

---

## 💻 版本支持 (Support)

本插件紧跟微软开发者生态，目前已完美适配以下版本：
*   ✅ **Visual Studio 2026** (最新支持)
*   ✅ **Visual Studio 2022**

---

## ✨ 核心特性

| 功能 | 描述 |
| :--- | :--- |
| 🔍 **智能划词/悬停** | 鼠标只需悬停在单词或选中文本上，即刻呈现精准翻译。 |
| 📚 **340万+ 离线词库** | 内置超大规模离线词典，无网环境下也能秒速响应生僻单词。 |
| 🧩 **强大分词技术** | 完美支持 `CamelCase` (驼峰)、`snake_case` (下划线) 等各种编程命名拆分翻译。 |
| 📝 **注释深度支持** | **支持翻译文档注释**！彻底解决看源码时面对大段英文文档的焦虑。 |
| 🔊 **多源发音引擎** | 集成微软 Edge TTS、有道发音，提供地道、流畅的单词/句子朗读。 |
| ☁️ **云端翻译扩展** | 本地词库未命中时，自动触发 Bing、Google、DeepL 等在线接口补位。 |

---

## 📸 效果预览

### 🌑 黑色主题 - 沉浸式体验
> 完美适配 VS 深色模式，保护视力。
![黑色主题预览](https://cdn.jsdelivr.net/gh/Entity-Now/EnTranslate/docs/new_hover.png)

### 🌕 白色主题 - 清晰明亮
![白色主题预览](https://cdn.jsdelivr.net/gh/Entity-Now/EnTranslate/docs/white.png)

### ⚙️ 个性化配置
> 自由选择发音角色与翻译引擎。
![设置界面预览](https://cdn.jsdelivr.net/gh/Entity-Now/EnTranslate/docs/new_setting.png)

---

## 🛠 未来路线图 (Roadmap)

我们一直在进化！以下是正在开发或计划中的功能：

- [ ] **AI 增强翻译 (重点计划)**: 集成 OpenAI/DeepSeek 接口，提供基于代码上下文的“程序员专属”精准翻译。
- [ ] **变量名取名助手**: 输入中文直接替换为符合规范的英文变量名（命名从此不求人）。
- [ ] **一键生成文档注释**: 基于 AI 自动将代码逻辑翻译并生成规范的中文/英文 Doc 注释。
- [x] **架构大规模重构**: 提升插件响应速度与稳定性。
- [x] **支持 Visual Studio 2026**。

---

## ❤️ 致谢与参考

感谢以下开源项目为本插件提供的灵感与部分实现参考：

*   [Code Translate (VSCode)](https://github.com/w88975/code-translate-vscode)
*   [Multi-Supplier-MT-Plugin](https://github.com/JuchiaLu/Multi-Supplier-MT-Plugin)

---

## 🤝 贡献与支持

1.  **点亮 Star**: 如果你觉得这个插件提高了你的效率，请帮我点一个 **Star**，这是对我最大的鼓励！
2.  **提交问题**: 发现 Bug 或有新点子？欢迎 [提交 Issue](https://github.com/Entity-Now/EnTranslate/issues)。

---

## 📜 更新日志 (Changelog)

### 📅 2026.04.30 - 架构重构与体验升级
> 本次更新对插件核心进行了深度重构，显著提升了可维护性与 UI 智能度。

*   **核心解耦**：新增 `QuickInfoTextExtractor`、`QuickInfoUIBuilder`、`LegacyInfoService`、`VoiceService` 等独立类，逻辑更加清晰。
*   **智能提取**：优化文本提取逻辑，优先处理用户选区内容，并自动过滤纯中文干扰。
*   **智能 UI 展示**：支持“本地词典优先+远程接口兜底”策略；针对单词和长句选区采用差异化展示逻辑。
*   **稳定性增强**：统一异常处理机制，Provider 属性开放访问，提升整体运行健壮性。

### 📅 2026.04.14
*   **版本适配**：正式增加对 **Visual Studio 2026** 的支持。

<details>
<summary>点击查看更多历史记录</summary>

- **2024.12.07**: 修复设置报错；新增本地未命中自动触发在线翻译；支持全量英文文档注释翻译。
- **2023.10.30**: 深度集成微软 Edge 语音引擎发音。
- **2023.09.26**: 升级 V2 版本，启用新框架。支持主题切换、发音系统。
- **2023.07.16**: 关键 Bug 修复，解决悬浮特定字符导致 VS 崩溃的稳定性问题。
</details>

---

# 🔗 友情链接
🛍️ [**莫欺客鞋帽优选**](https://www.moqistar.com) - 优质严选，品质生活。
