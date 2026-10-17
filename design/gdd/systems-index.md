# 星岛计划 — 系统索引 (Systems Index)

> 版本：v0.3 · 更新：2026-10-17
> 状态图例：⬜ 未开始 · 🟨 设计中 · 🟩 已实现

## 系统总览
| 系统 | 职责 | 依赖 | 状态 |
|---|---|---|---|
| **EcoSim（生态模拟核心）** | 按 tick 推进大气/水/生物多样性，驱动全局模拟 | ResourceSystem, BioSystem, SimConfig | 🟩 |
| **ResourceSystem（资源经济）** | 光/水/矿/生物质的增减与被动产出 | SimConfig | 🟩 |
| **BioSystem（生物系统）** | 孵化/物种/图鉴/生命周期；繁殖由 AI 驱动 | EcoSim, BioBrain, Codex, SimConfig | 🟩 |
| **BioBrain（生物大脑）** | Unity Sentis 端侧推理生物行为；无模型回退启发式 | SimConfig, Unity Sentis | 🟩 |
| **TechTree（科技树）** | 科技解锁、消耗与产出，效果实时作用于配置 | ResourceSystem, EcoSim, BioSystem | 🟩 |
| **Codex（图鉴）** | 记录已发现物种（留存驱动） | BioSystem | 🟩 |
| **SaveSystem（存档）** | 跨端序列化（PlayerPrefs 占位，S4 迁 IndexedDB/文件） | — | 🟩 |
| **UISystem / Hud（界面）** | UIToolkit HUD：资源/生态/建筑/科技/图鉴/存档 | GameManager | 🟩 |
| **InputSystem（跨端输入）** | 键鼠 / 触屏抽象（S4 移动端接入） | UISystem | ⬜ |
| **EventBus（事件总线）** | ScriptableObject 事件，系统间松耦（预留） | — | 🟨 |
| **AudioSystem（音频）** | BGM / 音效（预留） | — | ⬜ |
| **Tutorial（引导）** | 首 3 分钟爽点引导（预留） | UISystem, EcoSim | ⬜ |

## 依赖关系（简图）
```
SimConfig ──► EcoSim ──► BioSystem ──► BioBrain(Sentis) ──► Codex
                │            │
                ├─ ResourceSystem ─┤
                │            │
             TechTree ◄── 消耗资源/解锁物种
                │
             Hud(UIToolkit) ── 玩家动作 ──► GameManager
```

## 核心数据模型
- **生态指标**：Atmosphere / Hydro / Biodiversity（0..1）
- **资源**：light / water / mineral / biomass（数值池）
- **生物**：{ speciesId, energy, age, Alive }；行为由 BioBrain 决策
- **科技**：TechDef { id, cost[], prerequisites[], effects[] }
- **物种**：CreatureSpecies { id, color, scale }

## 下一步
- S4：训练 Sentis `.onnx` 模型接入 BioBrain；WebGL 构建 Demo；移动端/触屏接入。
