# 参考项目清单（类似《星岛计划》的开源项目）

> 用途：为《星岛计划》（Unity 轻科幻生态星球模拟经营 / 面向中学生 / 多端 / 作品集）寻找可借鉴的开源参考。
> 数据截至 2026-10 检索，星数为近似值（会变动），以 GitHub 实时为准。
> 排序逻辑：主题贴合度优先（生态/星球/经营模拟），兼顾星数。

## 一、主题最贴：生态 / 星球 / 改造模拟

### 1. Thrive — 进化与生态系统模拟
- 仓库：https://github.com/Revolutionary-Games/Thrive
- 星数：≈ 3.8k　引擎/语言：Godot + C#
- 相似点：从单细胞到复杂生命的**生态/进化模拟**，生物涌现、生态链耦合——与本项目"生态星球 + AI 生物"气质最像。
- 可借鉴：生态模拟系统设计、生物属性遗传/突变、从简单到复杂的成长曲线。
- 注：引擎是 Godot 非 Unity，但系统架构思路极有价值。

### 2. Gaia Maker — 星球改造模拟
- 仓库：https://github.com/garkimasera/gaia-maker
- 星数：≈ 76　引擎/语言：Rust
- 相似点：**行星级改造**（温度、生物量、碳循环、大气成分、文明演化）——主题与"星岛计划"几乎同构。
- 可借鉴：如何用简单数值模型驱动整颗星球的生态状态变化。

### 3. PROJECT ELYSIUM — 行星殖民模拟（浏览器）
- 仓库：https://github.com/wc4mp0s/elysium
- 星数：中等（浏览器静态站，纯 JS）　语言：HTML/CSS/JS
- 相似点：20 名幸存者在外星建基地，从零产出水/食物/金属/能源/科技——**经营 + 生存 + 科技树**。
- 可借鉴：确定性模拟、科技分层、事件驱动，且是浏览器即开即玩（吻合我们 Web 优先策略）。

### 4. Second Earth — 外星殖民与改造
- 仓库：https://github.com/Viktor-Butkovich/Second-Earth-game
- 星数：≈ 2　语言：Python
- 相似点：人类外星殖民 + 星球改造，强调科学准确性。
- 注：星数低但主题高度吻合，适合看改造/殖民的数值建模思路。

## 二、高星经营 / 模拟类（学系统架构与数值平衡）

### 5. Mindustry — 资源与物流自动化
- 仓库：https://github.com/Anuke/Mindustry
- 星数：≈ 22k　语言：Java
- 相似点：资源采集→生产链→自动化，tick 驱动的大规模模拟。
- 可借鉴：**生产链/经济系统的解耦架构**——作品集最该参考的工程结构；我们的 EcoSim 可对照其 tick 模型。

### 6. OpenRCT2 — 主题公园经营
- 仓库：https://github.com/OpenRCT2/OpenRCT2
- 星数：≈ 13k　语言：C++
- 相似点：经典经营模拟，长线数值平衡与留存循环。
- 可借鉴：经营类数值曲线、玩家长期目标设计。

### 7. OpenTTD — 运输与经济模拟
- 仓库：https://github.com/OpenTTD/OpenTTD
- 星数：≈ 11k　语言：C++
- 相似点：大规模经济系统 + 寻路 + 网络。
- 可借鉴：经济体量管理与性能优化思路。

### 8. Luanti (Minetest) — 体素沙盒世界
- 仓库：https://github.com/luanti-org/luanti
- 星数：≈ 11k　语言：C++ / Lua
- 相似点：开放世界 + 数据驱动 + Mod 生态。
- 可借鉴：世界生成、Mod/插件化扩展（可对应我们未来的图鉴/UGC）。

### 9. Cataclysm: Dark Days Ahead — 生存模拟
- 仓库：https://github.com/CleverRaven/Cataclysm-DDA
- 星数：≈ 10k　语言：C++
- 相似点：复杂系统耦合 + 事件驱动 + 状态机。
- 可借鉴：多系统互相影响的模拟设计（类似我们生态/资源/生物三系统耦合）。

## 三、Unity 直接相关（同引擎，最易直接参考）

### 10. OpenTS2 — 《模拟人生2》Unity 重制（生活模拟）
- 仓库：https://github.com/LazyDuchess/OpenTS2
- 星数：中等　引擎：Unity + C#
- 相似点：**直接用 Unity 做模拟游戏**，含模块化系统、存档、场景管理。
- 可借鉴：Unity 模拟类项目的工程分层与脚本组织（对照我们的 GameManager/EcoSim 架构）。

### 11. Festival Simulator - Unity Edition — 音乐节经营模拟
- 仓库：https://github.com/jamiegmusic/the-festive-sim
- 星数：较低　引擎：Unity + C#
- 相似点：模块化管理系统（安全/医疗/人群/天气/物流），低多边形美术——**与本项目的"中心 GameManager + 多子系统"架构几乎同构**。
- 可借鉴：分系统设计模式、低多边形美术落地。

### 12. Unity ML-Agents — Unity 官方 AI 工具包
- 仓库：https://github.com/Unity-Technologies/ml-agents
- 星数：≈ 18.6k　语言：C# / Python
- 相似点：Unity 内训练/驱动智能体——对应我们 **BioBrain / Sentis 端侧 AI 生物**这一核心卖点。
- 可借鉴：智能体行为训练范式（可作为我们未来上真模型的对照基线）。

## 四、额外参考（教育向 / 青少年向）
- **Antura** (https://github.com/vgaultier/Antura) — ⭐≈78，Unity 做的面向儿童的语言学习游戏，适合参考"中学生友好"的 UI/交互与护眼美术。

## 总结建议
- 想学**生态/星球主题**：看 Thrive、Gaia Maker、Elysium。
- 想学**系统架构/数值**：看 Mindustry、OpenRCT2、Cataclysm-DDA。
- 想学**Unity 直接落地**：看 OpenTS2、Festival Simulator、ML-Agents。
- 想学**Web/多端发布**：Elysium（纯前端即开即玩）是范本。
