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
    }

    // TODO: TouchProvider / MouseProvider 实现
}
