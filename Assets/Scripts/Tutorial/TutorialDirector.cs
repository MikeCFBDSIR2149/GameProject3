using System.Collections.Generic;
using UnityEngine;

namespace Tutorial
{
    // 场景中的教程控制器（不要使用单例）
    // 各个 ITutorialContext 实现在自己的 Awake 时调用 RegisterContext 注册
    // 在 Inspector 中指定教程步骤的执行顺序（按 ContextID）
    public class TutorialDirector : MonoBehaviour
    {
        [Tooltip("教程步骤的执行顺序（按 ContextID 填写，如 \"Movement\", \"Attack\", \"Dodge\"）")]
        [SerializeField] private List<string> contextOrder = new List<string>();

        // 存储已注册的所有 Context（key 为 ContextID）
        private readonly Dictionary<string, ITutorialContext> _registeredContexts = new Dictionary<string, ITutorialContext>();
        
        private int _currentIndex = -1;

        // 允许 Context 在自己的 Awake 时注册到 Director
        public void RegisterContext(ITutorialContext context)
        {
            if (context == null)
            {
                Debug.LogError("[TutorialDirector] 尝试注册 null context");
                return;
            }

            _registeredContexts[context.ContextID] = context;
        }

        private void Start()
        {
            StartTutorial();
        }

        // 根据 contextOrder 中的索引获取对应的 Context（如果已注册则返回，否则返回 null）
        private ITutorialContext GetContextByIndex(int index)
        {
            if (index < 0 || index >= contextOrder.Count)
                return null;

            var contextID = contextOrder[index];
            if (_registeredContexts.TryGetValue(contextID, out var ctx))
            {
                return ctx;
            }

            Debug.LogWarning($"[TutorialDirector] 找不到已注册的 ContextID 为 '{contextID}' 的教程步骤");
            return null;
        }

        // 启动教程，从第 0 步开始
        private void StartTutorial()
        {
            if (contextOrder.Count == 0)
            {
                Debug.LogWarning("[TutorialDirector] No context order configured");
                return;
            }

            _currentIndex = 0;
            var context = GetContextByIndex(_currentIndex);
            if (context != null)
            {
                context.Enter();
                Debug.Log($"[TutorialDirector] Tutorial started.");
            }
        }

        // 推进到下一步：调用当前的 Exit，然后进入下一步的 Enter
        public void NextStep()
        {
            if (_currentIndex < 0) return;

            var currentContext = GetContextByIndex(_currentIndex);
            if (currentContext != null)
            {
                currentContext.Exit();
            }

            _currentIndex++;
            var nextContext = GetContextByIndex(_currentIndex);
            if (nextContext != null)
            {
                nextContext.Enter();
            }
            else if (_currentIndex >= contextOrder.Count)
            {
                // 教程结束（可选行为）
                Debug.Log("Tutorial finished.");
            }
        }

        // 可用于重置或重新开始
        public void ResetTutorial()
        {
            var currentContext = GetContextByIndex(_currentIndex);
            if (currentContext != null)
            {
                currentContext.Exit();
            }
            _currentIndex = -1;
        }
    }
}
