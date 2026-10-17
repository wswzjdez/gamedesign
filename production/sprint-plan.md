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
- **BioBrain**：Unity Sentis 端侧推理接口 + 启发式回退。
- **UIToolkit HUD**：替换 OnGUI，自举 PanelSettings，无需预制体。
- **TechTree + TechDef**：数据驱动科技，效果实时作用于配置。
- **Codex + CreatureSpecies**：图鉴与多物种（默认 2 种，科技解锁第 2 种）。
- AI 驱动繁殖（能量高+有容量→涌现式孵化）。
- 文档：ai-bio-design.md、systems-index v0.3 更新。
**退出标准**：核心循环 + 科技 + 图鉴在编辑器中可玩；生物走 Sentis 或启发式。

## S4 — Web Demo + 作品集（下一步）
- 训练 Sentis `.onnx` 模型接入 BioBrain（详见 ai-bio-design.md）。
- WebGL 构建、在线 Demo 页、架构/AI 亮点说明。
- 把 SaveSystem 从 PlayerPrefs 迁 IndexedDB(Web)/文件(PC)。
**退出标准**：链接可访问、中低配 60fps。

## S5 — 移动端扩展
- Android/iOS 可用；触屏 UI 重构（InputSystem）、实名防沉迷预留、商店合规、Addressables 分包。
**退出标准**：真机可玩，首包体积达标。

## 风险
- Sentis 模型训练/压缩 → S4 留足时间。
- Web 性能/内存 → S4 起持续压测。
- 移动端合规 → S5 单独评估。
- **包依赖**：`BioBrain.cs` 使用 `Unity.Sentis`，必须安装 Unity Sentis 包才能编译（无模型可运行，但包必须装）。
