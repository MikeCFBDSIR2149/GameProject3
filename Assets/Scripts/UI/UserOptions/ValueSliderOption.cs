using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UserOptions;

namespace UI.UserOptions
{
    public class ValueSliderOption : MonoBehaviour
    {
        public EUserOptionKey key;
        public Slider slider;
        public TextMeshProUGUI valueText;
        public int valueAccuracyLevel;

        private void Awake()
        {
            if (valueAccuracyLevel < 0) valueAccuracyLevel = 0;
            if (!slider || !valueText) 
                return;
            float value = OptionsManager.Instance.GetOption(key);
            slider.value = value;
            valueText.text = value.ToString(valueAccuracyLevel == 0 ? "0" : "0." + new string('0', valueAccuracyLevel));
            slider.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnValueChanged(float newValue)
        {
            if (!slider || !valueText) 
                return;
            OptionsManager.Instance.SetOption(key, newValue);
            valueText.text = newValue.ToString(valueAccuracyLevel == 0 ? "0" : "0." + new string('0', valueAccuracyLevel));
        }
    }
}
