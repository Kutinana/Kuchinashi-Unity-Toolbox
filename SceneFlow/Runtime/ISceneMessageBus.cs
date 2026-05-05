using System;

namespace Kuchinashi.SceneFlow
{
    /// <summary>
    /// Project-agnostic publish/subscribe bus for scene-flow and UI coordination.
    /// </summary>
    public interface ISceneMessageBus
    {
        void Publish<T>(in T evt) where T : struct;

        /// <summary>
        /// Subscribe to events of type <typeparamref name="T"/>. Dispose to unsubscribe.
        /// </summary>
        IDisposable Subscribe<T>(Action<T> handler) where T : struct;
    }
}
