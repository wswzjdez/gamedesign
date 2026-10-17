// S3 — 科技定义（数据驱动 ScriptableObject）
using System.Collections.Generic;
using UnityEngine;

namespace StarIsle.Config
{
    public enum TechEffectKind { BoostPurifier, BoostRecycler, BoostBiomass, ReduceHatchCost, UnlockSpecies }

    [System.Serializable]
    public struct ResourceCost { public string id; public float amount; }

    [System.Serializable]
    public struct TechEffect
    {
        public TechEffectKind kind;
        public float value;
        public string target; // UnlockSpecies 时填物种 id
    }

    [CreateAssetMenu(menuName = "StarIsle/TechDef")]
    public class TechDef : ScriptableObject
    {
        public string id;
        public string displayName;
        public string description;
        public List<ResourceCost> cost;
        public string[] prerequisites;
        public List<TechEffect> effects;
    }
}
