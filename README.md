# 星岛计划 (Star Isle)

> 轻科幻生态星球模拟经营游戏 · Unity 6 · 面向中学生（12–15 岁）

## 一句话
你是一名星际生态工程师，登陆一颗荒芜的外星岛屿，通过维持生态平衡、研发科技、孵化会"思考"的奇异生物，逐步唤醒整颗星球。

## 环境要求
- **引擎**：Unity 6.0 LTS 或更高（渲染管线用 URP）
- **目标运行时**：WebGL / WebGPU（先行）→ PC(Standalone) → 后续 Android / iOS
- **必需包**：`Universal RP`、`Unity Sentis`（BioBrain 用到 `Unity.Sentis` 命名空间，必须安装才能编译；无模型也可运行，会回退启发式）
- **可选包**：`Addressables`（资源分包）
- **版本控制**：Git（忽略规则见 `.gitignore`）

## 工程初始化步骤
1. 打开 **Unity Hub → New Project → 模板选 `3D (URP)`**。
2. 项目位置选择**本仓库根目录**（与 `Assets/` 同级），工程名 `StarIsle`。
3. 引擎版本固定为 **Unity 6 LTS**（在 Hub 中确认安装）。
4. Package Manager 安装：`Universal RP`、`Unity Sentis`。
5. 场景里放一个空 GameObject，挂 `GameManager` 脚本（它会自动挂上所有子系统）。
6. 点 **Play** 即可游玩。

## 玩法（S3）
- 采集矿物 → 建造净化器/水循环 → 生态指标上升 → 生物耗水产生物质 → 攒够孵化更多。
- 科技树：生物质增产 / 强化净化 / 强化水循环 / 解锁第 2 物种（荧藻兽）。
- 图鉴：记录已发现物种。AI 驱动繁殖会出现涌现式增长。

## 目录结构
```
StarIsle/
├── Assets/Scripts/
│   ├── Core/      GameManager（总控/自举）
│   ├── Simulation/ EcoSim（生态tick）
│   ├── Systems/   ResourceSystem, TechTree
│   ├── Bio/       BioSystem, BioAgent, BioBrain(Sentis), Codex, CreatureSpecies
│   ├── Persistence/ SaveSystem
│   ├── UI/        Hud（UIToolkit）
│   └── Config/    SimConfig, TechDef
├── design/gdd/        概念 + 系统索引
├── docs/architecture/  架构 + ADR + ai-bio-design
└── production/         冲刺计划
```

## 文档索引
- 概念设计：`design/gdd/game-concept.md`
- 系统索引：`design/gdd/systems-index.md`
- 技术架构：`docs/architecture/architecture.md`
- 平台选型 ADR：`docs/architecture/adr-platform-choice.md`
- AI 生物设计：`docs/ai-bio-design.md`
- 冲刺计划：`production/sprint-plan.md`
