using System.Collections;
using UnityEngine;

namespace Kuchinashi.SceneFlow
{
    /// <summary>
    /// Completes both phases immediately; no visual transition.
    /// </summary>
    public sealed class NoOpSceneTransitionView : SceneTransitionViewBehaviour
    {
        public override IEnumerator EnterCover()
        {
            yield break;
        }

        public override IEnumerator ExitCover()
        {
            yield break;
        }
    }
}
