using Genetics;
using Genetics.GeneticDrivers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Genetics.Genes
{
    [CreateAssetMenu(fileName = "MendelianFloatGene", menuName = "Genetics/Genes/MendelianFloat", order = 2)]
    public class MendelianFloatGene : GeneEditor
    {
        public FloatGeneticDriver floatOutput;

        [Tooltip("The point between the min and max of the range to be treated as dominant. If .5, every value closer to the average of the range will be dominant over those farther away")]
        [Range(0, 1)]
        public float relativeDominantRange = 0;
        public float rangeMin = 0;
        public float rangeMax = 1;

        public int originIndex = 0;
        [Tooltip("sets the base pair size of this gene. higher number increases the precision, but also increases the number of genes which will have a small effect")]
        [Range(1, 32)]
        public int precision = 4;

        public override GeneSpan GeneUsage => new GeneSpan
        {
            start = new GeneIndex(originIndex),
            end = new GeneIndex(originIndex + precision)
        };
        public override bool AlwaysValid => true;

        public override bool Evaluate(CompiledGeneticDrivers editorHandle, SingleChromosomeCopy[] fullChromosomes)
        {
            if (editorHandle.TryGetGeneticData(floatOutput, out var _))
            {
                Debug.LogWarning($"Overwriting already set genetic driver {floatOutput} in gene {this}.lyzer");
            }
            var minimumDist = float.MaxValue;
            var dominantValue = 0d;
            foreach (var chromosomeCopy in fullChromosomes)
            {
                var value = EvaluateSingleGene(chromosomeCopy);
                var relativeVal = (value - rangeMin) / (rangeMax - rangeMin);
                var dist = Mathf.Abs((float)(relativeVal - relativeDominantRange));
                if(dist < minimumDist)
                {
                    minimumDist = dist;
                    dominantValue = value;
                }
            }
            editorHandle.SetGeneticDriverData(floatOutput, (float)dominantValue);
            return true;
        }
        private double EvaluateSingleGene(SingleChromosomeCopy gene)
        {
            var weight = gene.SampleBasePairs(GeneUsage) / (double)(1 << (GeneUsage.Length * 2));
            var adjusted = weight * (rangeMax - rangeMin) + rangeMin;
            return adjusted;
        }


        public override IEnumerable<GeneticDriver> GetInputs()
        {
            yield break;
        }

        public override IEnumerable<GeneticDriver> GetOutputs()
        {
            yield return floatOutput;
        }
    }
}