using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kuchinashi.Utils.Progressable
{
    public class RectPositionProgressable : Progressable
    {
        public RectTransform TargetRectTransform;

        public Vector2 StartPosition;
        public Vector2 EndPosition;

        private void Awake()
        {
            if (TargetRectTransform == null) TargetRectTransform = GetComponent<RectTransform>();
        }

        protected override void ApplyEvaluation()
        {
            if (TargetRectTransform == null) return;
            
            var next = Vector2.Lerp(StartPosition, EndPosition, evaluation);
            if (TargetRectTransform.anchoredPosition != next)
                TargetRectTransform.anchoredPosition = next;
        }

        protected override void OnValidate()
        {
            if (TargetRectTransform == null) TargetRectTransform = GetComponent<RectTransform>();
            base.OnValidate();
        }
    }
}