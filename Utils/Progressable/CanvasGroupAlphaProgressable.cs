using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kuchinashi.Utils.Progressable
{
    public class CanvasGroupAlphaProgressable : Progressable
    {
        [SerializeField] private CanvasGroup TargetCanvasGroup;

        [Header("Settings")]
        public float StartAlpha = 0f;
        public float EndAlpha = 1f;
        public bool IsInteractable = true;
        public bool IsBlockRaycasts = true;

        private void Awake()
        {
            if (TargetCanvasGroup == null) TargetCanvasGroup = TryGetComponent<CanvasGroup>(out var cg) ? cg : null;
        }

        protected override void ApplyEvaluation()
        {
            if (TargetCanvasGroup == null) return;

            var nextAlpha = Mathf.Lerp(StartAlpha, EndAlpha, evaluation);
            if (!Mathf.Approximately(TargetCanvasGroup.alpha, nextAlpha))
                TargetCanvasGroup.alpha = nextAlpha;
            
            if (Mathf.Approximately(TargetCanvasGroup.alpha, 1f))
            {
                TargetCanvasGroup.blocksRaycasts = IsBlockRaycasts;
                TargetCanvasGroup.interactable = IsInteractable;
            }
            else
            {
                TargetCanvasGroup.blocksRaycasts = false;
                TargetCanvasGroup.interactable = false;
            }
        }

        protected override void OnValidate()
        {
            if (TargetCanvasGroup == null) TargetCanvasGroup = TryGetComponent<CanvasGroup>(out var cg) ? cg : null;
            base.OnValidate();
        }
    }
}