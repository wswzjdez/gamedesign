// S4 — 生物大脑：默认使用训练好的神经网络（BioModel.Forward，纯 C# 前向传播，2022.3/Unity6 通用）；
// 若工程切到 Unity 6 + 安装 Sentis 2.0 并定义 STARISLE_SENTIS，则改用真·端侧推理（ONNX 模型）。
//
// 决策契约：输入(4)[atmosphere, hydro, biodiversity, energy] -> 输出(4)[seekResource, seekFood, roam, reproduce]
// 模型由 tools/train_bio_model.py 训练并导出到 BioModel.cs（weights 自动生成）。
//
// 升级路径（Unity 6 + Sentis 2.0）：
//   在 Edit > Project Settings > Player > Scripting Define Symbols 加入 `STARISLE_SENTIS`，
//   并把导出的 .onnx 拖到 BioBrain 的 modelAsset 字段即可启用端侧 AI 推理（无需改代码）。
#if STARISLE_SENTIS
using Unity.Sentis;
#endif
using UnityEngine;
using System;

namespace StarIsle.Bio
{
    /// <summary>
    /// 观测输入(4): [atmosphere, hydro, biodiversity, energy]
    /// 推理输出(4): [seekResource, seekFood, roam, reproduce] (0..1)
    /// 未定义 STARISLE_SENTIS 时直接走 Heuristic()，游戏照常运行。
    /// </summary>
    public class BioBrain : MonoBehaviour
    {
#if STARISLE_SENTIS
        [SerializeField] private ModelAsset modelAsset;
        private Model _model;
        private IWorker _worker;
#endif
        private bool _useSentis;

#if STARISLE_SENTIS
        private void Awake()
        {
            if (modelAsset == null) return;
            try
            {
                _model = ModelLoader.Load(modelAsset);
                _worker = new Worker(_model, BackendType.GPUCompute);
                _useSentis = true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[BioBrain] Sentis 初始化失败，回退启发式: {e.Message}");
                _useSentis = false;
            }
        }
#endif

        public float[] Decide(float atmosphere, float hydro, float biodiversity, float energy)
        {
#if STARISLE_SENTIS
            if (_useSentis && _worker != null)
            {
                using var input = new Tensor<float>(new TensorShape(1, 4));
                input[0] = atmosphere; input[1] = hydro; input[2] = biodiversity; input[3] = energy;
                _worker.Execute(input);
                var outT = _worker.PeekOutput() as Tensor<float>;
                if (outT != null)
                {
                    int n = outT.shape[1];
                    var r = new float[n];
                    for (int i = 0; i < n; i++) r[i] = Mathf.Clamp01(outT[i]);
                    return r;
                }
            }
#endif
            // 默认走嵌入式的训练神经网络（BioModel.Forward），2022.3 / Unity6 通用。
            // BioModel 由 tools/train_bio_model.py 训练导出，纯 C# 前向传播，零外部依赖。
            return BioModel.Forward(atmosphere, hydro, biodiversity, energy);
        }

        private void OnDestroy()
        {
#if STARISLE_SENTIS
            _worker?.Dispose();
#endif
        }
    }
}
