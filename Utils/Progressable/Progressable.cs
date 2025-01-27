using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kuchinashi.Utils.Progressable
{
    [ExecuteInEditMode]
    public class Progressable : MonoBehaviour
    {
        [Range(0, 1)] public float Progress = 0;
        public AnimationCurve ProgressCurve = AnimationCurve.Linear(0, 0, 1, 1);

        protected float evaluation;
        protected Coroutine currentCoroutine = null;

        protected virtual void Update()
        {
            evaluation = ProgressCurve.Evaluate(Progress);
        }

        public Coroutine LinearTransition(float time)
        {
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);

            return currentCoroutine = StartCoroutine(LinearTransitionCoroutine(time));
        }

        private IEnumerator LinearTransitionCoroutine(float time)
        {
            float elapsedTime = 0f;
            float startValue = Progress;

            while (elapsedTime < time)
            {
                Progress = Mathf.Lerp(startValue, 1f, elapsedTime / time);
                elapsedTime += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }

            Progress = 1f;
        }

        public Coroutine InverseLinearTransition(float time)
        {
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);

            return currentCoroutine = StartCoroutine(InverseLinearTransitionCoroutine(time));
        }

        private IEnumerator InverseLinearTransitionCoroutine(float time)
        {
            float elapsedTime = 0f;
            float startValue = Progress;

            while (elapsedTime < time)
            {
                Progress = Mathf.Lerp(startValue, 0f, elapsedTime / time);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            Progress = 0f;
        }

        public void PingPong(float time)
        {
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);

            currentCoroutine = StartCoroutine(PingPongCoroutine(time));
        }

        private IEnumerator PingPongCoroutine(float time)
        {
            while (true)
            {
                yield return LinearTransitionCoroutine(time);
                yield return null;
                yield return InverseLinearTransitionCoroutine(time);
            }
        }
    }
}