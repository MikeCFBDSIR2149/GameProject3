using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class EnergyUI : UIBase
    {
        [SerializeField] private Slider energySlider;
        [SerializeField] private Text energyText; // 新增：能量数值显示
        private float maxEnergy = 100f;

        public void Init(float max, float current)
        {
            maxEnergy = max;
            if (energySlider != null)
            {
                energySlider.maxValue = maxEnergy;
                energySlider.value = Mathf.Clamp(current, 0, maxEnergy);
            }
            UpdateEnergyText(current); // 更新数值文本
        }

        public void SetEnergy(float value)
        {
            if (energySlider != null)
            {
                energySlider.value = Mathf.Clamp(value, 0, maxEnergy);
                
            }
            UpdateEnergyText(value); // 更新数值文本
        }

        public void SetMaxEnergy(float max)
        {
            maxEnergy = max;
            if (energySlider != null)
            {
                energySlider.maxValue = maxEnergy;
            }
        }
        public override void UpdateUI(object data)
        {
            // 假设 data 是 float 类型的能量值
            if (data is float value)
            {
                SetEnergy(value);
            }
            // 你也可以根据需要支持其它类型的数据
        }
        // 新增：更新能量数值文本
        private void UpdateEnergyText(float value)
        {
            if (energyText != null)
            {
                energyText.text = $"{Mathf.Clamp(value, 0, maxEnergy)} / {maxEnergy}";
            }
        }
        // 如有需要，可以重写 UIBase 的方法
        public void OnShow()
        {
            base.OnShow();
            // 额外逻辑
        }
        private void OnEnable()
        {
            UIEventManager.AddListener("OnEnergyChanged", UpdateUI);
        }
        private void OnDisable()
        {
            UIEventManager.RemoveListener("OnEnergyChanged", UpdateUI);
        }
    }
}
