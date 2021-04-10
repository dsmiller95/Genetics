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
        public override int GeneSize => 1;

        [Tooltip("The point between the min and max of the range to be treated as dominant. If .5, every value closer to the average of the range will be dominant over those farther away")]
        [Range(0, 1)]
        public float relativeDominantRange = 0;
        public float rangeMin = 0;
        public float rangeMax = 1;

        public override void Evaluate(CompiledGeneticDrivers editorHandle, GeneCopies[] data)
        {
            if (editorHandle.TryGetGeneticData(floatOutput, out var _))
            {
                Debug.LogWarning($"Overwriting already set genetic driver {floatOutput} in gene {this}.lyzer");
            }
            var gene = data[0];

            var minimumDist = float.MaxValue;
            var dominantValue = 0d;
            foreach (var chromosomeCopy in gene.chromosomalCopies)
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
        }
        private double EvaluateSingleGene(SingleGene gene)
        {
            var weight = HammingUtilities.HammingWeight(gene.Value);
            var adjusted = (weight / 64d) * (rangeMax - rangeMin) + rangeMin;
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

        public override SingleGene[] GenerateGeneData(System.Random randomGen)
        {
            ulong newGene = HammingUtilities.RandomEvenHammingWeight(randomGen);

            return new SingleGene[] { new SingleGene { Value = newGene } };
        }
    }
}