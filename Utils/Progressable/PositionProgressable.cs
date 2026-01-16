using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kuchinashi.Utils.Progressable
{
    public class PositionProgressable : Progressable
    {
        public Transform TargetTransform;

        public Vector3 StartPosition;
        public Vector3 EndPosition;

        private void Awake()
        {
            if (TargetTransform == null) TargetTransform = transform;
        }

        protected override void ApplyEvaluation()
        {
            if (TargetTransform == null) return;
            
            var next = Vector3.Lerp(StartPosition, EndPosition, evaluation);
            if (TargetTransform.localPosition != next)
                TargetTransform.localPosition = next;
        }

        protected override void OnValidate()
        {
            if (TargetTransform == null) TargetTransform = transform;
            base.OnValidate();
        }
    }
}