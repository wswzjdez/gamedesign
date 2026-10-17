# 星岛计划 — 冲刺计划 (Sprint Plan)

> 版本：v0.3 · 更新：2026-10-17

## S0 — 立项讨论 ✅
- 专家圆桌确认定位/类型/平台/概念。
- 交付：纯娱乐 / 模拟经营 / 多端 / 个人作品集 / 星岛计划 / Web+PC 先 MVP。

## S1 — GDD + 脚手架 ✅
- 概念文档、系统索引、技术架构、ADR-001、冲刺计划、代码骨架、README、.gitignore。

## S2 — 垂直切片（核心循环）✅
- EcoSim + ResourceSystem + BioSystem + SaveSystem + OnGUI HUD。
- 闭环：采集→建生态→孵化→生物反哺，打开即玩。

## S3 — 系统深化（AI / UI / 科技 / 图鉴）✅
- **BioBrain**：默认走训练好的神经网络（BioModel 嵌入式 C# 推理，2022.3/Unity6 通用）；保留 `#if STARISLE_SENTIS` 通道供 Unity6 + Sentis 2.0 真·端侧推理（详见 s4-ai-bio.md）。
- **UIToolkit HUD**：替换 OnGUI，自举 PanelSettings，无需预制体。
- **TechTree + TechDef**：数据驱动科技，效果实时作用于配置。
- **Codex + CreatureSpecies**：图鉴与多物种（默认 2 种，科技解锁第 2 种）。
- AI 驱动繁殖（能量高+有容量→涌现式孵化）。
- 文档：ai-bio-design.md、systems-index v0.3 更新。
**退出标准**：核心循环 + 科技 + 图鉴在编辑器中可玩；生物走训练神经网络或 Sentis。

## S4 — AI 生物网络 + Web Demo + 作品集
- **AI 生物网络（已部分完成）**：4→8→4 MLP 已用 Python(numpy) 训练，权重嵌入 `BioModel.cs`，`BioBrain` 默认走纯 C# 端侧推理（2022.3 可运行，无需 Sentis）。训练脚本 `tools/train_bio_model.py` 可复现。验证 MSE≈0.0024，行为符合预期。
- **[已完成]** WebGL 兼容性审计：全工程无 `System.IO`/线程/`Socket`/`DllImport` 等 WebGL 不支持 API，可直接打 Web 包。
- **[已完成]** 在线 Demo 页：`web/index.html`（深色酷感、嵌入 WebGL 构建槽、技术亮点/玩法/关于）。详见 `docs/webgl-build.md`。
- **[待做]** 在 Unity 中用 Build Settings→WebGL 实际构建产物到 `web/build/`（需用户在 Unity 内执行，本环境无法跑 Unity 构建）。
- **[待做]** 把 SaveSystem 从 PlayerPrefs 迁 IndexedDB(Web)/文件(PC)。
- **[待做/可选]** 切到 Unity 6 + Sentis 2.0，由同权重导 `.onnx` 启用真·GPU 端侧推理（行为一致，仅运行后端升级）。
**退出标准**：生物决策由网络产生；Web 链接可访问、中低配 60fps。

## S5 — 移动端扩展
- Android/iOS 可用；触屏 UI 重构（InputSystem）、实名防沉迷预留、商店合规、Addressables 分包。
**退出标准**：真机可玩，首包体积达标。

## 风险
- Sentis 模型训练/压缩 → S4 留足时间（当前已用嵌入式 C# 推理绕开 Sentis 依赖，2022.3 可运行）。
- Web 性能/内存 → S4 起持续压测。
- 移动端合规 → S5 单独评估。
- **包依赖**：当前 `BioBrain.cs` 默认走嵌入式 C# 推理，**不依赖 Unity Sentis 包即可编译运行**；仅当定义 `STARISLE_SENTIS`（Unity6+Sentis 2.0）时才需要安装 Sentis 包。
