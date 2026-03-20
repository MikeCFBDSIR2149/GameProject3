using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CountdownUI : UIBase
    {
        [Header("倒计时UI组件")]
        [SerializeField] private Text countdownText;
        [SerializeField] private Text messageText;
        [SerializeField] private GameObject countdownPanel;
    
        private Coroutine countdownCoroutine;
        private int currentTime;
    
        public override void UpdateUI(object data)
        {
            if (data is CountdownData countdownData)
            {
                StartCountdown(countdownData);
            }
        }
    
        // 开始倒计时
        public void StartCountdown(CountdownData data)
        {
            // 停止之前的倒计时
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
            }
        
            currentTime = data.startTime;
            messageText.text = data.message;
            countdownText.text = currentTime.ToString();
            countdownPanel.SetActive(true);
        
            countdownCoroutine = StartCoroutine(CountdownRoutine(data));
        }
    
        private IEnumerator CountdownRoutine(CountdownData data)
        {
            while (currentTime > 0)
            {
                yield return new WaitForSeconds(1f);
                currentTime--;
                countdownText.text = currentTime.ToString();
            
                // 触发每秒事件
                OnCountdownTick(currentTime);
            
                // 最后3秒特殊效果
                if (currentTime <= 3)
                {
                    StartCoroutine(FlashText());
                }
            }
        
            // 倒计时结束
            OnCountdownComplete(data);
        }
    
        private IEnumerator FlashText()
        {
            Color originalColor = countdownText.color;
            countdownText.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            countdownText.color = originalColor;
        }
    
        private void OnCountdownTick(int remainingTime)
        {
            // 可以在这里触发事件
            UIEventManager.TriggerEvent("CountdownTick", remainingTime);
        }
    
        private void OnCountdownComplete(CountdownData data)
        {
            countdownText.text = "0";
            messageText.text = data.completeMessage;
        
            // 触发完成事件
            UIEventManager.TriggerEvent("CountdownComplete", data);
        
            // 延迟隐藏
            if (data.autoHide)
            {
                StartCoroutine(HideAfterDelay(data.hideDelay));
            }
        }
    
        private IEnumerator HideAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            //UIManager.Instance.HideCurrentUI();
        }
    
        // 外部控制方法
        public void PauseCountdown()
        {
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null;
            }
        }
    
        public void ResumeCountdown()
        {
            if (countdownCoroutine == null && currentTime > 0)
            {
                var data = new CountdownData
                {
                    startTime = currentTime,
                    message = messageText.text
                };
                countdownCoroutine = StartCoroutine(CountdownRoutine(data));
            }
        }
    
        public void StopCountdown()
        {
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null;
            }
            //UIManager.Instance.HideCurrentUI();
        }
    }

// 倒计时数据类
    [System.Serializable]
    public class CountdownData
    {
        public int startTime = 10;          // 开始时间
        public string message = "准备开始"; // 显示消息
        public string completeMessage = "开始!"; // 完成消息
        public bool autoHide = true;        // 完成后自动隐藏
        public float hideDelay = 1f;        // 隐藏延迟
    }
}