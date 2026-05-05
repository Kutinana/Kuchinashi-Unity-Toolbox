using System.Collections;
using UnityEngine;

namespace Kuchinashi.SceneFlow
{
    /// <summary>
    /// Use as the serialized field type on <see cref="SceneFlowHost"/> so Unity can show and persist a component reference while keeping the <see cref="ISceneTransitionView"/> contract.
    /// </summary>
    public abstract class SceneTransitionViewBehaviour : MonoBehaviour, ISceneTransitionView
    {
        public abstract IEnumerator EnterCover();

        public abstract IEnumerator ExitCover();
    }
}
