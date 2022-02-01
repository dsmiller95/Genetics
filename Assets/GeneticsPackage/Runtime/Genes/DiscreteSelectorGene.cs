using Genetics.GeneticDrivers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Genetics.Genes
{
    [CreateAssetMenu(fileName = "DiscreteSelectorGene", menuName = "Genetics/Genes/DiscreteSelectorGene", order = 2)]
    public class DiscreteSelectorGene : GeneEditor
    {
        [Tooltip("dominance is given to lower output classes")]
        public GeneticDriver<float> discreteOutput;
        [Range(1, 64)]
        public int maxDiscreteOutputClasses;

        public int originIndex = 0;
        [Tooltip("Extra alleles added beyond the minimum number required to select from the discrete output clases. If enforcing unique combinations, higher numbers result in more non-viable zygotes")]
        public int additionalVolatility;
        [Tooltip("When set, ensures that every discrete output can only result from one unique combination of genes")]
        public bool enforceUniqueCombination;

        public override GeneSpan GeneUsage => new GeneSpan
        {
            start = new GeneIndex(originIndex),
            end = new GeneIndex(originIndex + Mathf.CeilToInt(Mathf.Log(maxDiscreteOutputClasses, 4)) + additionalVolatility)
        };

        public override bool AlwaysValid => !enforceUniqueCombination;

        public override bool Evaluate(CompiledGeneticDrivers editorHandle, SingleChromosomeCopy[] fullChromosomes)
        {
            if (editorHandle.TryGetGeneticData(discreteOutput, out var _))
            {
                Debug.LogWarning($"Overwriting already set genetic driver {discreteOutput} in gene {this}.");
            }
            var span = GeneUsage;

            var min = ulong.MaxValue;
            for (int i = 0; i < fullChromosomes.Length; i++)
            {
                var value = fullChromosomes[i].SampleBasePairs(span);
                if (enforceUniqueCombination && value >= (ulong)maxDiscreteOutputClasses)
                {
                    return false;
                }
                min = Math.Min(value % (ulong)maxDiscreteOutputClasses, min);
            }
            editorHandle.SetGeneticDriverData(discreteOutput, min);
            return true;
        }

        public override IEnumerable<GeneticDriver> GetInputs()
        {
            yield break;
        }

        public override IEnumerable<GeneticDriver> GetOutputs()
        {
            yield return discreteOutput;
        }
    }
}
