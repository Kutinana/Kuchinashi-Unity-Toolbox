using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Kuchinashi.Utils.Progressable
{
    public class TMPTextColorProgressable : Progressable
    {
        public TMP_Text TargetText;
        
        public Color StartColor;
        public Color EndColor;

        private void Awake()
        {
            if (TargetText == null) TargetText = GetComponent<TMP_Text>();
        }

        protected override void ApplyEvaluation()
        {
            if (TargetText == null) return;
            
            var next = Color.Lerp(StartColor, EndColor, evaluation);
            if (TargetText.color != next)
                TargetText.color = next;
        }

        protected override void OnValidate()
        {
            if (TargetText == null) TargetText = GetComponent<TMP_Text>();
            base.OnValidate();
        }
    }
}