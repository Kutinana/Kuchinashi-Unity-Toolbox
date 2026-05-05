using UnityEngine;

namespace Kuchinashi.SceneFlow
{
    /// <summary>
    /// Single entry point: ensures bus, controller, and a transition view exist, then configures the flow for the shell scene this object lives in.
        /// Assign <see cref="m_TransitionViewBehaviour"/> to a project-specific <see cref="SceneTransitionViewBehaviour"/> (implements <see cref="ISceneTransitionView"/>); if unset, resolves one on this object or children, otherwise adds <see cref="NoOpSceneTransitionView"/>.
    /// </summary>
    /// <remarks>
    /// Default bus is <see cref="InMemorySceneMessageBus"/>. To use QFramework TypeEventSystem as the bus, add your asmdef reference to assembly
    /// <c>Kuchinashi.SceneFlow.QFramework</c>, use the <c>QFrameworkSceneMessageBus</c> component from the Integrations folder instead of
    /// <see cref="InMemorySceneMessageBus"/>, and keep an <see cref="ISceneMessageBus"/> on this GameObject so Awake wiring still works.
    /// </remarks>
    [DefaultExecutionOrder(-100)]
    public sealed class SceneFlowHost : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private string m_InitialContentScene;
        [SerializeField] private bool m_LoadInitialOnStart = true;
        [SerializeField] private bool m_WaitContentReadyOnInitialLoad = true;
        [SerializeField] private SceneTransitionViewBehaviour m_TransitionViewBehaviour;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            var bus = GetComponent<ISceneMessageBus>();
            if (bus == null)
            {
                bus = gameObject.AddComponent<InMemorySceneMessageBus>();
            }

            var controller = GetComponent<SceneFlowController>();
            if (controller == null)
            {
                controller = gameObject.AddComponent<SceneFlowController>();
            }

            var view = ResolveTransitionView();
            var shellScene = gameObject.scene;
            controller.Configure(view, bus, shellScene);
        }

        private void Start()
        {
            if (!m_LoadInitialOnStart || string.IsNullOrEmpty(m_InitialContentScene))
            {
                return;
            }

            var controller = GetComponent<SceneFlowController>();
            if (controller != null)
            {
                controller.RequestSwitchContent(m_InitialContentScene, m_WaitContentReadyOnInitialLoad);
            }
        }

        #endregion

        #region Private Methods

        private ISceneTransitionView ResolveTransitionView()
        {
            if (m_TransitionViewBehaviour != null)
            {
                return m_TransitionViewBehaviour;
            }

            var behaviours = GetComponents<MonoBehaviour>();
            for (var i = 0; i < behaviours.Length; i++)
            {
                var component = behaviours[i];
                if (component == null || component == this)
                {
                    continue;
                }

                if (component is ISceneTransitionView found)
                {
                    return found;
                }
            }

            var children = GetComponentsInChildren<MonoBehaviour>(true);
            for (var i = 0; i < children.Length; i++)
            {
                var component = children[i];
                if (component == null || component == this)
                {
                    continue;
                }

                if (component is ISceneTransitionView found)
                {
                    return found;
                }
            }

            return gameObject.AddComponent<NoOpSceneTransitionView>();
        }

        #endregion
    }
}
