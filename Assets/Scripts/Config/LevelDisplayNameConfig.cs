using System.Collections.Generic;
using UnityEngine;

namespace Config
{
    [CreateAssetMenu(fileName = "LevelDisplayNameConfig", menuName = "Config/Level Display Name Config")]
    public class LevelDisplayNameConfig : ScriptableObject
    {
        [System.Serializable]
        public struct LevelDisplayNameEntry
        {
            public int levelIndex;
            public string displayName;
        }

        [SerializeField] private List<LevelDisplayNameEntry> entries = new List<LevelDisplayNameEntry>();

        public string GetDisplayName(int levelIndex)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].levelIndex == levelIndex && !string.IsNullOrEmpty(entries[i].displayName))
                {
                    return entries[i].displayName;
                }
            }
            return levelIndex.ToString();
        }
    }
}


