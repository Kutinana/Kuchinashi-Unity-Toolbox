using UnityEngine;
using UnityEngine.Playables;

namespace Kuchinashi.Utils.Progressable.Timeline
{
    /// <summary>
    /// 在片段有效时间内将目标 <see cref="Progressable.Progress"/> 从 0 线性推到 1，并调用 <see cref="Progressable.Apply"/>。
    /// </summary>
    public sealed class ProgressablePlayableBehaviour : PlayableBehaviour
    {
        public Progressable Target;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (Target == null)
                return;

            double duration = playable.GetDuration();
            if (duration <= 1e-6)
            {
                Target.Progress = 0f;
                Target.Apply();
                return;
            }

            double t = playable.GetTime();
            float normalized = Mathf.Clamp01((float)(t / duration));
            Target.Progress = normalized;
            Target.Apply();
        }
    }
}
