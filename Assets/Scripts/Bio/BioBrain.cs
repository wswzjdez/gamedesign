// S3 — 生物大脑：Unity Sentis 端侧推理；无模型时回退启发式（保证可运行）
using UnityEngine;
using Unity.Sentis;
using System;

namespace StarIsle.Bio
{
    /// <summary>
    /// 观测输入(4): [atmosphere, hydro, biodiversity, energy]
    /// 推理输出(4): [seekResource, seekFood, roam, reproduce] (0..1)
    /// 未挂 ModelAsset 时走 Heuristic()，游戏照常运行。
    /// </summary>
    public class BioBrain : MonoBehaviour
    {
        [SerializeField] private ModelAsset modelAsset;
        private Model _model;
        private IWorker _worker;
        private bool _useSentis;

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

        public float[] Decide(float atmosphere, float hydro, float biodiversity, float energy)
        {
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

        private void OnDestroy() => _worker?.Dispose();
    }
}
