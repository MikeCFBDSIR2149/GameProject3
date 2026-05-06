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
        private Coroutine flashCoroutine;

        private int currentTime;
        private float currentTickInterval = 1f;

        // 缓存最后一次启动的数据，避免“暂停恢复/重启”时数据丢字段
        private CountdownData _lastData;

        public override void UpdateUI(object data)
        {
            if (data is CountdownData countdownData)
            {
                _lastData = countdownData;

                if (countdownPanel != null) countdownPanel.SetActive(true);
                if (messageText != null) messageText.text = countdownData.message;

                SetTime(countdownData.startTime);
                // 不要 StartCountdown，不要启动任何协程
            }
        }

        /// <summary>
        /// 只用于外部“一次性启动倒计时”，不要在运行中反复调用。
        /// </summary>
        public void StartCountdown(CountdownData data)
        {
            Debug.Log($"[CountdownUI] StartCountdown frame={Time.frameCount}\n{System.Environment.StackTrace}");
            if (data == null) return;

            _lastData = data;

            // 停止之前的倒计时
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null;
            }

            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
                flashCoroutine = null;
            }

            currentTime = Mathf.Max(0, data.startTime);
            currentTickInterval = Mathf.Max(0.01f, data.tickInterval);

            if (messageText != null) messageText.text = data.message;
            if (countdownText != null) countdownText.text = currentTime.ToString();
            if (countdownPanel != null) countdownPanel.SetActive(true);

            countdownCoroutine = StartCoroutine(CountdownRoutine());
        }

        /// <summary>
        /// 使用真实时间等待：不受 Time.timeScale=5 的影响。
        /// 暂停时（GameplayManager.Status==Paused）停止流逝。
        /// </summary>
        private IEnumerator CountdownRoutine()
        {
            while (currentTime > 0)
            {
                // 先等待一个 tick（期间如果暂停则等待不前进）
                float t = 0f;
                while (t < currentTickInterval)
                {
                    // 暂停：不累计 t，直接挂起
                    if (IsPaused())
                    {
                        yield return null;
                        continue;
                    }

                    t += Time.unscaledDeltaTime; // 用 unscaled 累计真实时间
                    yield return null;
                }

                // tick 到了，扣 1 秒
                currentTime--;
                if (countdownText != null) countdownText.text = currentTime.ToString();

                // 触发每秒事件
                UIEventManager.TriggerEvent("CountdownTick", currentTime);

                // 最后3秒闪烁：只允许同时存在一个闪烁协程，避免叠加
                if (currentTime <= 3 && currentTime > 0)
                {
                    if (flashCoroutine == null)
                        flashCoroutine = StartCoroutine(FlashTextOnce());
                }
            }

            // 倒计时结束
            OnCountdownComplete();
        }

        private bool IsPaused()
        {
            // 只要 GameplayManager 存在且处于 Paused，就认为暂停
            // 这样不依赖 timeScale（因为你 bulletTime/默认状态会改变 timeScale）
            return GameplayManager.Instance != null && GameplayManager.Instance.Status == EGameplayStatus.Paused;
        }

        private IEnumerator FlashTextOnce()
        {
            if (countdownText == null)
            {
                flashCoroutine = null;
                yield break;
            }

            Color originalColor = countdownText.color;
            countdownText.color = Color.red;

            // 用真实时间闪烁，不受 timeScale 影响；暂停时也冻结闪烁
            float t = 0f;
            while (t < 0.2f)
            {
                if (IsPaused())
                {
                    yield return null;
                    continue;
                }

                t += Time.unscaledDeltaTime;
                yield return null;
            }

            countdownText.color = originalColor;
            flashCoroutine = null;
        }

        private void OnCountdownComplete()
        {
            if (countdownText != null) countdownText.text = "0";
            if (messageText != null && _lastData != null) messageText.text = _lastData.completeMessage;

            // 触发完成事件
            UIEventManager.TriggerEvent("CountdownComplete", _lastData);

            // 延迟隐藏（真实时间），暂停时也冻结延迟
            if (_lastData != null && _lastData.autoHide)
            {
                StartCoroutine(HideAfterDelayRealtime(_lastData.hideDelay));
            }
        }

        private IEnumerator HideAfterDelayRealtime(float delay)
        {
            delay = Mathf.Max(0f, delay);

            float t = 0f;
            while (t < delay)
            {
                if (IsPaused())
                {
                    yield return null;
                    continue;
                }

                t += Time.unscaledDeltaTime;
                yield return null;
            }

            // 你如果希望隐藏整个 panel：
            // if (countdownPanel != null) countdownPanel.SetActive(false);
        }

        // 这些接口如果外部还在调用，可以保留；方法内部做“安全停止”
        public void PauseCountdown()
        {
            // 方法A/自驱倒计时模式下不需要外部暂停，这里留空或仅停协程都行
            // 如果你希望外部强制停，可以取消注释：
            /*
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null;
            }
            */
        }

        public void ResumeCountdown()
        {
            // 同上：通常不需要外部恢复
        }

        public void StopCountdown()
        {
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null;
            }

            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
                flashCoroutine = null;
            }
        }

        // 可选：你之前加的 SetTime 还想保留的话也可以
        public void SetTime(int seconds, string message = null)
        {
            seconds = Mathf.Max(0, seconds);

            if (countdownPanel != null && !countdownPanel.activeSelf)
                countdownPanel.SetActive(true);

            if (!string.IsNullOrEmpty(message) && messageText != null)
                messageText.text = message;

            if (countdownText != null)
                countdownText.text = seconds.ToString();
        }
    }

    [System.Serializable]
    public class CountdownData
    {
        public int startTime = 10;
        public string message = "准备开始";
        public string completeMessage = "开始!";
        public bool autoHide = true;
        public float hideDelay = 1f;
        public float tickInterval = 1f;
    }
}