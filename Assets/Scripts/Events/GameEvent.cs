// Scaffold — 基于 ScriptableObject 的松散耦合事件
using UnityEngine;
using System;

namespace StarIsle.Events
{
    [CreateAssetMenu(menuName = "StarIsle/GameEvent")]
    public class GameEvent : ScriptableObject
    {
        private event Action _raise;
        public void Raise() => _raise?.Invoke();
        public void Subscribe(Action a) => _raise += a;
        public void Unsubscribe(Action a) => _raise -= a;
    }

    // 约定事件示例（在 Editor 中创建对应 SO 实例）：
    // OnResourceChanged, OnBiomeShift, OnBioHatched, OnTechUnlocked
}
