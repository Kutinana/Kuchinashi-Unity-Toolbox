using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kuchinashi.SceneFlow
{
    /// <summary>
    /// Orchestrates additive content loads while the shell scene stays loaded. Awaits <see cref="ISceneTransitionView"/> between phases.
    /// </summary>
    public sealed class SceneFlowController : MonoBehaviour
    {
        #region Private Fields

        private ISceneTransitionView m_View;
        private ISceneMessageBus m_Bus;
        private Scene m_ShellScene;
        private string m_CurrentContentSceneName = string.Empty;
        private bool m_IsTransitioning;
        private bool m_ContentReady;
        private Coroutine m_Running;
        private bool m_Configured;

        #endregion

        #region Public Properties

        public static SceneFlowController Instance { get; private set; }

        public bool IsTransitioning => m_IsTransitioning;

        public string CurrentContentSceneName => m_CurrentContentSceneName;

        public bool IsConfigured => m_Configured;

        #endregion

        #region Unity Lifecycle

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Injects dependencies; call from host before any transition. Shell scene is the scene that owns this controller (never unloaded by this type).
        /// </summary>
        public void Configure(ISceneTransitionView view, ISceneMessageBus bus, Scene shellScene)
        {
            if (view == null || bus == null || !shellScene.IsValid())
            {
                m_Configured = false;
                Debug.LogError("[SceneFlow] Configure requires non-null view and bus and a valid shell scene.");
                return;
            }

            m_View = view;
            m_Bus = bus;
            m_ShellScene = shellScene;
            m_Configured = true;
            Instance = this;
        }

        /// <summary>
        /// Additive-load target as new active content; unloads previous content if any (never unloads shell).
        /// </summary>
        public bool TryRequestSwitchContent(string sceneName, bool waitForContentReady = true)
        {
            if (!EnsureConfigured())
            {
                return false;
            }

            if (m_IsTransitioning)
            {
                return false;
            }

            if (string.IsNullOrEmpty(sceneName))
            {
                return false;
            }

            if (sceneName == m_ShellScene.name)
            {
                Debug.LogWarning("[SceneFlow] Target must be a content scene, not the shell scene.");
                return false;
            }

            if (sceneName == m_CurrentContentSceneName)
            {
                return false;
            }

            m_Running = StartCoroutine(SwitchContentRoutine(sceneName, waitForContentReady));
            return true;
        }

        public void RequestSwitchContent(string sceneName, bool waitForContentReady = true)
        {
            TryRequestSwitchContent(sceneName, waitForContentReady);
        }

        /// <summary>
        /// Lets the current content scene signal it is ready to reveal (after <see cref="RequestSwitchContent"/> with wait flag).
        /// </summary>
        public void NotifyContentReady()
        {
            m_ContentReady = true;
        }

        /// <summary>
        /// Unloads current content and returns active scene to shell. Optional same readiness gate as switch.
        /// </summary>
        public bool TryRequestUnloadCurrentContent(bool waitForContentReady = false)
        {
            if (!EnsureConfigured())
            {
                return false;
            }

            if (m_IsTransitioning)
            {
                return false;
            }

            if (string.IsNullOrEmpty(m_CurrentContentSceneName))
            {
                return false;
            }

            m_Running = StartCoroutine(UnloadContentRoutine(waitForContentReady));
            return true;
        }

        public void RequestUnloadCurrentContent(bool waitForContentReady = false)
        {
            TryRequestUnloadCurrentContent(waitForContentReady);
        }

        #endregion

        #region Private Methods

        private bool EnsureConfigured()
        {
            if (m_Configured)
            {
                return true;
            }

            Debug.LogError("[SceneFlow] SceneFlowController is not configured. Ensure SceneFlowHost (or bootstrap) ran Configure.");
            return false;
        }

        private IEnumerator SwitchContentRoutine(string targetSceneName, bool waitForContentReady)
        {
            m_IsTransitioning = true;
            m_ContentReady = false;

            var previousContent = m_CurrentContentSceneName;
            m_Bus.Publish(new OnSceneFlowEnterCoverStartedEvent(targetSceneName));

            yield return m_View.EnterCover();
            m_Bus.Publish(new OnSceneFlowEnterCoverCompletedEvent(targetSceneName));

            var asyncLoad = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);
            if (asyncLoad == null)
            {
                Debug.LogError($"[SceneFlow] LoadSceneAsync failed for '{targetSceneName}'. Is it in Build Settings?");
                m_IsTransitioning = false;
                m_Running = null;
                yield break;
            }

            asyncLoad.allowSceneActivation = false;
            while (asyncLoad.progress < 0.9f)
            {
                yield return null;
            }

            asyncLoad.allowSceneActivation = true;
            yield return asyncLoad;

            var loaded = SceneManager.GetSceneByName(targetSceneName);
            if (!loaded.IsValid() || !loaded.isLoaded)
            {
                Debug.LogError($"[SceneFlow] Scene '{targetSceneName}' did not load correctly.");
                m_IsTransitioning = false;
                m_Running = null;
                yield break;
            }

            SceneManager.SetActiveScene(loaded);
            m_CurrentContentSceneName = targetSceneName;
            m_Bus.Publish(new OnSceneFlowContentSceneActivatedEvent(targetSceneName));

            if (!string.IsNullOrEmpty(previousContent) && previousContent != m_ShellScene.name)
            {
                var prev = SceneManager.GetSceneByName(previousContent);
                if (prev.IsValid() && prev.isLoaded)
                {
                    yield return SceneManager.UnloadSceneAsync(prev);
                }
            }

            if (waitForContentReady)
            {
                m_ContentReady = false;
                yield return new WaitUntil(() => m_ContentReady);
            }

            m_Bus.Publish(new OnSceneFlowExitCoverStartedEvent(m_CurrentContentSceneName));
            yield return m_View.ExitCover();
            m_Bus.Publish(new OnSceneFlowExitCoverCompletedEvent(m_CurrentContentSceneName));
            m_Bus.Publish(new OnSceneFlowTransitionIdleEvent());

            m_IsTransitioning = false;
            m_Running = null;
        }

        private IEnumerator UnloadContentRoutine(bool waitForContentReady)
        {
            m_IsTransitioning = true;
            m_ContentReady = false;

            var contentName = m_CurrentContentSceneName;
            m_Bus.Publish(new OnSceneFlowEnterCoverStartedEvent(contentName));

            yield return m_View.EnterCover();
            m_Bus.Publish(new OnSceneFlowEnterCoverCompletedEvent(contentName));

            if (m_ShellScene.IsValid() && m_ShellScene.isLoaded)
            {
                SceneManager.SetActiveScene(m_ShellScene);
            }

            var contentScene = SceneManager.GetSceneByName(contentName);
            if (contentScene.IsValid() && contentScene.isLoaded)
            {
                yield return SceneManager.UnloadSceneAsync(contentScene);
            }

            m_CurrentContentSceneName = string.Empty;

            if (waitForContentReady)
            {
                m_ContentReady = false;
                yield return new WaitUntil(() => m_ContentReady);
            }

            m_Bus.Publish(new OnSceneFlowExitCoverStartedEvent(contentName));
            yield return m_View.ExitCover();
            m_Bus.Publish(new OnSceneFlowExitCoverCompletedEvent(contentName));
            m_Bus.Publish(new OnSceneFlowTransitionIdleEvent());

            m_IsTransitioning = false;
            m_Running = null;
        }

        #endregion
    }
}
