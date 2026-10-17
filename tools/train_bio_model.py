#!/usr/bin/env python3
"""
S4 — 训练《星岛计划》生物决策神经网络，并导出为可嵌入 C# 的权重。

网络结构: 输入(4) -> 隐藏(8, tanh) -> 输出(4, sigmoid)
  输入 x:  [atmosphere, hydro, biodiversity, energy]  (各 0..1)
  输出 y:  [seekResource, seekFood, roam, reproduce]   (各 0..1)

说明:
- 用 numpy 训练（无需 torch），因为当前工程在 Unity 2022.3，Sentis 2.0 不可用。
- 训练好的权重直接写入 Assets/Scripts/Bio/BioModel.cs，
  由纯 C# 前向传播运行（2022.3 / Unity6 通用，零外部依赖）。
- 同时保留 #if STARISLE_SENTIS 通道，未来切 Unity6 + Sentis 可做真·ONNX 端侧推理。

运行:
  python3 tools/train_bio_model.py
"""
import json
import os
import numpy as np

RNG = np.random.default_rng(20261017)
H = 8  # 隐藏层宽度
IN = 4
OUT = 4
LR = 0.01
EPOCHS = 4000
BATCH = 256
N_TRAIN = 8000


def policy(a, h, b, e):
    """理想生物行为策略（监督标签），带轻微噪声。"""
    eco = 0.5 * (a + h)
    seek_resource = 1.0 - e
    seek_food = 0.8 * (1.0 - e)
    reproduce = e * eco * (0.4 + 0.6 * b)
    roam = 1.0 - max(seek_resource, reproduce)
    out = np.array([seek_resource, seek_food, roam, reproduce])
    out = np.clip(out, 0.0, 1.0)
    out = np.clip(out + RNG.normal(0.0, 0.05, OUT), 0.0, 1.0)
    return out


def make_dataset(n):
    X = RNG.uniform(0.0, 1.0, size=(n, IN))
    Y = np.array([policy(*row) for row in X])
    return X, Y


def sigmoid(z):
    return 1.0 / (1.0 + np.exp(-z))


def forward(x, W1, b1, W2, b2):
    x2 = np.atleast_2d(x)
    if x2.shape[0] != IN:          # (1, IN) -> (IN, 1)
        x2 = x2.T
    z1 = W1 @ x2 + b1[:, None]
    a1 = np.tanh(z1)
    z2 = W2 @ a1 + b2[:, None]
    a2 = sigmoid(z2)
    if np.asarray(x).ndim == 1:
        return a1[:, 0], a2[:, 0]
    return a1, a2


def main():
    X, Y = make_dataset(N_TRAIN)
    n = len(X)

    # 初始化（He/Kaiming 风格）
    W1 = RNG.normal(0, np.sqrt(2.0 / IN), size=(H, IN))
    b1 = np.zeros(H)
    W2 = RNG.normal(0, np.sqrt(2.0 / H), size=(OUT, H))
    b2 = np.zeros(OUT)

    # Adam 状态
    mW1 = np.zeros_like(W1); vW1 = np.zeros_like(W1)
    mb1 = np.zeros_like(b1); vb1 = np.zeros_like(b1)
    mW2 = np.zeros_like(W2); vW2 = np.zeros_like(W2)
    mb2 = np.zeros_like(b2); vb2 = np.zeros_like(b2)
    b1a = 0.9; b2a = 0.999; eps = 1e-8

    idx = np.arange(n)
    for epoch in range(EPOCHS):
        RNG.shuffle(idx)
        for s in range(0, n, BATCH):
            sel = idx[s:s + BATCH]
            xb = X[sel]; yb = Y[sel]; m = len(sel)
            # 前向
            z1 = W1 @ xb.T + b1[:, None]          # (H, m)
            a1 = np.tanh(z1)
            z2 = W2 @ a1 + b2[:, None]            # (OUT, m)
            a2 = sigmoid(z2)
            # 反向 (MSE)
            dz2 = (a2 - yb.T) * a2 * (1 - a2)      # (OUT, m)
            dW2 = dz2 @ a1.T / m
            db2 = dz2.sum(axis=1) / m
            da1 = W2.T @ dz2
            dz1 = da1 * (1 - a1 * a1)
            dW1 = dz1 @ xb / m
            db1 = dz1.sum(axis=1) / m
            # Adam 更新
            for (p, g, mp, vp) in [
                (W1, dW1, mW1, vW1), (b1, db1, mb1, vb1),
                (W2, dW2, mW2, vW2), (b2, db2, mb2, vb2),
            ]:
                mp[:] = b1a * mp + (1 - b1a) * g
                vp[:] = b2a * vp + (1 - b2a) * (g * g)
                mhat = mp / (1 - b1a ** (epoch + 1))
                vhat = vp / (1 - b2a ** (epoch + 1))
                p[:] -= LR * mhat / (np.sqrt(vhat) + eps)

        if (epoch + 1) % 500 == 0:
            _, a2v = forward(X.T, W1, b1, W2, b2)
            loss = np.mean((a2v.T - Y) ** 2)
            print(f"epoch {epoch+1:4d}  train MSE = {loss:.5f}")

    # 验证
    Xt, Yt = make_dataset(2000)
    _, a2t = forward(Xt.T, W1, b1, W2, b2)
    val_mse = np.mean((a2t.T - Yt) ** 2)
    print(f"\nVALIDATION MSE = {val_mse:.5f}")

    cases = [
        ("低能量 (e=0.1)", [0.5, 0.5, 0.5, 0.1]),
        ("高能量+生态好 (e=0.9,a=h=0.9,b=0.8)", [0.9, 0.9, 0.8, 0.9]),
        ("能量中等+生态一般 (e=0.5,a=h=0.5,b=0.3)", [0.5, 0.5, 0.3, 0.5]),
    ]
    print("\n定性检查 (输入 -> [seekRes, seekFood, roam, reproduce]):")
    for name, xin in cases:
        _, out = forward(np.array(xin), W1, b1, W2, b2)
        print(f"  {name}: {np.round(out,3).tolist()}")

    write_csharp(W1, b1, W2, b2)
    # 同时导出权重 JSON（便于将来转 ONNX / 复现）
    export_json(W1, b1, W2, b2)


def fmt(v):
    # C# float 字面量必须带 'f' 后缀，否则被当成 double 导致 CS0266/CS0664
    s = f"{v:.8g}"
    if s in ("inf", "-inf", "nan"):
        raise ValueError(f"权重含非法值: {s}")
    return s + "f"


def write_csharp(W1, b1, W2, b2):
    here = os.path.dirname(os.path.abspath(__file__))
    out = os.path.join(here, "..", "Assets", "Scripts", "Bio", "BioModel.cs")
    out = os.path.normpath(out)

    def mat(rows, cols, arr):
        lines = []
        for r in range(rows):
            vals = ", ".join(fmt(arr[r, c]) for c in range(cols))
            suffix = "," if r < rows - 1 else ""
            lines.append(f"        new float[] {{ {vals} }}{suffix}")
        return "\n".join(lines)

    content = f"""// S4 — 生物决策神经网络权重（由 tools/train_bio_model.py 自动生成，请勿手改）
// 结构: 输入(4) -> 隐藏({H}, tanh) -> 输出(4, sigmoid)
//   输入: [atmosphere, hydro, biodiversity, energy]
//   输出: [seekResource, seekFood, roam, reproduce]
// 该网络在 2022.3 / Unity6 下均通过纯 C# 前向传播运行，无需任何外部包。
using UnityEngine;

namespace StarIsle.Bio
{{
    public static class BioModel
    {{
        private const int IN = {IN};
        private const int H = {H};
        private const int OUT = {OUT};

        // W1[j,i] : 隐藏层 j 对输入 i 的权重
        private static readonly float[][] W1 = new float[][]
        {{
{mat(H, IN, W1)}
        }};

        private static readonly float[] B1 = new float[] {{ {", ".join(fmt(v) for v in b1)} }};

        // W2[k,j] : 输出 k 对隐藏 j 的权重
        private static readonly float[][] W2 = new float[][]
        {{
{mat(OUT, H, W2)}
        }};

        private static readonly float[] B2 = new float[] {{ {", ".join(fmt(v) for v in b2)} }};

        public static float[] Forward(float atmosphere, float hydro, float biodiversity, float energy)
        {{
            float[] x = new float[IN] {{ atmosphere, hydro, biodiversity, energy }};
            float[] z1 = new float[H];
            float[] a1 = new float[H];
            for (int j = 0; j < H; j++)
            {{
                float s = B1[j];
                for (int i = 0; i < IN; i++) s += W1[j][i] * x[i];
                z1[j] = s;
                // Unity Mathf 无 Tanh，用 exp 推导的等价式（数值稳定）
                float e2 = Mathf.Exp(2f * s);
                a1[j] = (e2 - 1f) / (e2 + 1f);
            }}
            float[] z2 = new float[OUT];
            float[] y = new float[OUT];
            for (int k = 0; k < OUT; k++)
            {{
                float s = B2[k];
                for (int j = 0; j < H; j++) s += W2[k][j] * a1[j];
                z2[k] = s;
                y[k] = 1f / (1f + Mathf.Exp(-s));
            }}
            return y;
        }}
    }}
}}
"""
    with open(out, "w") as f:
        f.write(content)
    print(f"\n已生成 C# 模型文件: {out}")


def export_json(W1, b1, W2, b2):
    here = os.path.dirname(os.path.abspath(__file__))
    out = os.path.join(here, "bio_model_weights.json")
    out = os.path.normpath(out)
    data = {
        "arch": {"in": IN, "hidden": H, "out": OUT,
                 "hidden_act": "tanh", "output_act": "sigmoid"},
        "W1": W1.tolist(), "b1": b1.tolist(),
        "W2": W2.tolist(), "b2": b2.tolist(),
    }
    with open(out, "w") as f:
        json.dump(data, f, indent=2)
    print(f"已导出权重 JSON: {out}")


if __name__ == "__main__":
    main()
