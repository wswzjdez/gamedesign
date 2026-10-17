// S3 — 生物系统：孵化、物种、图鉴、中心化模拟
using System.Collections.Generic;
using UnityEngine;
using StarIsle.Systems;
using StarIsle.Config;
using StarIsle.Simulation;

namespace StarIsle.Bio
{
    public class BioSystem : MonoBehaviour
    {
        public int Count => _creatures.Count;
        public List<CreatureSpecies> allSpecies = new();
        private readonly List<CreatureSpecies> _available = new();
        private readonly List<BioAgent> _creatures = new();
        private BioBrain _brain;
        private Codex _codex;
        private SimConfig _cfg;

        public void Init(Codex codex, SimConfig cfg)
        {
            _codex = codex; _cfg = cfg;
            _brain = GetComponent<BioBrain>();
            if (_brain == null) _brain = gameObject.AddComponent<BioBrain>();
            if (_available.Count == 0 && allSpecies.Count > 0) _available.Add(allSpecies[0]);
        }

        public void UnlockSpecies(string id)
        {
            var s = allSpecies.Find(x => x.id == id);
            if (s != null && !_available.Contains(s)) _available.Add(s);
        }

        public BioAgent Hatch(Vector3 pos)
        {
            if (_available.Count == 0) return null;
            var species = _available[Random.Range(0, _available.Count)];
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Creature_" + Count;
            go.transform.position = pos;
            var a = go.AddComponent<BioAgent>();
            a.Init(this, _brain, species);
            _creatures.Add(a);
            if (_codex != null) _codex.Discover(species.id);
            return a;
        }

        public void Tick(float dt, SimConfig cfg, ResourceSystem res, EcoSim eco)
        {
            var births = new List<Vector3>();
            for (int i = _creatures.Count - 1; i >= 0; i--)
            {
                var c = _creatures[i];
                if (c == null) { _creatures.RemoveAt(i); continue; }
                c.Act(dt, cfg, eco);

                // 中心化模拟：耗水 → 产生物质 / 饥饿
                if (res.TrySpend("water", cfg.creatureWaterCost))
                {
                    c.Energy = Mathf.Min(1.2f, c.Energy + cfg.creatureEnergyGain);
                    res.Add("biomass", cfg.creatureBiomassYield);
                }
                else
                {
                    c.Energy -= cfg.creatureStarveRate;
                }
                c.SetEnergyVisual();

                // AI 驱动繁殖（演示涌现）：高能量 + 有容量 + 付得起 → 父母附近孵化
                if (c.Energy > 0.9f && Count + births.Count < cfg.maxPopulation
                    && res.TrySpend("biomass", cfg.hatchBiomassCost * 0.5f))
                {
                    births.Add(c.transform.position + Random.insideUnitSphere * 1f);
                }

                if (c.Energy <= 0f) { Destroy(c.gameObject); _creatures.RemoveAt(i); }
            }
            foreach (var p in births) Hatch(new Vector3(p.x, 0.5f, p.z));
        }

        public void Remove(BioAgent a) => _creatures.Remove(a);
    }
}
