// S3 — 生物物种定义（可做成 SO 资产；S3 运行时也自动生成默认物种）
using UnityEngine;

namespace StarIsle.Bio
{
    [CreateAssetMenu(menuName = "StarIsle/CreatureSpecies")]
    public class CreatureSpecies : ScriptableObject
    {
        public string id;
        public string displayName;
        public Color color = Color.green;
        public float scale = 0.5f;
    }
}
