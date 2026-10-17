// S3 — 生物大脑：优先 Unity Sentis 端侧推理；未启用/无模型时回退启发式（保证可运行）
//
// 兼容性说明：Unity Sentis 2.0 仅支持 Unity 6。当前工程在 Unity 2022.3 下编译，
// 因此默认关闭真实推理、走启发式（游戏照常运行）。
// 将来若切回 Unity 6 + 安装 Sentis 2.0：在
//   Edit > Project Settings > Player > Scripting Define Symbols
// 加入 `STARISLE_SENTIS` 即可启用端侧 AI 推理（无需改代码）。
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
            return Heuristic(atmosphere, hydro, biodiversity, energy);
        }

        // 启发式：缺能量→找资源；能量高且生态好→繁殖；否则漫游
        private float[] Heuristic(float a, float h, float b, float e)
        {
            float seek = 1f - e;
            float reproduce = e * Mathf.Clamp01((a + h) * 0.5f);
            float roam = 1f - Mathf.Max(seek, reproduce);
            return new float[] { seek, seek, roam, reproduce };
        }

        private void OnDestroy()
        {
#if STARISLE_SENTIS
            _worker?.Dispose();
#endif
        }
    }
}
