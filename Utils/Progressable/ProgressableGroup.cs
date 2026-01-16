using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kuchinashi.Utils.Progressable
{
    public class ProgressableGroup : Progressable
    {
        public List<Progressable> Progressables = new();

        protected override void ApplyEvaluation()
        {
            foreach (var progressable in Progressables)
            {
                if (progressable == null || progressable == this) continue;
                progressable.Progress = evaluation;

                if (!Application.isPlaying)
                    progressable.Apply();
            }
        }
    }
}