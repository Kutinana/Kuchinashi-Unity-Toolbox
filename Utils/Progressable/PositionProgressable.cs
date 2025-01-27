using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kuchinashi.Utils.Progressable
{
    [ExecuteInEditMode]
    public class PositionProgressable : Progressable
    {
        public Transform TargetTransform;

        public Vector3 StartPosition;
        public Vector3 EndPosition;

        private void Awake()
        {
            if (TargetTransform == null) TargetTransform = transform;
        }

        protected override void Update()
        {
            if (TargetTransform == null) return;
            base.Update();

            TargetTransform.localPosition = Vector3.Lerp(StartPosition, EndPosition, evaluation);
        }
    }
}