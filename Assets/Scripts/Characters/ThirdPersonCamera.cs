// S4 — 第三人称跟随相机：球面坐标环绕 + 平滑跟随 + 拖拽环绕 + 滚轮缩放
using UnityEngine;

namespace StarIsle.Characters
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        public Transform target;
        public float distance = 6f;
        public float height = 2.2f;       // 注视点高度（胸口偏上）
        public float yaw = 0f;             // 水平角（度）
        public float pitch = 14f;          // 俯仰角（度）
        public float minPitch = -5f, maxPitch = 55f;
        public float minDist = 3f, maxDist = 14f;
        public float followLerp = 8f;

        private bool _inited = false;
        private Vector3 _curPos;

        void LateUpdate()
        {
            if (target == null) return;

            float ry = yaw * Mathf.Deg2Rad;
            float rp = pitch * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(
                Mathf.Sin(ry) * Mathf.Cos(rp),
                Mathf.Sin(rp),
                Mathf.Cos(ry) * Mathf.Cos(rp)) * distance;

            Vector3 desired = target.position + Vector3.up * height + offset;

            if (!_inited) { _curPos = desired; _inited = true; }
            else _curPos = Vector3.Lerp(_curPos, desired, 1f - Mathf.Exp(-followLerp * Time.deltaTime));

            transform.position = _curPos;
            transform.LookAt(target.position + Vector3.up * height * 0.9f);
        }

        public void Orbit(float dx, float dy)
        {
            yaw += dx;
            pitch = Mathf.Clamp(pitch - dy, minPitch, maxPitch);
        }

        public void Zoom(float d)
        {
            distance = Mathf.Clamp(distance - d, minDist, maxDist);
        }
    }
}
