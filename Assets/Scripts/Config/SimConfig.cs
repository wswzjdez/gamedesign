// S3 — 数据驱动配置（运行时可用默认实例，也可做成 SO 资产）
using UnityEngine;

namespace StarIsle.Config
{
    [CreateAssetMenu(menuName = "StarIsle/SimConfig")]
    public class SimConfig : ScriptableObject
    {
        // 模拟节奏
        public float tickInterval = 1f;

        // 初始生态指标（0..1）
        public float startAtmosphere = 0.15f;
        public float startHydro = 0.20f;
        public float startBiodiversity = 0f;

        // 资源自然产出
        public float lightRate = 2f;            // 光/秒
        public float waterPassiveRate = 0.6f;   // 水/秒（基础回补）

        // 建筑对生态目标的提升
        public float atmospherePerPurifier = 0.08f;
        public float hydroPerRecycler = 0.10f;

        // 玩家动作成本
        public float mineralPerGather = 10f;
        public float purifierCost = 20f;
        public float recyclerCost = 15f;
        public float hatchBiomassCost = 30f;
        public float hatchAtmoReq = 0.4f;
        public float hatchHydroReq = 0.4f;

        // 生物行为
        public float creatureWaterCost = 0.4f;     // 每只/ tick 耗水
        public float creatureBiomassYield = 1.2f;  // 每只/ tick 产生物质
        public float creatureEnergyGain = 0.05f;
        public float creatureStarveRate = 0.08f;
        public int maxPopulation = 12;             // 种群上限（AI 繁殖封顶）
    }
}
