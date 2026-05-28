namespace Tutorial
{
    // 教程步骤接口（极简）
    // 任何希望作为教程步骤的组件都应该实现此接口
    public interface ITutorialContext
    {
        // 唯一标识，用于在运行时或调试时识别步骤
        string ContextID { get; }

        // 当该教程步骤被激活时调用
        void Enter();

        // 当该教程步骤被取消或推进到下一步时调用
        void Exit();

        // 将 TutorialDirector 的引用传入步骤，使步骤能够回调或推进流程
        // Director 会在 Awake/初始化时将自身注入到所有已注册的 Context
        void SetDirector(TutorialDirector director);
    }
}
