# 星岛计划 — AI 生物设计 (Unity Sentis)

> 更新：2026-10-17 · 状态：接口已接入，模型待训练

## 目标
让生物行为由**端侧神经网络**驱动，而非固定状态机。玩家看到的是"会自己拿主意"的奇异生物，行为涌现、可解释但不完全可控 → 作品集最强差异化卖点。

## 当前实现（S3）
- `Bio/BioBrain.cs`：加载 `ModelAsset`，每决策步做一次推理。
- 输入张量 `[1,4]`：`[atmosphere, hydro, biodiversity, energy]`
- 输出张量 `[1,4]`：`[seekResource, seekFood, roam, reproduce]`（0..1）
- **无模型时自动回退 `Heuristic()`**（能量低→找资源；能量高且生态好→繁殖），保证打开即玩。

## 训练一个 .onnx 模型（S4 任务）
1. 用 PyTorch 训一个极小 MLP（输入 4 → 隐藏 8 → 输出 4，sigmoid 输出）。
2. 监督信号可用启发式标签（先让 Heuristic 产出"标准答案"作为训练数据）。
3. `torch.onnx.export(model, dummy, "bio_brain.onnx")`。
4. 把 `.onnx` 拖入 Unity `Assets/`，自动生成 `ModelAsset`。
5. 在 Inspector 给任意 `BioBrain` 挂上该 `ModelAsset` 即启用真·AI。

## 推理后端
- 默认 `BackendType.GPUCompute`；WebGL 构建时 Sentis 会自动回退到 CPU 后端，无需改代码。

## 注意
- 模型文件缺失不影响运行（走启发式）；但 **Unity Sentis 包必须安装**，否则 `BioBrain.cs` 无法编译。
- 真模型接入后，可在 `BioAgent.Act` 中扩展输出维度（如加入"迁徙/社交"行为）。
