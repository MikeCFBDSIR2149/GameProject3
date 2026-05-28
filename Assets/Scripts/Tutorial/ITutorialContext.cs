namespace Tutorial
{
    // 教程步骤接口（极简）
    // 任何希望作为教程步骤的组件都应该实现此接口
    // 
    // 使用流程：
    // 1. 在该组件的 Inspector 中将 TutorialDirector 拖入 Director 字段
    // 2. 在 Awake 中调用 Director.RegisterContext(this) 进行注册
    //    例如：Director?.RegisterContext(this);
    // 3. TutorialDirector 会根据 contextOrder 配置中的顺序组织这些步骤
    // 4. 调用 TutorialDirector.StartTutorial() 开始教程
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
    }
}
