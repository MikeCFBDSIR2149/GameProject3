using System.Collections.Generic;
using UnityEngine;

namespace Tutorial
{
    // 场景中的教程控制器（不要使用单例）
    // 在 Inspector 中按顺序将实现了 ITutorialContext 的组件拖入 `tutorialSteps` 列表
    public class TutorialDirector : MonoBehaviour
    {
        [Tooltip("在 Inspector 中按顺序拖入实现了 ITutorialContext 的组件（使用 Component 类型，因为 Inspector 不支持接口列表）")]
        [SerializeField] private List<MonoBehaviour> tutorialSteps = new List<MonoBehaviour>();

        // 运行时转换后的接口列表
        private readonly List<ITutorialContext> _contexts = new List<ITutorialContext>();
        private int _currentIndex = -1;

        private void Awake()
        {
            // 手动转换并忽略不匹配的条目（避免依赖 LINQ）
            _contexts.Clear();
            foreach (var mb in tutorialSteps)
            {
                if (mb is ITutorialContext ctx)
                {
                    _contexts.Add(ctx);
                    // 将自身注入到每个 Context，使其可以在需要时回调或推进教程
                    ctx.SetDirector(this);
                }
            }
        }

        // 启动教程，从第 0 步开始
        public void StartTutorial()
        {
            if (_contexts.Count == 0) return;
            _currentIndex = 0;
            _contexts[_currentIndex].Enter();
        }

        // 推进到下一步：调用当前的 Exit，然后进入下一步的 Enter
        public void NextStep()
        {
            if (_currentIndex < 0 || _currentIndex >= _contexts.Count) return;

            _contexts[_currentIndex].Exit();

            _currentIndex++;
            if (_currentIndex < _contexts.Count)
            {
                _contexts[_currentIndex].Enter();
            }
            else
            {
                // 教程结束（可选行为）
                Debug.Log("Tutorial finished.");
            }
        }

        // 可用于重置或重新开始
        public void ResetTutorial()
        {
            if (_currentIndex >= 0 && _currentIndex < _contexts.Count)
            {
                _contexts[_currentIndex].Exit();
            }
            _currentIndex = -1;
        }
    }
}
