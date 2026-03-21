// HealthData.cs - 放在Assets/Scripts/UI/文件夹下
using System;

namespace UI
{
    [Serializable]
    public class HealthData
    {
        public int currentHealth = 10;
        public int maxHealth = 10;
        public bool showNumbers = true; // 是否显示数字
        
        // 构造函数
        public HealthData() { }
        
        public HealthData(int current, int max)
        {
            currentHealth = current;
            maxHealth = max;
        }
        //
        // public HealthData(int current, int max, bool showNumbers)
        // {
        //     currentHealth = current;
        //     maxHealth = max;
        //     this.showNumbers = showNumbers;
        // }
        //
        // // 计算血量百分比
        // public float GetHealthPercentage()
        // {
        //     if (maxHealth <= 0) return 0f;
        //     return (float)currentHealth / maxHealth;
        // }
        //
        // // 检查是否满血
        // public bool IsFullHealth()
        // {
        //     return currentHealth >= maxHealth;
        // }
        //
        // // 检查是否死亡
        // public bool IsDead()
        // {
        //     return currentHealth <= 0;
        // }
        //
        // // 增加血量
        // public void AddHealth(int amount)
        // {
        //     currentHealth += amount;
        //     if (currentHealth > maxHealth)
        //         currentHealth = maxHealth;
        // }
        //
        // // 减少血量
        // public void RemoveHealth(int amount)
        // {
        //     currentHealth -= amount;
        //     if (currentHealth < 0)
        //         currentHealth = 0;
        // }
        //
        // // 设置血量
        // public void SetHealth(int health)
        // {
        //     currentHealth = health;
        //     if (currentHealth > maxHealth)
        //         currentHealth = maxHealth;
        //     if (currentHealth < 0)
        //         currentHealth = 0;
        // }
        //
        // // 设置最大血量
        // public void SetMaxHealth(int max)
        // {
        //     maxHealth = max;
        //     if (currentHealth > maxHealth)
        //         currentHealth = maxHealth;
        // }
        //
        // // 重置血量
        // public void ResetHealth()
        // {
        //     currentHealth = maxHealth;
        // }
    }
}