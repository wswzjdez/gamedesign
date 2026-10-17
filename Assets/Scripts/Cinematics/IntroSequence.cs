// S4 — 开场动画（Opening Cinematic）
// 黑场淡入 → 标题浮现 → 高空俯冲推进 → 殖民者降临(能量涟漪) → 平滑交还第三人称操控
// 全程程序化驱动，零外部资源；任意键/点击/触屏可跳过。
using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using StarIsle.Characters;

namespace StarIsle.Cinematics
{
    [Serializable]
    public class IntroShot
    {
        public Vector3 pos;
        public Vector3 look;
        public float dur = 2f;      // 从上一镜头过渡到本镜头所用时长
    }

    public class IntroSequence : MonoBehaviour
    {
        [Header("时间轴（秒）")]
        public float blackFade = 0.9f;        // 黑幕淡出时长
        public float titleIn = 0.25f;         // 标题开始淡入
        public float titleFull = 2.2f;        // 标题完全显示
        public float titleOutStart = 3.6f;    // 标题开始淡出
        public float titleOutEnd = 4.4f;      // 标题完全消失
        public float dropStart = 4.2f;        // 殖民者开始降临
        public float dropEnd = 5.1f;          // 落地
        public float dropHeight = 9f;         // 降临起始高度
        public float rippleDur = 0.6f;        // 能量涟漪时长

        [Header("镜头关键帧")]
        public IntroShot[] shots;

        [Header("文案")]
        public string titleText = "星岛计划";
        public string subText = "STAR ISLE PROJECT";
        public string hintText = "点击 / 按任意键 跳过";

        private Camera _cam;
        private Transform _player;
        private ThirdPersonCamera _tpc;
        private PlayerController _pc;
        private Action _onDone;

        private bool _playing, _uiBuilt, _dropped, _landed;
        private float _t, _total;

        private UIDocument _doc;
        private VisualElement _black, _titleWrap;

        private GameObject _ripple;
        private Material _rippleMat;

        void Awake() { EnsureUI(); }

        /// <summary>开始播放开场动画。</summary>
        public void Play(Transform player, ThirdPersonCamera tpc, Action onDone)
        {
            _player = player;
            _tpc = tpc;
            _onDone = onDone;
            _pc = player != null ? player.GetComponent<PlayerController>() : null;
            _cam = Camera.main;

            // 极端情况：场景无主相机时不能走 Finish（其 _playing 守卫会直接 return，
            // 导致 onDone 永不回调、游戏永久卡在 Intro 状态）。这里直接放行。
            if (_cam == null)
            {
                if (_player != null) _player.gameObject.SetActive(true);
                if (_pc != null) _pc.InputLocked = false;
                if (_tpc != null) { _tpc.target = _player; _tpc.enabled = true; }
                _onDone?.Invoke();
                return;
            }

            if (shots == null || shots.Length < 2) shots = DefaultShots();
            _total = 0f;
            for (int i = 1; i < shots.Length; i++) _total += Mathf.Max(shots[i].dur, 0.01f);

            // 交场期间：禁跟随相机、锁玩家输入、角色先隐藏（等降临）
            if (_tpc != null) { _tpc.target = _player; _tpc.enabled = false; }
            if (_pc != null) _pc.InputLocked = true;
            if (_player != null) _player.gameObject.SetActive(false);

            EnsureUI();
            if (_doc != null) _doc.rootVisualElement.style.display = DisplayStyle.Flex;

            _t = 0f; _playing = true; _dropped = false; _landed = false;
        }

        void Update()
        {
            if (!_playing) return;

            if (SkipPressed()) { Finish(); return; }

            _t += Time.deltaTime;
            UpdateCamera(_t);
            UpdateUI(_t);
            UpdateDrop(_t);
            UpdateRipple(_t);

            if (_t >= _total) Finish();
        }

        // ---------- 镜头：沿关键帧插值 ----------
        void UpdateCamera(float t)
        {
            float acc = 0f;
            for (int i = 0; i < shots.Length - 1; i++)
            {
                float d = Mathf.Max(shots[i + 1].dur, 0.01f);
                if (t <= acc + d || i == shots.Length - 2)
                {
                    float p = Mathf.Clamp01((t - acc) / d);
                    float e = p * p * (3f - 2f * p);   // smoothstep，起落更柔和
                    Vector3 pos = Vector3.Lerp(shots[i].pos, shots[i + 1].pos, e);
                    Vector3 look = Vector3.Lerp(shots[i].look, shots[i + 1].look, e);
                    _cam.transform.position = pos;
                    Vector3 fwd = look - pos;
                    if (fwd.sqrMagnitude > 1e-6f)
                        _cam.transform.rotation = Quaternion.LookRotation(fwd.normalized);
                    return;
                }
                acc += d;
            }
        }

        // ---------- 黑幕 / 标题 ----------
        void UpdateUI(float t)
        {
            if (_black != null)
            {
                float bo = 1f - Mathf.Clamp01(t / Mathf.Max(blackFade, 0.01f));
                _black.style.opacity = bo;
                _black.style.display = bo <= 0.001f ? DisplayStyle.None : DisplayStyle.Flex;
            }
            if (_titleWrap != null)
            {
                float to;
                if (t < titleIn) to = 0f;
                else if (t < titleFull) to = (t - titleIn) / Mathf.Max(titleFull - titleIn, 0.01f);
                else if (t < titleOutStart) to = 1f;
                else if (t < titleOutEnd) to = 1f - (t - titleOutStart) / Mathf.Max(titleOutEnd - titleOutStart, 0.01f);
                else to = 0f;
                to = Mathf.Clamp01(to);
                _titleWrap.style.opacity = to;
                _titleWrap.style.top = (1f - to) * 30f;   // 淡入同时上浮
            }
        }

        // ---------- 殖民者降临 ----------
        void UpdateDrop(float t)
        {
            if (_player == null) return;
            if (!_dropped && t >= dropStart)
            {
                _dropped = true;
                _player.gameObject.SetActive(true);
            }
            if (!_dropped) return;

            float p = Mathf.Clamp01((t - dropStart) / Mathf.Max(dropEnd - dropStart, 0.01f));
            float e = 1f - Mathf.Pow(1f - p, 3f);   // easeOutCubic：急速下落、轻柔着地
            Vector3 pos = _player.position;
            pos.y = Mathf.Lerp(dropHeight, 0f, e);
            _player.position = pos;

            if (p >= 1f && !_landed)
            {
                _landed = true;
                SpawnRipple(_player.position);
            }
        }

        // ---------- 落地能量涟漪 ----------
        void SpawnRipple(Vector3 at)
        {
            Material tmpl = FindTemplateMaterial();
            if (tmpl == null) return;

            _ripple = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            var col = _ripple.GetComponent<Collider>();
            if (col != null) Destroy(col);
            _ripple.transform.position = new Vector3(at.x, 0.03f, at.z);
            _ripple.transform.localScale = new Vector3(0.6f, 0.01f, 0.6f);

            _rippleMat = new Material(tmpl);
            _rippleMat.EnableKeyword("_EMISSION");
            _rippleMat.SetColor("_EmissiveColor", new Color(0.2f, 0.9f, 1f));
            _ripple.GetComponent<Renderer>().material = _rippleMat;
        }

        void UpdateRipple(float t)
        {
            if (_ripple == null) return;
            float rp = (t - dropEnd) / Mathf.Max(rippleDur, 0.01f);
            if (rp >= 1f)
            {
                Destroy(_ripple);
                _ripple = null; _rippleMat = null;
                return;
            }
            float s = Mathf.Lerp(0.6f, 5f, rp);
            _ripple.transform.localScale = new Vector3(s, 0.01f, s);
            float f = 1f - rp;
            _rippleMat.SetColor("_EmissiveColor", new Color(0.2f * f, 0.9f * f, 1f * f));
        }

        // ---------- 结束 / 交还操控 ----------
        void Finish()
        {
            if (!_playing) return;
            _playing = false;

            if (_player != null)
            {
                Vector3 p = _player.position; p.y = 0f; _player.position = p;
                _player.gameObject.SetActive(true);
            }
            if (_pc != null) _pc.InputLocked = false;
            if (_tpc != null)
            {
                _tpc.target = _player;
                _tpc.enabled = true;
                _tpc.BeginBlendFrom(_tpc.transform.position);  // 从过场末尾平滑过渡到跟随位
            }
            _onDone?.Invoke();
            Destroy(gameObject);   // 过场只播一次，播完清理自身（含开场 UI 物体）
        }

        bool SkipPressed()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.anyKey.isPressed) return true;
            var m = Mouse.current;
            if (m != null && m.leftButton.wasPressedThisFrame) return true;
            var ts = Touchscreen.current;
            if (ts != null && ts.primaryTouch.press.wasPressedThisFrame) return true;
            return false;
        }

        // ---------- UI（UI Toolkit，与 HUD 同策略：运行时自举 PanelSettings） ----------
        void EnsureUI()
        {
            if (_uiBuilt) return;
            _uiBuilt = true;

            // 关键：UI 必须挂在独立子物体上。若与本物体(或 GameManager)上已有的 UIDocument
            // 共用，GetComponent 会拿到同一个文档，开场 UI 会和 HUD 元素混在一起。
            var uiGo = new GameObject("IntroUI");
            uiGo.transform.SetParent(transform, false);
            _doc = uiGo.AddComponent<UIDocument>();
            if (_doc.panelSettings == null)
            {
                _doc.panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
                _doc.panelSettings.sortingOrder = 10;   // 确保开场层盖在 HUD 之上
            }

            var root = _doc.rootVisualElement;
            root.style.justifyContent = Justify.Center;
            root.style.alignItems = Align.Center;

            // 黑幕（铺满，用于开场淡入）
            _black = new VisualElement();
            _black.style.position = Position.Absolute;
            _black.style.left = 0; _black.style.top = 0;
            _black.style.right = 0; _black.style.bottom = 0;
            _black.style.backgroundColor = new Color(0f, 0f, 0f, 1f);
            _black.style.opacity = 1f;
            root.Add(_black);

            // 标题
            _titleWrap = new VisualElement();
            _titleWrap.style.position = Position.Relative;
            _titleWrap.style.alignItems = Align.Center;
            _titleWrap.style.opacity = 0f;
            _titleWrap.style.top = 30f;

            var title = new Label(titleText);
            title.style.fontSize = 88f;
            title.style.color = new Color(0.92f, 0.98f, 1f);
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.unityTextAlign = TextAnchor.MiddleCenter;
            title.style.marginBottom = 6f;

            var sub = new Label(subText);
            sub.style.fontSize = 24f;
            sub.style.color = new Color(0.35f, 0.85f, 1f);
            sub.style.unityTextAlign = TextAnchor.MiddleCenter;
            sub.style.letterSpacing = 10f;

            _titleWrap.Add(title); _titleWrap.Add(sub);
            root.Add(_titleWrap);

            // 跳过提示
            var hint = new Label(hintText);
            hint.style.position = Position.Absolute;
            hint.style.bottom = 40f;
            hint.style.fontSize = 18f;
            hint.style.color = new Color(1f, 1f, 1f, 0.6f);
            hint.style.unityTextAlign = TextAnchor.MiddleCenter;
            root.Add(hint);

            // 默认隐藏：只有 Play() 真正开始过场才显示，避免异常路径下黑幕常驻遮屏
            root.style.display = DisplayStyle.None;
        }

        // 模板材质：优先取岛面（其着色器必被打包进构建，WebGL 安全）
        static Material FindTemplateMaterial()
        {
            var g = GameObject.Find("IslandGround");
            if (g != null)
            {
                var r = g.GetComponent<Renderer>();
                if (r != null && r.sharedMaterial != null) return r.sharedMaterial;
            }
            var any = UnityEngine.Object.FindObjectOfType<Renderer>();
            if (any != null && any.sharedMaterial != null) return any.sharedMaterial;
            Shader sh = Shader.Find("Universal Render Pipeline/Lit");
            if (sh == null) sh = Shader.Find("Standard");
            return sh != null ? new Material(sh) : null;
        }

        static IntroShot[] DefaultShots() => new[]
        {
            new IntroShot { pos = new Vector3(0f, 52f, 40f),  look = Vector3.zero,                 dur = 0.01f },  // 高空俯瞰
            new IntroShot { pos = new Vector3(16f, 22f, 26f), look = new Vector3(0f, 1f, 0f),       dur = 3.0f  },  // 斜向绕近（弧线）
            new IntroShot { pos = new Vector3(7f, 7.5f, 12f), look = new Vector3(0f, 1.4f, 0f),     dur = 2.4f  },  // 掠过岛面
            new IntroShot { pos = new Vector3(0f, 4.1f, 7.2f),look = new Vector3(0f, 1.7f, 0f),     dur = 1.6f  },  // 落到角色身后
        };
    }
}
