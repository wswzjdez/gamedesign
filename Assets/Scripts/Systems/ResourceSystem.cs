// S2 — 资源经济：光/水/矿/生物质
using System.Collections.Generic;
using UnityEngine;
using StarIsle.Config;

namespace StarIsle.Systems
{
    public class ResourceSystem : MonoBehaviour
    {
        private readonly Dictionary<string, float> _amount = new();

        public void Ensure(string id, float initial = 0f)
        {
            if (!_amount.ContainsKey(id)) _amount[id] = initial;
        }
        public void Set(string id, float v) { Ensure(id); _amount[id] = v; }
        public void Add(string id, float d) { Ensure(id); _amount[id] += d; }
        public bool TrySpend(string id, float cost)
        {
            Ensure(id);
            if (_amount[id] < cost) return false;
            _amount[id] -= cost;
            return true;
        }
        public float Get(string id) => _amount.TryGetValue(id, out var v) ? v : 0f;

        // 被动产出：光 + 水（水生态达标后由 EcoSim 决定，这里只做基础回补）
        public void Tick(float dt, SimConfig cfg)
        {
            Add("light", cfg.lightRate * dt);
            Add("water", cfg.waterPassiveRate * dt);
        }
    }
}
