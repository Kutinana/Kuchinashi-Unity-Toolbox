using System.Collections;
using UnityEngine;

namespace Kuchinashi.Utils
{
    public class FollowTransform : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset;

        [Header("Additional Settings")]
        public bool followX = true;
        public bool followY = true;
        public bool followZ = true;

        public float thresholdDistance = 0.1f;

        [Header("Smooth Follow Settings")]
        public bool smoothFollow = false;
        public float smoothSpeed = 0.125f;
        private Coroutine m_SmoothFollowCoroutine;

        void FixedUpdate()
        {
            if (target == null) return;

            Vector3 targetPosition = target.position + offset;
            Vector3 currentPosition = transform.position;
            var newPosition = smoothFollow ? Vector3.Lerp(currentPosition, targetPosition, smoothSpeed) : targetPosition;

            if (Vector3.Distance(currentPosition, targetPosition) > thresholdDistance && smoothFollow)
            {
                if (m_SmoothFollowCoroutine != null) return;
                m_SmoothFollowCoroutine = StartCoroutine(SmoothFollowCoroutine());
            }
        }

        private IEnumerator SmoothFollowCoroutine()
        {
            while (!Mathf.Approximately(transform.position.x, target.position.x)
                || !Mathf.Approximately(transform.position.y, target.position.y)
                || !Mathf.Approximately(transform.position.z, target.position.z))
            {
                var newPosition = Vector3.Lerp(transform.position, target.position, smoothSpeed);
                transform.position = new Vector3(
                    followX ? newPosition.x : transform.position.x,
                    followY ? newPosition.y : transform.position.y,
                    followZ ? newPosition.z : transform.position.z
                );
                yield return new WaitForFixedUpdate();
            }
            m_SmoothFollowCoroutine = null;
        }
    }
}
