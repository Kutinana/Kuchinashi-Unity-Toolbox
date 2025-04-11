using System.Collections;
using Homebrew;
using UnityEngine;

namespace Kuchinashi.Utils
{
    public class FollowTransform : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset;

        public bool followX = true;
        public bool followY = true;
        public bool followZ = true;

        public float thresholdDistance = 0.1f;

        [Foldout("Smooth Follow Settings", true)]
        public bool smoothFollow = false;
        public float smoothSpeed = 0.125f;

        void FixedUpdate()
        {
            if (target == null) return;

            Vector3 targetPosition = target.position + offset;
            Vector3 currentPosition = transform.position;
            var newPosition = smoothFollow ? Vector3.Lerp(currentPosition, targetPosition, smoothSpeed) : targetPosition;

            if (Vector3.Distance(currentPosition, targetPosition) > thresholdDistance)
            {
                transform.position = new Vector3(
                    followX ? newPosition.x : currentPosition.x,
                    followY ? newPosition.y : currentPosition.y,
                    followZ ? newPosition.z : currentPosition.z
                );
            }
        }
    }
}
