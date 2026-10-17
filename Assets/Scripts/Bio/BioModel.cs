// S4 — 生物决策神经网络权重（由 tools/train_bio_model.py 自动生成，请勿手改）
// 结构: 输入(4) -> 隐藏(8, tanh) -> 输出(4, sigmoid)
//   输入: [atmosphere, hydro, biodiversity, energy]
//   输出: [seekResource, seekFood, roam, reproduce]
// 该网络在 2022.3 / Unity6 下均通过纯 C# 前向传播运行，无需任何外部包。
using UnityEngine;

namespace StarIsle.Bio
{
    public static class BioModel
    {
        private const int IN = 4;
        private const int H = 8;
        private const int OUT = 4;

        // W1[j,i] : 隐藏层 j 对输入 i 的权重
        private static readonly float[][] W1 = new float[][]
        {
        new float[] { -0.94185067f, -0.9198616f, 0.39339636f, 1.8382788f },
        new float[] { 0.13514984f, 0.079798938f, 0.066928508f, -2.0665171f },
        new float[] { -0.18611698f, -0.23530019f, -0.68471188f, -0.40664524f },
        new float[] { 0.0017684081f, 0.026942257f, -0.012502486f, 4.9925983f },
        new float[] { -1.2384011f, -1.2566057f, 0.6483244f, 0.17205711f },
        new float[] { -0.16500277f, -0.16630186f, -0.16781574f, -2.1531999f },
        new float[] { -0.08665327f, 0.031071337f, 0.062350093f, 5.4758962f },
        new float[] { -0.827055f, -0.84759056f, -0.83264755f, -4.7939939f }
        };

        private static readonly float[] B1 = new float[] { -3.0187191f, 2.1047559f, 0.86919533f, 0.020825512f, -0.37694256f, 1.9330533f, -3.9259733f, 5.8413261f };

        // W2[k,j] : 输出 k 对隐藏 j 的权重
        private static readonly float[][] W2 = new float[][]
        {
        new float[] { 1.5287241f, 4.7607593f, -0.1027919f, -2.4373665f, -0.09454374f, 1.4793938f, 0.78023963f, 0.1620135f },
        new float[] { 2.0227392f, 4.6275428f, -0.066981351f, -0.81378303f, -0.074748862f, 1.3168964f, 0.78601418f, 0.19119842f },
        new float[] { 0.99158643f, -1.6365628f, 0.28208565f, 2.3431843f, 0.16506644f, -2.7045062f, -0.286676f, 2.2622447f },
        new float[] { 1.9322934f, 2.265711f, -1.3774311f, 1.705873f, -1.4754511f, -2.423233f, -0.32624671f, -0.30619657f }
        };

        private static readonly float[] B2 = new float[] { -0.39270501f, -1.7591811f, -0.81780506f, -2.9570701f };

        public static float[] Forward(float atmosphere, float hydro, float biodiversity, float energy)
        {
            float[] x = new float[IN] { atmosphere, hydro, biodiversity, energy };
            float[] z1 = new float[H];
            float[] a1 = new float[H];
            for (int j = 0; j < H; j++)
            {
                float s = B1[j];
                for (int i = 0; i < IN; i++) s += W1[j][i] * x[i];
                z1[j] = s;
                // Unity Mathf 无 Tanh，用 exp 推导的等价式（数值稳定：溢出自然收敛到 ±1）
                float e2 = Mathf.Exp(2f * s);
                a1[j] = (e2 - 1f) / (e2 + 1f);
            }
            float[] z2 = new float[OUT];
            float[] y = new float[OUT];
            for (int k = 0; k < OUT; k++)
            {
                float s = B2[k];
                for (int j = 0; j < H; j++) s += W2[k][j] * a1[j];
                z2[k] = s;
                y[k] = 1f / (1f + Mathf.Exp(-s));
            }
            return y;
        }
    }
}
