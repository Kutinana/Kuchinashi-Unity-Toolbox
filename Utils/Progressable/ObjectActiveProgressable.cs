using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kuchinashi.Utils.Progressable
{
    public class ObjectActiveProgressable : Progressable
    {
        [SerializeField] private GameObject TargetObject;

        [Header("Settings")]
        [Range(0f, 1f)] public float ActiveThreshold = 0.5f;

        protected override void ApplyEvaluation()
        {
            if (TargetObject == null) return;
            
            var shouldActive = evaluation > ActiveThreshold;
            if (TargetObject.activeSelf != shouldActive)
                TargetObject.SetActive(shouldActive);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
        }
    }
}