using UnityEngine;

namespace UserOptions
{
    [System.Serializable]
    public class OptionsData
    {
        public float horizontalSensitivity = 12.0f;
        public float verticalSensitivity = 15.0f;
        public float mainVolume = 60.0f;
        public float soundEffectsVolume = 60.0f;
        // 新增设置项时在此添加
    }
}
