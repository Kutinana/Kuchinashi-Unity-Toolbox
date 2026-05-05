using System;
using QFramework;
using UnityEngine;

namespace Kuchinashi.SceneFlow.Integrations.QFramework
{
    /// <summary>
    /// Bridges <see cref="ISceneMessageBus"/> to QFramework <see cref="TypeEventSystem.Global"/>.
    /// </summary>
    /// <remarks>
    /// <para><b>Assembly setup:</b> This type lives in <c>Kuchinashi.SceneFlow.QFramework</c>, which is not auto-referenced.
    /// If your gameplay code uses a custom Assembly Definition, add a reference to <c>Kuchinashi.SceneFlow.QFramework</c>
    /// (in addition to <c>Kuchinashi.SceneFlow.Runtime</c>) on the asmdef that needs this component. Replace
    /// <see cref="InMemorySceneMessageBus"/> on the shell with this component when you want events on the global TypeEventSystem.</para>
    /// </remarks>
    public sealed class QFrameworkSceneMessageBus : MonoBehaviour, ISceneMessageBus
    {
        public void Publish<T>(in T evt) where T : struct
        {
            TypeEventSystem.Global.Send(evt);
        }

        public IDisposable Subscribe<T>(Action<T> handler) where T : struct
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            var token = TypeEventSystem.Global.Register<T>(handler);
            return new UnregisterDisposable(() => token.UnRegister());
        }

        private sealed class UnregisterDisposable : IDisposable
        {
            private Action m_OnDispose;

            public UnregisterDisposable(Action onDispose)
            {
                m_OnDispose = onDispose;
            }

            public void Dispose()
            {
                m_OnDispose?.Invoke();
                m_OnDispose = null;
            }
        }
    }
}
