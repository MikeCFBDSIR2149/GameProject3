using System;

namespace GameProgress
{
    [Serializable]
    public class GameProgressData
    {
        public bool isNewPlayer = true;
        public System.Collections.Generic.List<int> clearedLevelIndices = new();
    }
}

