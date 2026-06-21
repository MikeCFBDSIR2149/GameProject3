using System;
using UnityEngine;
using Tools;

namespace UserOptions
{
    public enum EUserOptionKey
    {
        HorizontalSensitivity,
        VerticalSensitivity,
        MainVolume,
        SoundEffectsVolume
        // 新增设置项时在此添加
    }

    public class OptionsManager : MonoSingleton<OptionsManager>
    {
        public string fileName = "options.json";
        private OptionsData _optionsData;

        public event Action OnOptionsChanged;

        protected override void Awake()
        {
            base.Awake();
            LoadOptions();
        }

        public void LoadOptions()
        {
            if (!PersistentJsonStorage.TryLoad(fileName, out _optionsData) || _optionsData == null)
            {
                _optionsData = new OptionsData();
                PersistentJsonStorage.Save(fileName, _optionsData);
            }

            OnOptionsChanged?.Invoke();
        }

        public void SaveOptions()
        {
            if (_optionsData == null)
            {
                _optionsData = new OptionsData();
            }

            PersistentJsonStorage.Save(fileName, _optionsData);
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
                case EUserOptionKey.MainVolume:
                    _optionsData.mainVolume = value;
                    break;
                case EUserOptionKey.SoundEffectsVolume:
                    _optionsData.soundEffectsVolume = value;
                    break;
                // 新增设置项时在此添加
                default:
                    Debug.LogWarning($"Unknown OptionKey: {key}");
                    return;
            }
            SaveOptions();
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
                case EUserOptionKey.MainVolume:
                    return _optionsData.mainVolume;
                case EUserOptionKey.SoundEffectsVolume:
                    return _optionsData.soundEffectsVolume;
                // 新增设置项时在此添加
                default:
                    Debug.LogWarning($"Unknown OptionKey: {key}");
                    return -1f;
            }
        }
    }
}
