using UnityEngine.Timeline;

namespace Kuchinashi.Utils.Progressable.Timeline
{
    /// <summary>
    /// 绑定到要驱动的 <see cref="Progressable"/>；绑定后可在轨道上添加 <see cref="ProgressableResetSignal"/> 标记。
    /// </summary>
    [TrackBindingType(typeof(Progressable))]
    [TrackColor(0.35f, 0.75f, 0.45f)]
    [TrackClipType(typeof(ProgressableClip))]
    public sealed class ProgressableTrack : TrackAsset
    {
    }
}
