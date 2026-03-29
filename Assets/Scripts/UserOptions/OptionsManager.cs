using System;
using System.IO;
using UnityEngine;

namespace UserOptions
{
    public enum EUserOptionKey
    {
        HorizontalSensitivity,
        VerticalSensitivity
        // 新增设置项时在此添加
    }

    public class OptionsManager : MonoSingleton<OptionsManager>
    {
        public string fileName = "options.json";
        private OptionsData _optionsData;

        public event Action OnOptionsChanged;

        private string GetFilePath()
        {
            return Path.Combine(Application.persistentDataPath, fileName);
        }

        protected override void Awake()
        {
            base.Awake();
            LoadOptions();
        }

        public void LoadOptions()
        {
            string path = GetFilePath();
            if (File.Exists(path))
            {
                string jsonString = File.ReadAllText(path);
                _optionsData = JsonUtility.FromJson<OptionsData>(jsonString);
            }
            else
            {
                ResetToDefault();
            }
            OnOptionsChanged?.Invoke();
        }

        public void SaveOptions()
        {
            string path = GetFilePath();
            string jsonString = JsonUtility.ToJson(_optionsData, true);
            File.WriteAllText(path, jsonString);
            OnOptionsChanged?.Invoke();
        }

        public void ResetToDefault()
        {
            _optionsData = new OptionsData();
            SaveOptions();
        }

        public OptionsData GetOptions()
        {
            return _optionsData;
        }

        public void SetOption(EUserOptionKey key, float value)
        {
            switch (key)
            {
                case EUserOptionKey.HorizontalSensitivity:
                    _optionsData.horizontalSensitivity = value;
                    break;
                case EUserOptionKey.VerticalSensitivity:
                    _optionsData.verticalSensitivity = value;
                    break;
                // 新增设置项时在此添加
                default:
                    Debug.LogWarning($"Unknown OptionKey: {key}");
                    return;
            }
            // SaveOptions();
        }

        public float GetOption(EUserOptionKey key)
        {
            return ReturnOptionFromEnum(key);
        }

        private float ReturnOptionFromEnum(EUserOptionKey key)
        {
            switch (key)
            {
                case EUserOptionKey.HorizontalSensitivity:
                    return _optionsData.horizontalSensitivity;
                case EUserOptionKey.VerticalSensitivity:
                    return _optionsData.verticalSensitivity;
                // 新增设置项时在此添加
                default:
                    Debug.LogWarning($"Unknown OptionKey: {key}");
                    return -1f;
            }
        }
    }
}
