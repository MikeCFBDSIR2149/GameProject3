using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class HealthUI : UIBase
    {
        [Header("血量显示设置")]
        [SerializeField] private Transform heartContainer; // 存放心形图标的容器
        [SerializeField] private GameObject heartPrefab;   // 单个心形预制体
        [SerializeField] private Sprite fullHeartSprite;   // 满血心形
        [SerializeField] private Sprite emptyHeartSprite;  // 空血心形
        public Slider healthBar;
        [Header("数字显示设置")]
        [SerializeField] private Text healthText;          // 血量数字显示
        [SerializeField] private bool showNumbers = true;  // 是否显示数字
        
        [Header("布局设置")]
        [SerializeField] private float heartSpacing = 10f; // 心形间距
        [SerializeField] private Vector2 heartSize = new Vector2(50, 50); // 心形大小
        
        [Header("动画设置")]
        [SerializeField] private float heartScaleDuration = 0.2f;
        [SerializeField] private float heartDropDelay = 0.1f;
        
        private List<Image> heartImages = new List<Image>();
        private HealthData healthData;
        
        public override void OnInit()
        {
            base.OnInit();
            
            // 确保容器有布局组件
            EnsureLayoutComponents();
            
            // 初始化数字显示
            if (healthText == null && showNumbers)
            {
                CreateHealthText();
            }
        }
        
        public override void UpdateUI(object data)
        {
            if (data is HealthData healthData)
            {
                this.healthData = healthData;
                this.showNumbers = healthData.showNumbers;
                InitializeHealth();
            }
            else if (data is int healthValue)
            {
                // 如果传入的是int，创建默认HealthData
                this.healthData = new HealthData(healthValue, 10);
                InitializeHealth();
            }
        }
        
        
        public void SetHealth(float value)
        {
            // 刷新血量UI
            healthBar.value = value;
            healthText.text = value.ToString();
        }
        // 初始化血量显示
        private void InitializeHealth()
        {
            if (healthData == null)
            {
                Debug.LogWarning("HealthData为空，使用默认值");
                healthData = new HealthData(10, 10);
            }
            
            // 清除现有的心形
            ClearExistingHearts();
            
            // 创建心形
            CreateHearts();
            
            // 更新显示
            UpdateHeartDisplay();
            
            // 更新数字显示
            UpdateHealthText();
        }
        
        // 确保容器有布局组件
        private void EnsureLayoutComponents()
        {
            if (heartContainer == null)
            {
                // 创建容器
                GameObject containerObj = new GameObject("HeartContainer");
                containerObj.transform.SetParent(transform);
                heartContainer = containerObj.transform;
            }
            
            // 添加或获取HorizontalLayoutGroup
            HorizontalLayoutGroup layoutGroup = heartContainer.GetComponent<HorizontalLayoutGroup>();
            if (layoutGroup == null)
            {
                layoutGroup = heartContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            }
            
            // 配置布局
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.spacing = heartSpacing;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
            
            // 添加ContentSizeFitter（可选）
            ContentSizeFitter fitter = heartContainer.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = heartContainer.gameObject.AddComponent<ContentSizeFitter>();
            }
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // 设置容器位置
            RectTransform containerRect = heartContainer.GetComponent<RectTransform>();
            if (containerRect != null)
            {
                containerRect.anchorMin = new Vector2(0.5f, 0.5f);
                containerRect.anchorMax = new Vector2(0.5f, 0.5f);
                containerRect.pivot = new Vector2(0.5f, 0.5f);
                containerRect.anchoredPosition = Vector2.zero;
            }
        }
        
        // 创建血量数字显示
        private void CreateHealthText()
        {
            GameObject textObj = new GameObject("HealthText");
            textObj.transform.SetParent(transform);
            
            healthText = textObj.AddComponent<Text>();
            healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            healthText.fontSize = 20;
            healthText.color = Color.white;
            healthText.alignment = TextAnchor.MiddleCenter;
            
            RectTransform rect = textObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0, -40);
            rect.sizeDelta = new Vector2(200, 30);
        }
        
        // 清除现有的心形
        private void ClearExistingHearts()
        {
            foreach (Transform child in heartContainer)
            {
                Destroy(child.gameObject);
            }
            heartImages.Clear();
        }
        
        // 创建心形
        private void CreateHearts()
        {
            for (int i = 0; i < healthData.maxHealth; i++)
            {
                GameObject heartObj = Instantiate(heartPrefab, heartContainer);
                
                // 设置心形大小
                RectTransform heartRect = heartObj.GetComponent<RectTransform>();
                if (heartRect != null)
                {
                    heartRect.sizeDelta = heartSize;
                }
                
                // 获取Image组件
                Image heartImage = heartObj.GetComponent<Image>();
                if (heartImage == null)
                {
                    heartImage = heartObj.AddComponent<Image>();
                }
                
                heartImages.Add(heartImage);
                
                // 添加点击测试（可选）
                Button heartButton = heartObj.GetComponent<Button>();
                if (heartButton != null)
                {
                    int index = i;
                    heartButton.onClick.AddListener(() => OnHeartClicked(index));
                }
            }
        }
        
        // 更新心形显示
        private void UpdateHeartDisplay()
        {
            for (int i = 0; i < heartImages.Count; i++)
            {
                if (i < healthData.currentHealth)
                {
                    // 满血
                    heartImages[i].sprite = fullHeartSprite;
                    heartImages[i].color = Color.red;
                }
                else
                {
                    // 空血
                    heartImages[i].sprite = emptyHeartSprite;
                    heartImages[i].color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                }
                
                // 如果sprite为空，使用颜色区分
                if (heartImages[i].sprite == null)
                {
                    heartImages[i].color = i < healthData.currentHealth ? Color.red : Color.gray;
                }
            }
        }
        
        // 更新血量数字显示
        private void UpdateHealthText()
        {
            if (healthText != null && showNumbers)
            {
                healthText.gameObject.SetActive(true);
                healthText.text = $"{healthData.currentHealth}/{healthData.maxHealth}";
            }
            else if (healthText != null)
            {
                healthText.gameObject.SetActive(false);
            }
        }
        
        // 减少血量（一个一个掉）
        public void DecreaseHealth(int amount = 1)
        {
            if (amount <= 0 || healthData.currentHealth <= 0) return;
            
            StartCoroutine(DecreaseHealthRoutine(amount));
        }
        
        private IEnumerator DecreaseHealthRoutine(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                if (healthData.currentHealth <= 0) break;
                
                healthData.currentHealth--;
                int heartIndex = healthData.currentHealth; // 要掉落的心形索引
                
                // 播放掉落动画
                yield return StartCoroutine(PlayHeartDropAnimation(heartIndex));
                
                // 更新心形显示
                UpdateHeartDisplay();
                UpdateHealthText();
                
                // 触发血量变化事件
                UIEventManager.TriggerEvent("HealthChanged", healthData.currentHealth);
                
                // 延迟下一个心形掉落
                yield return new WaitForSeconds(heartDropDelay);
            }
            
            // 检查是否死亡
            if (healthData.currentHealth <= 0)
            {
                OnHealthDepleted();
            }
        }
        
        // 增加血量
        public void IncreaseHealth(int amount = 1)
        {
            if (amount <= 0 || healthData.currentHealth >= healthData.maxHealth) return;
            
            for (int i = 0; i < amount; i++)
            {
                if (healthData.currentHealth >= healthData.maxHealth) break;
                
                int heartIndex = healthData.currentHealth;
                healthData.currentHealth++;
                
                // 播放恢复动画
                StartCoroutine(PlayHeartRestoreAnimation(heartIndex));
                
                // 更新心形显示
                UpdateHeartDisplay();
                UpdateHealthText();
                
                // 触发血量变化事件
                UIEventManager.TriggerEvent("HealthChanged", healthData.currentHealth);
            }
        }
        
        // 设置血量（直接设置）
        // public void SetHealth(int health)
        // {
        //     if (healthData == null) return;
        //     
        //     healthData.SetHealth(health);
        //     
        //     // 更新显示
        //     UpdateHeartDisplay();
        //     UpdateHealthText();
        //     
        //     // 触发事件
        //     UIEventManager.TriggerEvent("HealthChanged", healthData.currentHealth);
        //     
        //     if (healthData.currentHealth <= 0)
        //     {
        //         OnHealthDepleted();
        //     }
        // }
        //
        // 设置最大血量
        // public void SetMaxHealth(int maxHealth)
        // {
        //     if (healthData == null) return;
        //     
        //     healthData.SetMaxHealth(maxHealth);
        //     
        //     // 重新初始化
        //     InitializeHealth();
        // }
        //
        // 获取当前血量
        public int GetCurrentHealth()
        {
            return healthData?.currentHealth ?? 0;
        }
        
        // 获取最大血量
        public int GetMaxHealth()
        {
            return healthData?.maxHealth ?? 0;
        }
        
        // 心形掉落动画
        private IEnumerator PlayHeartDropAnimation(int index)
        {
            if (index < 0 || index >= heartImages.Count) yield break;
            
            Image heartImage = heartImages[index];
            RectTransform rectTransform = heartImage.rectTransform;
            Vector3 originalScale = rectTransform.localScale;
            
            // 放大
            float timer = 0f;
            while (timer < heartScaleDuration / 2)
            {
                timer += Time.deltaTime;
                float progress = timer / (heartScaleDuration / 2);
                rectTransform.localScale = Vector3.Lerp(originalScale, originalScale * 1.3f, progress);
                yield return null;
            }
            
            // 缩小
            timer = 0f;
            while (timer < heartScaleDuration / 2)
            {
                timer += Time.deltaTime;
                float progress = timer / (heartScaleDuration / 2);
                rectTransform.localScale = Vector3.Lerp(originalScale * 1.3f, originalScale * 0.5f, progress);
                yield return null;
            }
            
            rectTransform.localScale = originalScale;
        }
        
        // 心形恢复动画
        private IEnumerator PlayHeartRestoreAnimation(int index)
        {
            if (index < 0 || index >= heartImages.Count) yield break;
            
            Image heartImage = heartImages[index];
            RectTransform rectTransform = heartImage.rectTransform;
            Vector3 originalScale = rectTransform.localScale;
            
            // 心跳效果
            float timer = 0f;
            while (timer < heartScaleDuration)
            {
                timer += Time.deltaTime;
                float progress = timer / heartScaleDuration;
                float scale = 1 + Mathf.Sin(progress * Mathf.PI) * 0.3f;
                rectTransform.localScale = originalScale * scale;
                yield return null;
            }
            
            rectTransform.localScale = originalScale;
        }
        
        // 血量耗尽
        private void OnHealthDepleted()
        {
            // 触发死亡事件
            UIEventManager.TriggerEvent("HealthDepleted", null);
            
            // 可以播放额外效果
            StartCoroutine(PlayDepletionEffect());
        }
        
        private IEnumerator PlayDepletionEffect()
        {
            // 所有心形闪烁
            for (int i = 0; i < 3; i++)
            {
                foreach (var heart in heartImages)
                {
                    heart.color = Color.red;
                }
                yield return new WaitForSeconds(0.2f);
                
                foreach (var heart in heartImages)
                {
                    heart.color = Color.white;
                }
                yield return new WaitForSeconds(0.2f);
            }
        }
        
        // 心形点击事件（可选功能）
        private void OnHeartClicked(int index)
        {
            // 点击心形可以查看详细信息或执行其他操作
            Debug.Log($"点击了第 {index + 1} 个心形，当前血量: {healthData.currentHealth}");
        }
    }
}