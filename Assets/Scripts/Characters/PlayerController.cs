// S4 — 第三人称玩家控制器：键鼠(WASD/方向键) + 触屏/鼠标 点击移动 + 拖拽转视角
using UnityEngine;
using UnityEngine.InputSystem;

namespace StarIsle.Characters
{
    [RequireComponent(typeof(ColonistAvatar))]
    public class PlayerController : MonoBehaviour
    {
        [Header("移动")]
        public float moveSpeed = 4.5f;
        public float turnLerp = 12f;
        public float boundRadius = 8.5f;   // 限制在岛面范围内（地面 20x20，留边）

        [Header("过场控制")]
        public bool InputLocked = false;    // 开场/过场期间为 true：不读输入、不移动

        private ColonistAvatar _avatar;
        private Camera _cam;
        private ThirdPersonCamera _tpc;
        private Vector3 _prevPos;
        private Vector3? _moveTarget;

        // 指针：轻点=移动指令；拖拽=转视角
        private bool _pointerDown = false, _dragging = false;
        private Vector2 _downScreen;
        private const float DRAG_THRESH = 6f;
        private float _orbitSens = 0.25f;

        void Awake()
        {
            _avatar = GetComponent<ColonistAvatar>();
            _prevPos = transform.position;
        }

        // 延迟接线：避免与 GameManager 的 Awake/Start 顺序耦合
        void EnsureLinks()
        {
            if (_cam == null)
            {
                _cam = Camera.main;
                if (_cam != null)
                {
                    _tpc = _cam.GetComponent<ThirdPersonCamera>();
                    if (_tpc != null && _tpc.target == null) _tpc.target = transform;
                }
            }
        }

        void Update()
        {
            // 过场期间锁定：不读输入、不移动，仅保留待机呼吸动画
            if (InputLocked)
            {
                _avatar.SetLocomotion(0f, Time.deltaTime);
                _prevPos = transform.position;
                return;
            }

            EnsureLinks();
            HandlePointer();

            Vector3 dir = ReadKeyboard();
            if (dir.sqrMagnitude > 0.001f)
            {
                _moveTarget = null;
            }
            else if (_moveTarget.HasValue)
            {
                dir = _moveTarget.Value - transform.position;
                dir.y = 0f;
                if (dir.magnitude < 0.18f) { dir = Vector3.zero; _moveTarget = null; }
                else dir = dir.normalized;
            }

            if (dir.sqrMagnitude > 0.001f)
            {
                Vector3 np = transform.position + dir.normalized * moveSpeed * Time.deltaTime;
                Vector2 xz = new Vector2(np.x, np.z);
                if (xz.magnitude > boundRadius) { xz = xz.normalized * boundRadius; np.x = xz.x; np.z = xz.y; }
                np.y = 0f;
                transform.position = np;

                Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot,
                    1f - Mathf.Exp(-turnLerp * Time.deltaTime));
            }

            float sp = Vector3.Distance(_prevPos, transform.position) / Mathf.Max(Time.deltaTime, 1e-4f);
            _avatar.SetLocomotion(sp, Time.deltaTime);
            _prevPos = transform.position;
        }

        // 键盘：WASD / 方向键，并转换为"相机相对"方向
        Vector3 ReadKeyboard()
        {
            Vector3 v = Vector3.zero;
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) v.z += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) v.z -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) v.x += 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) v.x -= 1f;
            }
            if (v.sqrMagnitude > 1f) v.Normalize();
            if (_cam != null && v.sqrMagnitude > 0.001f)
            {
                Vector3 f = _cam.transform.forward; f.y = 0f; f.Normalize();
                Vector3 r = _cam.transform.right; r.y = 0f; r.Normalize();
                v = f * v.z + r * v.x;
            }
            return v;
        }

        void HandlePointer()
        {
            bool leftDown = (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                         || (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame);
            bool leftUp = (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
                       || (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame);

            Vector2 delta = Vector2.zero, pos = Vector2.zero;
            if (Mouse.current != null) { delta = Mouse.current.delta.ReadValue(); pos = Mouse.current.position.ReadValue(); }
            else if (Touchscreen.current != null) { delta = Touchscreen.current.primaryTouch.delta.ReadValue(); pos = Touchscreen.current.primaryTouch.position.ReadValue(); }

            // PC：右键拖拽转视角 + 滚轮缩放
            if (Mouse.current != null)
            {
                if (Mouse.current.rightButton.isPressed)
                {
                    var d = Mouse.current.delta.ReadValue();
                    if (_tpc != null) _tpc.Orbit(d.x * _orbitSens, d.y * _orbitSens);
                }
                float s = Mouse.current.scroll.ReadValue().y;
                if (s != 0f && _tpc != null) _tpc.Zoom(s * 0.01f);
            }

            if (leftDown) { _pointerDown = true; _dragging = false; _downScreen = pos; }
            if (_pointerDown)
            {
                if (!_dragging && Vector2.Distance(pos, _downScreen) > DRAG_THRESH) _dragging = true;
                if (_dragging && _tpc != null) _tpc.Orbit(delta.x * _orbitSens, delta.y * _orbitSens);
            }
            if (leftUp)
            {
                if (_pointerDown && !_dragging) IssueMove(pos);
                _pointerDown = false; _dragging = false;
            }
        }

        // 屏幕点 -> 与 y=0 岛面求交 -> 设置移动目标
        void IssueMove(Vector2 screenPos)
        {
            if (_cam == null) return;
            Ray ray = _cam.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y, 0f));
            Plane ground = new Plane(Vector3.up, Vector3.zero);
            if (ground.Raycast(ray, out float dist))
            {
                Vector3 p = ray.GetPoint(dist);
                Vector2 xz = new Vector2(p.x, p.z);
                if (xz.magnitude > boundRadius) xz = xz.normalized * boundRadius;
                _moveTarget = new Vector3(xz.x, 0f, xz.y);
            }
        }
    }
}
