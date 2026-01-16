using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kuchinashi.Utils.Progressable
{
    public class ScaleProgressable : Progressable
    {
        public Transform TargetTransform;

        public Vector3 StartScale = Vector3.one;
        public Vector3 EndScale = Vector3.one;

        private void Awake()
        {
            if (TargetTransform == null) TargetTransform = transform;
        }

        protected override void ApplyEvaluation()
        {
            if (TargetTransform == null) return;
            
            var next = Vector3.Lerp(StartScale, EndScale, evaluation);
            if (TargetTransform.localScale != next)
                TargetTransform.localScale = next;
        }

        protected override void OnValidate()
        {
            if (TargetTransform == null) TargetTransform = transform;
            base.OnValidate();
        }
    }
}