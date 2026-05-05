using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kuchinashi.SceneFlow
{
    /// <summary>
    /// Default in-process message bus; no third-party dependencies.
    /// </summary>
    public sealed class InMemorySceneMessageBus : MonoBehaviour, ISceneMessageBus
    {
        private readonly Dictionary<Type, List<Delegate>> m_Subscribers = new Dictionary<Type, List<Delegate>>();

        public void Publish<T>(in T evt) where T : struct
        {
            var type = typeof(T);
            if (!m_Subscribers.TryGetValue(type, out var list))
            {
                return;
            }

            var snapshot = list.ToArray();
            for (var i = 0; i < snapshot.Length; i++)
            {
                if (snapshot[i] is Action<T> action)
                {
                    action.Invoke(evt);
                }
            }
        }

        public IDisposable Subscribe<T>(Action<T> handler) where T : struct
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            var type = typeof(T);
            if (!m_Subscribers.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                m_Subscribers[type] = list;
            }

            list.Add(handler);
            return new Subscription(this, type, handler);
        }

        private void Unsubscribe(Type type, Delegate handler)
        {
            if (!m_Subscribers.TryGetValue(type, out var list))
            {
                return;
            }

            list.Remove(handler);
            if (list.Count == 0)
            {
                m_Subscribers.Remove(type);
            }
        }

        private sealed class Subscription : IDisposable
        {
            private InMemorySceneMessageBus m_Bus;
            private readonly Type m_Type;
            private readonly Delegate m_Handler;

            public Subscription(InMemorySceneMessageBus bus, Type type, Delegate handler)
            {
                m_Bus = bus;
                m_Type = type;
                m_Handler = handler;
            }

            public void Dispose()
            {
                if (m_Bus == null)
                {
                    return;
                }

                m_Bus.Unsubscribe(m_Type, m_Handler);
                m_Bus = null;
            }
        }
    }
}
