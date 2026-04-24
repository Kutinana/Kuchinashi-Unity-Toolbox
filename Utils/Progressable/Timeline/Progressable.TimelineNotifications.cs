using UnityEngine.Playables;
using Kuchinashi.Utils.Progressable.Timeline;

namespace Kuchinashi.Utils.Progressable
{
    /// <summary>
    /// 接收 Timeline 上 <see cref="ProgressableResetSignal"/> 的通知并重置进度。
    /// 需在 Timeline 中将 <see cref="ProgressableTrack"/> 绑定到本组件。
    /// </summary>
    public partial class Progressable : INotificationReceiver
    {
        public void OnNotify(Playable playable, INotification notification, object context)
        {
            if (notification is ProgressableResetSignal)
            {
                Progress = 0f;
                Apply();
            }
        }
    }
}
