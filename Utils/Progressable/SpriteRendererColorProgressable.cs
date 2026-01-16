using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Kuchinashi.Utils.Progressable
{
    public class SpriteRendererColorProgressable : Progressable
    {
        public SpriteRenderer TargetRenderer;

        public Color StartColor = Color.white;
        public Color EndColor = Color.white;

        private void Awake()
        {
            if (TargetRenderer == null) TargetRenderer = TryGetComponent<SpriteRenderer>(out var renderer) ? renderer : null;
        }

        protected override void ApplyEvaluation()
        {
            if (TargetRenderer == null) return;
            
            var next = Color.Lerp(StartColor, EndColor, evaluation);
            if (TargetRenderer.color != next)
                TargetRenderer.color = next;
        }

        protected override void OnValidate()
        {
            if (TargetRenderer == null) TargetRenderer = TryGetComponent<SpriteRenderer>(out var renderer) ? renderer : null;
            base.OnValidate();
        }
    }
}