// S3 — 生物个体：BioBrain 决策移动；能量/生死由 BioSystem 中心化模拟
using UnityEngine;
using StarIsle.Config;
using StarIsle.Simulation;

namespace StarIsle.Bio
{
    public class BioAgent : MonoBehaviour
    {
        public float Energy = 1f;
        public float Age;
        public bool Alive = true;

        private BioBrain _brain;
        private CreatureSpecies _species;
        private Renderer _r;
        private Vector3 _target;
        private float _retarget;

        public void Init(BioSystem sys, BioBrain brain, CreatureSpecies species)
        {
            _brain = brain; _species = species;
            _r = GetComponent<Renderer>();
            if (_r != null && _species != null)
            {
                _r.material.color = _species.color;
                transform.localScale = Vector3.one * _species.scale;
            }
            PickTarget();
        }

        private void PickTarget()
        {
            _target = new Vector3(Random.Range(-4f, 4f), 0.5f, Random.Range(-4f, 4f));
            _retarget = Random.Range(1.5f, 3f);
        }

        // 行为决策 + 移动（资源/生死由 BioSystem.Tick 处理）
        public void Act(float dt, SimConfig cfg, EcoSim eco)
        {
            Age += dt;
            float[] d = _brain != null
                ? _brain.Decide(eco.Atmosphere, eco.Hydro, eco.Biodiversity, Energy)
                : new float[] { 1f - Energy, 1f - Energy, 0f, Energy };

            _retarget -= dt;
            if (_retarget <= 0f || Vector3.Distance(transform.position, _target) < 0.3f) PickTarget();

            Vector3 dir = (_target - transform.position).normalized;
            if (d[0] > 0.5f) dir = (Vector3.zero - transform.position).normalized; // 缺资源→回中心
            transform.position += dir * dt * 1.2f;
            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, -5f, 5f), 0.5f,
                Mathf.Clamp(transform.position.z, -5f, 5f));
        }

        public void SetEnergyVisual()
        {
            if (_r == null) return;
            Color baseCol = _species != null ? _species.color : Color.green;
            _r.material.color = Color.Lerp(Color.red, baseCol, Mathf.Clamp01(Energy));
        }
    }
}
