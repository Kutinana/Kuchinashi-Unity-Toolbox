using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Kuchinashi.Utils.Progressable.Timeline
{
    /// <summary>
    /// Timeline 片段：在片段时长内把绑定目标的 <see cref="Progressable.Progress"/> 从 0 线性驱动到 1（或勾选 Inverse 时从 1 到 0）。
    /// </summary>
    public sealed class ProgressableClip : PlayableAsset, ITimelineClipAsset
    {
        [Tooltip("要驱动的 Progressable；在 Timeline 窗口中从场景拖入以建立 Exposed Reference。")]
        public ExposedReference<Progressable> Target;

        [Tooltip("勾选后等价于 InverseLinearTransition：进度随时间从 1 线性回到 0。")]
        public bool Inverse;

        public ClipCaps clipCaps => ClipCaps.None;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<ProgressablePlayableBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();

            var director = owner != null ? owner.GetComponent<PlayableDirector>() : null;
            behaviour.Target = director != null ? Target.Resolve(director) : null;
            behaviour.Inverse = Inverse;

            return playable;
        }
    }
}
