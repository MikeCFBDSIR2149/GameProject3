using System.Collections;

namespace Tutorial
{
    public interface ITutorialContext
    {
        // 在 Inspector 中拖入的 TutorialDirector 引用
        TutorialDirector Director { get; }

        // 唯一标识，用于在运行时或调试时识别步骤（需与 contextOrder 中的字符串对应）
        string ContextID { get; }

        // 当该教程步骤被激活时调用
        void Enter();

        // 当该教程步骤被取消或推进到下一步时调用
        void Exit();

        IEnumerator NextStepBuffer();
    }
}
