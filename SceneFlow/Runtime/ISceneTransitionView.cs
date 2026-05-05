using System.Collections;

namespace Kuchinashi.SceneFlow
{
    /// <summary>
    /// Two-phase presentation contract. Controller awaits each IEnumerator before continuing.
    /// Cover = block input / show loading presentation until ready for scene work.
    /// </summary>
    public interface ISceneTransitionView
    {
        /// <summary>
        /// Raise loading UI (or equivalent). End enumerator when presentation is fully ready; Controller then loads scenes.
        /// </summary>
        IEnumerator EnterCover();

        /// <summary>
        /// Called when load and optional content-ready gate are done. End enumerator when screen is released (auto or after user input).
        /// </summary>
        IEnumerator ExitCover();
    }
}
