// S3 — 科技树系统（数据驱动，效果实时作用于运行时配置）
using System.Collections.Generic;
using UnityEngine;
using StarIsle.Config;
using StarIsle.Simulation;
using StarIsle.Bio;

namespace StarIsle.Systems
{
    public class TechTree : MonoBehaviour
    {
        public List<TechDef> allTech = new();
        private readonly HashSet<string> _unlocked = new();
        private EcoSim _eco; private SimConfig _cfg; private BioSystem _bio;

        public List<TechDef> AllTech => allTech;

        public void Init(EcoSim eco, SimConfig cfg, BioSystem bio)
        {
            _eco = eco; _cfg = cfg; _bio = bio;
        }

        public bool IsUnlocked(string id) => _unlocked.Contains(id);

        public bool CanUnlock(TechDef t, ResourceSystem res)
        {
            if (t == null || _unlocked.Contains(t.id)) return false;
            if (t.prerequisites != null)
                foreach (var p in t.prerequisites)
                    if (!_unlocked.Contains(p)) return false;
            if (t.cost != null)
                foreach (var c in t.cost)
                    if (res.Get(c.id) < c.amount) return false;
            return true;
        }

        public bool TryUnlock(string id, ResourceSystem res)
        {
            var t = allTech.Find(x => x.id == id);
            if (t == null || !CanUnlock(t, res)) return false;
            if (t.cost != null)
                foreach (var c in t.cost) res.TrySpend(c.id, c.amount);
            _unlocked.Add(t.id);
            Apply(t);
            return true;
        }

        private void Apply(TechDef t)
        {
            if (t.effects == null) return;
            foreach (var e in t.effects)
            {
                switch (e.kind)
                {
                    case TechEffectKind.BoostPurifier:    _cfg.atmospherePerPurifier += e.value; break;
                    case TechEffectKind.BoostRecycler:     _cfg.hydroPerRecycler += e.value; break;
                    case TechEffectKind.BoostBiomass:      _cfg.creatureBiomassYield += e.value; break;
                    case TechEffectKind.ReduceHatchCost:   _cfg.hatchBiomassCost = Mathf.Max(5f, _cfg.hatchBiomassCost - e.value); break;
                    case TechEffectKind.UnlockSpecies:     _bio.UnlockSpecies(e.target); break;
                }
            }
        }
    }
}
