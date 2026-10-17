// S3 — 图鉴：记录已发现的物种（留存驱动）
using System.Collections.Generic;
using UnityEngine;

namespace StarIsle.Bio
{
    public class Codex : MonoBehaviour
    {
        private readonly HashSet<string> _discovered = new();
        public int DiscoveredCount => _discovered.Count;
        public void Discover(string id) => _discovered.Add(id);
        public bool Has(string id) => _discovered.Contains(id);
    }
}
