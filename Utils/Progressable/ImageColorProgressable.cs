using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Kuchinashi.Utils.Progressable
{
    public class ImageColorProgressable : Progressable
    {
        public Image TargetImage;

        public Color StartColor = Color.white;
        public Color EndColor = Color.white;

        private void Awake()
        {
            if (TargetImage == null) TargetImage = TryGetComponent<Image>(out var image) ? image : null;
        }

        protected override void ApplyEvaluation()
        {
            if (TargetImage == null) return;
            
            var next = Color.Lerp(StartColor, EndColor, evaluation);
            if (TargetImage.color != next)
                TargetImage.color = next;
        }

        protected override void OnValidate()
        {
            if (TargetImage == null) TargetImage = TryGetComponent<Image>(out var image) ? image : null;
            base.OnValidate();
        }
    }
}