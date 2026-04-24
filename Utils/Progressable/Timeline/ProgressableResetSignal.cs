using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Kuchinashi.Utils.Progressable.Timeline
{
    /// <summary>
    /// 放在 <see cref="ProgressableTrack"/> 上：播放到该时刻时向轨道绑定对象发送通知，
    /// 由同物体上的 <see cref="Progressable"/>（实现 <see cref="INotificationReceiver"/> 的 partial）将 <see cref="Progressable.Progress"/> 置 0 并 <see cref="Progressable.Apply"/>。
    /// </summary>
    [Serializable]
    public class ProgressableResetSignal : Marker, INotification, INotificationOptionProvider
    {
        [SerializeField] bool m_Retroactive = true;
        [SerializeField] bool m_EmitOnce;

        /// <summary>若播放起点晚于本标记时间，是否补发一次（与 Timeline Signal 行为一致）。</summary>
        public bool retroactive
        {
            get => m_Retroactive;
            set => m_Retroactive = value;
        }

        /// <summary>循环播放时是否只触发一次。</summary>
        public bool emitOnce
        {
            get => m_EmitOnce;
            set => m_EmitOnce = value;
        }

        PropertyName INotification.id => new PropertyName("Kuchinashi/Progressable/Reset");

        NotificationFlags INotificationOptionProvider.flags =>
            (retroactive ? NotificationFlags.Retroactive : default) |
            (emitOnce ? NotificationFlags.TriggerOnce : default) |
            NotificationFlags.TriggerInEditMode;
    }
}
