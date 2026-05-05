namespace Kuchinashi.SceneFlow
{
    public readonly struct OnSceneFlowEnterCoverStartedEvent
    {
        public string TargetSceneName { get; }

        public OnSceneFlowEnterCoverStartedEvent(string targetSceneName)
        {
            TargetSceneName = targetSceneName;
        }
    }

    public readonly struct OnSceneFlowEnterCoverCompletedEvent
    {
        public string TargetSceneName { get; }

        public OnSceneFlowEnterCoverCompletedEvent(string targetSceneName)
        {
            TargetSceneName = targetSceneName;
        }
    }

    public readonly struct OnSceneFlowContentSceneActivatedEvent
    {
        public string SceneName { get; }

        public OnSceneFlowContentSceneActivatedEvent(string sceneName)
        {
            SceneName = sceneName;
        }
    }

    public readonly struct OnSceneFlowExitCoverStartedEvent
    {
        public string ContentSceneName { get; }

        public OnSceneFlowExitCoverStartedEvent(string contentSceneName)
        {
            ContentSceneName = contentSceneName;
        }
    }

    public readonly struct OnSceneFlowExitCoverCompletedEvent
    {
        public string ContentSceneName { get; }

        public OnSceneFlowExitCoverCompletedEvent(string contentSceneName)
        {
            ContentSceneName = contentSceneName;
        }
    }

    public readonly struct OnSceneFlowTransitionIdleEvent { }
}
