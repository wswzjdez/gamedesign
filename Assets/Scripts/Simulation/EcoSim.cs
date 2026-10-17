// S3 — 生态模拟核心：按 tick 推进生态指标，驱动全局模拟
using UnityEngine;
using StarIsle.Systems;
using StarIsle.Bio;
using StarIsle.Config;

namespace StarIsle.Simulation
{
    public class EcoSim : MonoBehaviour
    {
        // 生态指标（0..1）
        public float Atmosphere, Hydro, Biodiversity;
        public int Purifiers, Recyclers;

        private float _tickAccum;
        private ResourceSystem _res;
        private BioSystem _bio;
        private SimConfig _cfg;

        public void Init(ResourceSystem res, BioSystem bio, SimConfig cfg)
        {
            _res = res; _bio = bio; _cfg = cfg;
            Atmosphere = cfg.startAtmosphere;
            Hydro = cfg.startHydro;
            Biodiversity = cfg.startBiodiversity;
        }

        private void Update()
        {
            if (_cfg == null) return;
            _tickAccum += Time.deltaTime;
            while (_tickAccum >= _cfg.tickInterval)
            {
                _tickAccum -= _cfg.tickInterval;
                Tick(_cfg.tickInterval);
            }
        }

        private void Tick(float dt)
        {
            float atmoTarget = Mathf.Min(1f, Purifiers * _cfg.atmospherePerPurifier);
            float hydroTarget = Mathf.Min(1f, 0.3f + Recyclers * _cfg.hydroPerRecycler);
            Atmosphere = Mathf.MoveTowards(Atmosphere, atmoTarget, dt * 0.15f);
            Hydro = Mathf.MoveTowards(Hydro, hydroTarget, dt * 0.15f);

            _res.Tick(dt, _cfg);
            _bio?.Tick(dt, _cfg, _res, this);

            float bioNorm = Mathf.Min(1f, _res.Get("biomass") / 100f);
            float countTerm = _bio != null ? _bio.Count * 0.12f : 0f;
            Biodiversity = Mathf.Clamp01(countTerm + bioNorm * 0.5f);
        }
    }
}
