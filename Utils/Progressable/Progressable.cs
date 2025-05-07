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

        public void LinearTransition(float time)
        {
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);

            currentCoroutine = StartCoroutine(LinearTransitionCoroutine(time));
        }

        public void LinearTransition(float time, out Coroutine coroutine)
        {
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);

            coroutine = currentCoroutine = StartCoroutine(LinearTransitionCoroutine(time));
        }

        public Coroutine LinearTransition(float time, float delay = 0f)
        {
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);

            return currentCoroutine = StartCoroutine(LinearTransitionCoroutine(time, delay));
        }

        private IEnumerator LinearTransitionCoroutine(float time, float delay = 0f)
        {
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

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

        public void InverseLinearTransition(float time)
        {
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);

            currentCoroutine = StartCoroutine(InverseLinearTransitionCoroutine(time));
        }

        public void InverseLinearTransition(float time, out Coroutine coroutine)
        {
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);

            coroutine = currentCoroutine = StartCoroutine(InverseLinearTransitionCoroutine(time));
        }

        public Coroutine InverseLinearTransition(float time, float delay = 0f)
        {
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);

            return currentCoroutine = StartCoroutine(InverseLinearTransitionCoroutine(time, delay));
        }

        private IEnumerator InverseLinearTransitionCoroutine(float time, float delay = 0f)
        {
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

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

        public Coroutine PingPong(float time, float startValue = 0f, float endValue = 1f)
        {
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);

            return currentCoroutine = StartCoroutine(PingPongCoroutine(time, startValue, endValue));
        }

        private IEnumerator PingPongCoroutine(float time, float startValue, float endValue)
        {
            var elapsedTime = 0f;
            while (true)
            {
                elapsedTime = 0f;
                while (!Mathf.Approximately(Progress, endValue))
                {
                    Progress = Mathf.Lerp(startValue, endValue, elapsedTime / time);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                elapsedTime = 0f;
                while (!Mathf.Approximately(Progress, startValue))
                {
                    Progress = Mathf.Lerp(endValue, startValue, elapsedTime / time);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }
        }

        public void Lerp(float step)
        {
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);

            currentCoroutine = StartCoroutine(LerpCoroutine(0f, 1f, step));
        }

        public void InverseLerp(float step)
        {
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);

            currentCoroutine = StartCoroutine(LerpCoroutine(1f, 0f, step));
        }

        public IEnumerator LerpCoroutine(float startValue, float endValue, float step = 0.1f)
        {
            if (Mathf.Approximately(Progress, endValue)) yield break;

            while (!Mathf.Approximately(Progress, endValue))
            {
                Progress = Mathf.Lerp(Progress, endValue, step);
                yield return null;
            }
            Progress = ProgressCurve.Evaluate(endValue);
        }
    }
}