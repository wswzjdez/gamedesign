// S2 — 跨端存档：用 PlayerPrefs 做可用占位（Web/PC 通用；S3 迁 IndexedDB/文件）
using UnityEngine;
using System;

namespace StarIsle.Persistence
{
    [Serializable]
    public struct SaveData
    {
        public float atmosphere, hydro, biodiversity;
        public int purifiers, recyclers;
        public float light, water, mineral, biomass;
        public int creatures;
    }

    public class SaveSystem : MonoBehaviour
    {
        private const string KEY = "starisle_save_v1";

        public void SaveGame(SaveData d) => PlayerPrefs.SetString(KEY, JsonUtility.ToJson(d));
        public bool HasSave() => PlayerPrefs.HasKey(KEY);
        public SaveData LoadGame() => JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(KEY));
        public void Clear() => PlayerPrefs.DeleteKey(KEY);
    }
}
