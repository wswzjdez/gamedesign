// S3 — 游戏总控：状态机 + 子系统自举 + 玩家动作 + 科技/图鉴
using UnityEngine;
using StarIsle.Simulation;
using StarIsle.Systems;
using StarIsle.Bio;
using StarIsle.Config;
using StarIsle.Persistence;
using StarIsle.UI;
using System.Collections.Generic;

namespace StarIsle.Core
{
    public enum GameState { Boot, MainMenu, Playing, Paused }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        public GameState State = GameState.Boot;

        public EcoSim Eco => _eco;
        public ResourceSystem Res => _res;
        public BioSystem Bio => _bio;
        public TechTree Tech => _tech;
        public Codex Codex => _codex;
        public SimConfig Cfg => _cfg;

        [SerializeField] private SimConfig _configAsset;
        private EcoSim _eco; private ResourceSystem _res; private BioSystem _bio;
        private SaveSystem _save; private Hud _hud;
        private TechTree _tech; private Codex _codex;
        private SimConfig _cfg;

        private void Awake()
        {
            Instance = this;
            _cfg = _configAsset != null ? _configAsset : ScriptableObject.CreateInstance<SimConfig>();

            EnsureSub(ref _res);
            EnsureSub(ref _bio);
            EnsureSub(ref _eco);
            EnsureSub(ref _save);
            EnsureSub(ref _tech);
            EnsureSub(ref _codex);
            EnsureSub(ref _hud);

            _res.Ensure("light", 20f); _res.Ensure("water", 10f);
            _res.Ensure("mineral", 0f); _res.Ensure("biomass", 0f);

            CreateDefaultContent();
            _eco.Init(_res, _bio, _cfg);
            _bio.Init(_codex, _cfg);
            _tech.Init(_eco, _cfg, _bio);

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.transform.localScale = Vector3.one * 2f;
            ground.name = "IslandGround";

            State = GameState.Playing;
        }

        // 无 SO 资产时，运行时生成默认科技与物种，保证"打开即玩"
        private void CreateDefaultContent()
        {
            if (_bio.allSpecies.Count == 0)
            {
                var seed = ScriptableObject.CreateInstance<CreatureSpecies>();
                seed.id = "seed"; seed.displayName = "原核芽体"; seed.color = Color.green; seed.scale = 0.5f;
                var glow = ScriptableObject.CreateInstance<CreatureSpecies>();
                glow.id = "glow"; glow.displayName = "荧藻兽"; glow.color = Color.cyan; glow.scale = 0.6f;
                _bio.allSpecies.Add(seed); _bio.allSpecies.Add(glow);
            }
            if (_tech.allTech.Count == 0)
            {
                _tech.allTech.Add(MakeTech("biofactory", "生物质增产", null, null,
                    new[] { new TechEffect { kind = TechEffectKind.BoostBiomass, value = 0.6f } },
                    new ResourceCost { id = "biomass", amount = 60f }));
                _tech.allTech.Add(MakeTech("purifier2", "强化净化", null, null,
                    new[] { new TechEffect { kind = TechEffectKind.BoostPurifier, value = 0.05f } },
                    new ResourceCost { id = "mineral", amount = 40f }));
                _tech.allTech.Add(MakeTech("recycler2", "强化水循环", null, null,
                    new[] { new TechEffect { kind = TechEffectKind.BoostRecycler, value = 0.06f } },
                    new ResourceCost { id = "mineral", amount = 30f }));
                _tech.allTech.Add(MakeTech("species_glow", "解锁·荧藻兽", null, new[] { "biofactory" },
                    new[] { new TechEffect { kind = TechEffectKind.UnlockSpecies, target = "glow" } },
                    new ResourceCost { id = "biomass", amount = 80f },
                    new ResourceCost { id = "mineral", amount = 30f }));
            }
        }

        private TechDef MakeTech(string id, string name, string desc, string[] prereq, TechEffect[] eff, params ResourceCost[] cost)
        {
            var t = ScriptableObject.CreateInstance<TechDef>();
            t.id = id; t.displayName = name; t.description = desc;
            t.prerequisites = prereq;
            t.effects = new List<TechEffect>(eff);
            t.cost = new List<ResourceCost>(cost);
            return t;
        }

        private void EnsureSub<T>(ref T field) where T : MonoBehaviour
        {
            field = GetComponent<T>();
            if (field == null) field = gameObject.AddComponent<T>();
        }

        // ---------- 玩家动作 ----------
        public void Gather() { if (State == GameState.Playing) _res.Add("mineral", _cfg.mineralPerGather); }
        public void BuildPurifier() { if (_res.TrySpend("mineral", _cfg.purifierCost)) _eco.Purifiers++; }
        public void BuildRecycler() { if (_res.TrySpend("mineral", _cfg.recyclerCost)) _eco.Recyclers++; }
        public void Hatch()
        {
            if (State != GameState.Playing) return;
            if (_eco.Atmosphere < _cfg.hatchAtmoReq || _eco.Hydro < _cfg.hatchHydroReq) return;
            if (!_res.TrySpend("biomass", _cfg.hatchBiomassCost)) return;
            Vector3 p = new Vector3(Random.Range(-3f, 3f), 0.5f, Random.Range(-3f, 3f));
            _bio.Hatch(p);
        }
        public void UnlockTech(string id)
        {
            if (_tech.TryUnlock(id, _res)) _hud.RefreshTechButtons();
        }

        // ---------- 存档 ----------
        public void Save()
        {
            _save.SaveGame(new SaveData
            {
                atmosphere = _eco.Atmosphere, hydro = _eco.Hydro, biodiversity = _eco.Biodiversity,
                purifiers = _eco.Purifiers, recyclers = _eco.Recyclers,
                light = _res.Get("light"), water = _res.Get("water"),
                mineral = _res.Get("mineral"), biomass = _res.Get("biomass"),
                creatures = _bio.Count
            });
        }
        public void Load()
        {
            if (!_save.HasSave()) return;
            var d = _save.LoadGame();
            _eco.Atmosphere = d.atmosphere; _eco.Hydro = d.hydro; _eco.Biodiversity = d.biodiversity;
            _eco.Purifiers = d.purifiers; _eco.Recyclers = d.recyclers;
            _res.Set("light", d.light); _res.Set("water", d.water);
            _res.Set("mineral", d.mineral); _res.Set("biomass", d.biomass);
            // 生物实例 S3 仍不重建（仅恢复数值）
        }
    }
}
