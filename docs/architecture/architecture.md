# 星岛计划 — 技术架构 (Architecture)

> 版本：v0.1 · 更新：2026-10-17 · 引擎：Unity 6 LTS (URP)

## 1. 技术栈
- **引擎**：Unity 6 LTS，渲染管线 URP（移动/Web 友好，APV/STP 可用）
- **语言**：C#（命名空间 `StarIsle.*`）
- **AI**：Unity Sentis（端侧 `.onnx` 推理，生物行为）
- **UI**：UIToolkit（Web/PC 一套，跨端友好；复杂动效可混用 UIElements）
- **资源**：Addressables（按需加载，Web 首包体积控制；本期可选）

## 2. 架构模式
- **数据驱动 + 事件松耦**：所有可调参数放 `ScriptableObject`（`SimConfig` 等），系统间通过 `GameEvent`（SO 事件）通信，避免硬引用。
- **模拟与表现分离**：`EcoSim` 只算数据（tick），表现层（模型/UI）订阅事件刷新。便于单元测试与后期迁 DOTS。
- **跨端抽象**：输入、存档、平台能力均走接口，编译指令切换实现。

## 3. 模块分层（Assets/Scripts）
```
StarIsle/
├── Core/         # GameManager(状态机)、Boot
├── Simulation/   # EcoSim(生态tick)、SimClock
├── Systems/      # ResourceSystem, TechTree
├── Bio/          # BioAgent(AI), BioSystem
├── Persistence/  # SaveSystem + ISaveBackend(Web/File)
├── Input/        # InputRouter + IInputProvider(Touch/Mouse)
├── Events/       # GameEvent(SO), EventBus
├── UI/           # UIToolkit 控制器
├── Audio/        # AudioSystem
└── Config/       # ScriptableObject 配置(生物/资源/科技)
```

## 4. 数据流
```
Update(tick) → EcoSim.Tick()
   → 更新生态指标 & 资源 rate
   → 触发 GameEvent(Raise): OnResourceChanged / OnBiomeShift
   → ResourceSystem / BioSystem 响应
   → UISystem 订阅刷新面板
   → SaveSystem 定时快照
```

## 5. 关键决策
| 决策 | 选择 | 理由 | 备选 |
|---|---|---|---|
| 模拟实现 | MonoBehaviour + SO 起步 | 简单、快、作品集可读 | DOTS/ECS（数据量大再迁） |
| 系统通信 | ScriptableObject 事件 | 松耦、可视化、易调试 | 直接方法调用 |
| 存档 | 接口 + 编译分支 | 一套代码跨端 | 各自实现 |
| UI | UIToolkit | 跨端一套、性能好 | UGUI |
| AI 生物 | Sentis 端侧 | 行为涌现、卖点 | 固定状态机 |

## 6. 性能要点（Unity 6）
- **Adaptive Probe Volumes**：自动布光探针，省手动打光。
- **GPU Resident Drawer + GPU Occlusion Culling**：大场景低 CPU。
- **STP（时空后处理）**：廉价提质。
- Web 端注意：堆内存上限、避免同步 IO；存档走 IndexedDB 异步。

## 7. Web 构建注意
- 选用 **WebGL** 起步（稳定），**WebGPU** 作为高画质分支（实验性，按需开启）。
- 首包体积控制：Addressables 分包，首屏只载核心。
- 输入：Web 移动端走触屏 `InputRouter`。

## 8. 目录与文件约定
- 每个系统一个文件夹、一个主 `MonoBehaviour` + 对应 `ScriptableObject` 配置。
- 事件以 `OnXxx` 命名，集中在 `Events/` 便于检索。
- 脚手架代码见 `Assets/Scripts/*`（已生成 .cs 占位）。
