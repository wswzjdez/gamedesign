// Scaffold — 跨端输入抽象：键鼠(PC) / 触屏(Web移动+手机)
using UnityEngine;

namespace StarIsle.Input
{
    public interface IInputProvider { Vector2 Pointer { get; } bool Tap { get; } }

    public class InputRouter : MonoBehaviour
    {
        private IInputProvider _provider;

        private void Awake()
        {
#if UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL_TOUCH
            _provider = new TouchProvider();
#else
            _provider = new MouseProvider();
#endif
        }

        public Vector2 Pointer => _provider != null ? _provider.Pointer : Vector2.zero;
        public bool Tap => _provider != null && _provider.Tap;
    }

    // 最小可用实现（脚手架阶段返回默认输入，后续可按平台补全手势/键鼠逻辑）
    // 注意：本文件处于 namespace StarIsle.Input，未限定的 Input 会被解析为命名空间，
    // 因此此处显式使用 UnityEngine.Input 指代 UnityEngine 的输入类。
    public class MouseProvider : IInputProvider
    {
        public Vector2 Pointer => UnityEngine.Input.mousePosition;
        public bool Tap => UnityEngine.Input.GetMouseButtonDown(0);
    }

    public class TouchProvider : IInputProvider
    {
        public Vector2 Pointer => UnityEngine.Input.touchCount > 0 ? UnityEngine.Input.GetTouch(0).position : Vector2.zero;
        public bool Tap => UnityEngine.Input.touchCount > 0 && UnityEngine.Input.GetTouch(0).phase == TouchPhase.Began;
    }
}
