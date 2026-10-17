# S4 — AI 生物决策网络（端侧推理）

本文档说明《星岛计划》中"会思考的生物"是如何实现的，以及它在当前 Unity 2022.3 环境与未来 Unity 6 环境下的运行方式。

## 目标
让每个奇异生物的行动决策由**神经网络**产生（而非手写 if-else 规则），形成"看起来有自主意识"的涌现行为——这是作品集的核心技术亮点。

## 决策契约
```
输入(4): [atmosphere(大气), hydro(水生态), biodiversity(生物多样性), energy(个体能量)]  // 各 0..1
输出(4): [seekResource(觅食/回资源), seekFood(觅食), roam(漫游), reproduce(繁殖)]     // 各 0..1
```
- `BioAgent.Act()` 用 `d[0] > 0.5` 判定是否回到基地中心（缺资源时）；`d[3]`（reproduce）作为繁殖倾向，由 `BioSystem` 中心化模拟决定是否孵化新个体。

## 模型架构
- 结构：`4 → 8(tanh) → 4(sigmoid)` 的轻量 MLP。
- 参数量极小（约 100 个浮点），前向传播在任意设备上都是微秒级，适合中学生机型的实时模拟。

## 训练数据（合成监督）
用"理想生物行为策略"生成 8000 条合成样本，加 5% 噪声：
| 状态 | 期望行为 |
|---|---|
| 能量低 (e<0.4) | seekResource↑、roam↓、reproduce≈0（先活命） |
| 能量高 + 生态好 | reproduce↑、roam 中、seekResource↓（繁衍扩张） |
| 其余 | roam 平衡 |

训练结果：验证集 MSE ≈ 0.0024，行为符合预期（见下"行为验证"）。

## 运行方式（两档）
### A. 嵌入式 C# 推理（默认，2022.3 / Unity6 通用）
- 训练好的权重由 `tools/train_bio_model.py` 直接写入 `Assets/Scripts/Bio/BioModel.cs`（纯浮点数组）。
- `BioBrain.Decide()` 默认调用 `BioModel.Forward(...)`，纯 C# 前向传播，**零外部依赖**，当前 2022.3 工程即可运行真实网络推理。
- 重新训练只需跑脚本，权重自动刷新，无需改游戏代码。

### B. Unity Sentis 真·端侧推理（升级路径，需 Unity 6 + Sentis 2.0）
- `BioBrain.cs` 内 `#if STARISLE_SENTIS` 段保留 ONNX 推理逻辑（`ModelLoader` + `Worker` + GPUCompute）。
- 启用方式：
  1. 将工程切到 **Unity 6 LTS**，Package Manager 安装 **Unity Sentis 2.0**；
  2. 由 `tools/train_bio_model.py` 的权重（或重新用 PyTorch 导出同架构 `.onnx`）生成模型资产；
  3. 把 `.onnx` 拖到 `BioBrain` 的 `modelAsset` 字段；
  4. 在 `Edit > Project Settings > Player > Scripting Define Symbols` 加入 `STARISLE_SENTIS`。
- 此时生物决策走 GPU/CPU 上的 Sentis 推理，权重与嵌入式版本一致，行为不变。

## 行为验证（训练后定性检查）
| 输入状态 | 输出 [seekRes, seekFood, roam, reproduce] |
|---|---|
| 低能量 (e=0.1) | [0.906, 0.719, 0.099, 0.042] → 回去觅食、不繁殖 |
| 高能量+生态好 | [0.104, 0.082, 0.295, 0.680] → 繁殖、适度漫游 |
| 能量中等 | [0.513, 0.404, 0.501, 0.148] → 均衡漫游 |

> 注：以上数值在 Python 训练端与 C# `BioModel.Forward` 端已做对拍，结果一致。

## 文件清单
- `tools/train_bio_model.py` — 训练 + 导出脚本（可复现）
- `tools/bio_model_weights.json` — 训练权重（便于将来转 ONNX / 复现）
- `Assets/Scripts/Bio/BioModel.cs` — 自动生成的 C# 权重 + 前向传播（**勿手改**）
- `Assets/Scripts/Bio/BioBrain.cs` — 决策入口，默认走 `BioModel`，可选走 Sentis

## 复现命令
```bash
python3 tools/train_bio_model.py
```
