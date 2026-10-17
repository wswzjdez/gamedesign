// S4 — 程序化 Low Poly 科幻殖民者：纯代码拼装 + 程序化走/待机动画（零外部资源）
using UnityEngine;

namespace StarIsle.Characters
{
    /// <summary>
    /// 挂载在玩家根物体上。Awake 时用基础几何体拼出一个低多边形科幻殖民者，
    /// 并通过关节枢轴做程序化走/待机动画，无需任何外部模型或动画片段。
    /// 美术：暗蓝制服 + 青色发光面罩/胸核，契合项目"低多边形 + 扁平矢量"风格。
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class ColonistAvatar : MonoBehaviour
    {
        [Header("动画调参")]
        public float idleBob = 0.025f;       // 待机呼吸幅度
        public float walkFreqBase = 3.2f;    // 步频基数
        public float walkAmp = 0.5f;         // 摆幅基数

        private Transform _body, _head;
        private Transform _lArm, _rArm, _lLeg, _rLeg;
        private Material _visorMat;

        private float _stride = 0f;   // 步态相位
        private float _t = 0f;        // 全局时间（待机用）
        private float _curLegL = 0f, _curLegR = 0f, _curArmL = 0f, _curArmR = 0f;

        void Awake() { Build(); }

        /// <summary>
        /// 由 PlayerController 每帧调用：传入当前水平移动速度，驱动关节。
        /// </summary>
        public void SetLocomotion(float speed, float dt)
        {
            _t += dt;
            bool moving = speed > 0.06f;
            if (moving) _stride += dt * (walkFreqBase + Mathf.Min(speed, 5f) * 1.1f);

            float swing = moving ? Mathf.Sin(_stride) * (walkAmp + Mathf.Min(speed, 5f) * 0.12f) : 0f;
            float legL = swing * 42f, legR = -swing * 42f;     // 腿反相
            float armL = -swing * 38f, armR = swing * 38f;     // 手臂与对侧腿同相

            float k = 1f - Mathf.Exp(-12f * dt);               // 平滑插值，避免突变
            _curLegL = Mathf.Lerp(_curLegL, legL, k);
            _curLegR = Mathf.Lerp(_curLegR, legR, k);
            _curArmL = Mathf.Lerp(_curArmL, armL, k);
            _curArmR = Mathf.Lerp(_curArmR, armR, k);

            _lLeg.localEulerAngles = new Vector3(_curLegL, 0f, 0f);
            _rLeg.localEulerAngles = new Vector3(_curLegR, 0f, 0f);
            _lArm.localEulerAngles = new Vector3(_curArmL, 0f, 0f);
            _rArm.localEulerAngles = new Vector3(_curArmR, 0f, 0f);

            // 待机呼吸
            float bob = moving ? 0f : Mathf.Sin(_t * 2.1f) * idleBob;
            _body.localPosition = new Vector3(0f, bob, 0f);

            // 面罩微光脉动
            if (_visorMat != null)
            {
                float p = 0.6f + 0.4f * Mathf.Sin(_t * 3f);
                _visorMat.SetColor("_EmissiveColor", new Color(0.15f * p, 0.85f * p, 1f * p));
            }
        }

        // ---------- 建模 ----------
        void Build()
        {
            Color suit = new Color(0.16f, 0.22f, 0.34f);   // 制服蓝
            Color suitDark = new Color(0.09f, 0.12f, 0.18f);
            Color steel = new Color(0.62f, 0.70f, 0.82f);  // 头盔/手套钢色
            Color cyan = new Color(0.15f, 0.95f, 1f);       // 面罩青
            Color core = new Color(0.2f, 1f, 0.8f);         // 胸核绿青

            _body = new GameObject("Body").transform;
            _body.SetParent(transform, false);

            var torso = Part(PrimitiveType.Cube, new Vector3(0.5f, 0.7f, 0.28f), MakeMat(suit, Color.black, 0.3f));
            torso.transform.SetParent(_body, false);
            torso.transform.localPosition = new Vector3(0f, 1.25f, 0f);

            var hips = Part(PrimitiveType.Cube, new Vector3(0.46f, 0.18f, 0.26f), MakeMat(suitDark, Color.black, 0.3f));
            hips.transform.SetParent(_body, false);
            hips.transform.localPosition = new Vector3(0f, 0.92f, 0f);

            var coreM = Part(PrimitiveType.Cube, new Vector3(0.16f, 0.16f, 0.07f), MakeMat(suitDark, core, 0.4f));
            coreM.transform.SetParent(_body, false);
            coreM.transform.localPosition = new Vector3(0f, 1.32f, 0.16f);

            var pack = Part(PrimitiveType.Cube, new Vector3(0.34f, 0.42f, 0.18f), MakeMat(suitDark, Color.black, 0.25f));
            pack.transform.SetParent(_body, false);
            pack.transform.localPosition = new Vector3(0f, 1.18f, -0.2f);

            // 头
            _head = new GameObject("Head").transform;
            _head.SetParent(_body, false);
            _head.localPosition = new Vector3(0f, 1.72f, 0f);

            var head = Part(PrimitiveType.Sphere, new Vector3(0.34f, 0.38f, 0.34f), MakeMat(steel, Color.black, 0.3f));
            head.transform.SetParent(_head, false);

            var helmet = Part(PrimitiveType.Sphere, new Vector3(0.37f, 0.3f, 0.37f), MakeMat(suit, Color.black, 0.3f));
            helmet.transform.SetParent(_head, false);
            helmet.transform.localPosition = new Vector3(0f, 0.06f, 0f);

            _visorMat = MakeMat(suit, cyan, 0.2f);
            var visor = Part(PrimitiveType.Cube, new Vector3(0.3f, 0.12f, 0.06f), _visorMat);
            visor.transform.SetParent(_head, false);
            visor.transform.localPosition = new Vector3(0f, 0f, 0.17f);
            visor.transform.localRotation = Quaternion.Euler(8f, 0f, 0f);

            // 手臂（肩枢轴）
            _lArm = MakeLimb(new Vector3(-0.3f, 1.55f, 0f),
                Part(PrimitiveType.Cube, new Vector3(0.14f, 0.32f, 0.16f), MakeMat(suit, Color.black, 0.3f)),
                Part(PrimitiveType.Cube, new Vector3(0.12f, 0.3f, 0.14f), MakeMat(steel, Color.black, 0.3f)),
                new Vector3(0f, -0.16f, 0f), new Vector3(0f, -0.46f, 0f));
            _rArm = MakeLimb(new Vector3(0.3f, 1.55f, 0f),
                Part(PrimitiveType.Cube, new Vector3(0.14f, 0.32f, 0.16f), MakeMat(suit, Color.black, 0.3f)),
                Part(PrimitiveType.Cube, new Vector3(0.12f, 0.3f, 0.14f), MakeMat(steel, Color.black, 0.3f)),
                new Vector3(0f, -0.16f, 0f), new Vector3(0f, -0.46f, 0f));
            _lArm.SetParent(_body, false); _rArm.SetParent(_body, false);

            // 腿（髋枢轴）
            _lLeg = MakeLeg(new Vector3(-0.13f, 0.9f, 0f),
                Part(PrimitiveType.Cube, new Vector3(0.17f, 0.42f, 0.2f), MakeMat(suitDark, Color.black, 0.3f)),
                Part(PrimitiveType.Cube, new Vector3(0.16f, 0.42f, 0.2f), MakeMat(suitDark, Color.black, 0.3f)),
                Part(PrimitiveType.Cube, new Vector3(0.18f, 0.1f, 0.26f), MakeMat(suitDark, Color.black, 0.3f)),
                new Vector3(0f, -0.21f, 0f), new Vector3(0f, -0.63f, 0f), new Vector3(0f, -0.86f, 0.05f));
            _rLeg = MakeLeg(new Vector3(0.13f, 0.9f, 0f),
                Part(PrimitiveType.Cube, new Vector3(0.17f, 0.42f, 0.2f), MakeMat(suitDark, Color.black, 0.3f)),
                Part(PrimitiveType.Cube, new Vector3(0.16f, 0.42f, 0.2f), MakeMat(suitDark, Color.black, 0.3f)),
                Part(PrimitiveType.Cube, new Vector3(0.18f, 0.1f, 0.26f), MakeMat(suitDark, Color.black, 0.3f)),
                new Vector3(0f, -0.21f, 0f), new Vector3(0f, -0.63f, 0f), new Vector3(0f, -0.86f, 0.05f));
            _lLeg.SetParent(_body, false); _rLeg.SetParent(_body, false);
        }

        // 模板材质（克隆自场景已有渲染器，其着色器必被打包进构建，规避 Shader.Find 在 WebGL 失效）
        private static Material _template;

        // 手臂：pivot -> 上臂 + 前臂（绕肩旋转）
        Transform MakeLimb(Vector3 pivotPos, GameObject upper, GameObject lower, Vector3 upperOff, Vector3 lowerOff)
        {
            var pivot = new GameObject("ArmPivot").transform;
            pivot.localPosition = pivotPos;
            upper.transform.SetParent(pivot, false); upper.transform.localPosition = upperOff;
            lower.transform.SetParent(pivot, false); lower.transform.localPosition = lowerOff;
            return pivot;
        }

        // 腿：pivot -> 大腿 + 小腿 + 脚（绕髋旋转）
        Transform MakeLeg(Vector3 pivotPos, GameObject upper, GameObject lower, GameObject foot, Vector3 upperOff, Vector3 lowerOff, Vector3 footOff)
        {
            var pivot = new GameObject("LegPivot").transform;
            pivot.localPosition = pivotPos;
            upper.transform.SetParent(pivot, false); upper.transform.localPosition = upperOff;
            lower.transform.SetParent(pivot, false); lower.transform.localPosition = lowerOff;
            foot.transform.SetParent(pivot, false); foot.transform.localPosition = footOff;
            return pivot;
        }

        // 用基础几何体生成一个"平着色"部件（去掉碰撞体，避免无谓物理开销）
        GameObject Part(PrimitiveType type, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(type);
            var mf = go.GetComponent<MeshFilter>();
            mf.sharedMesh = FlatShade(mf.sharedMesh);
            go.GetComponent<MeshRenderer>().material = mat;
            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);
            go.transform.localScale = scale;
            return go;
        }

        // 复制顶点使其不共享 -> 重算法线 -> 得到 Low Poly 刻面效果
        static Mesh FlatShade(Mesh src)
        {
            if (src == null) return null;
            var verts = src.vertices;
            var norms = src.normals;
            var uvs = src.uv;
            var tris = src.triangles;
            int n = tris.Length;
            var nv = new Vector3[n];
            var nu = new Vector2[n];
            var nt = new int[n];
            for (int i = 0; i < n; i++)
            {
                int id = tris[i];
                nv[i] = verts[id];
                nu[i] = uvs.Length > id ? uvs[id] : Vector2.zero;
                nt[i] = i;
            }
            var m = new Mesh
            {
                vertices = nv,
                uv = nu,
                triangles = nt
            };
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }

        static Material MakeMat(Color baseCol, Color emi, float smooth)
        {
            if (_template == null)
            {
                var r = Object.FindObjectOfType<Renderer>();
                _template = r != null ? r.sharedMaterial : null;
            }
            Material m;
            if (_template != null) m = new Material(_template);          // 克隆：继承已包含进构建的着色器
            else
            {
                Shader sh = Shader.Find("Universal Render Pipeline/Lit");
                if (sh == null) sh = Shader.Find("Standard");
                m = new Material(sh);
            }
            m.color = baseCol;
            m.SetColor("_BaseColor", baseCol);
            if (emi != Color.black)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissiveColor", emi);
            }
            m.SetFloat("_Smoothness", smooth);
            m.SetFloat("_Metallic", 0f);
            return m;
        }
    }
}
