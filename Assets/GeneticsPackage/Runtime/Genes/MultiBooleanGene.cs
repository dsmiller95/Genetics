using Genetics.GeneticDrivers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Genetics.Genes
{
    /// <summary>
    /// Will output a single whole number value, based on the result of combining multiple boolean genes together. For ever True gene,
    ///     the output will increment by one. Producing a binomial distribution of output values when input genes are randomized
    /// </summary>
    [CreateAssetMenu(fileName = "MultiBooleanGene", menuName = "Genetics/Genes/MultiBooleanGene", order = 2)]
    public class MultiBooleanGene : GeneEditor
    {
        [Tooltip("A list of all boolean genes to populate")]
        public BooleanGeneticDriver[] outputDrivers;
        public bool[] dominantValues;

        public int originIndex = 0;
        [Tooltip("sets the base pair size of this gene. higher number increases the chance than a mutation will hit the gene")]
        [Range(1, 32)]
        public int volatility = 2;

        public override GeneSpan GeneUsage => new GeneSpan
        {
            start = new GeneIndex(originIndex),
            end = new GeneIndex(originIndex + volatility * outputDrivers.Length)
        };
        public override bool AlwaysValid => true;

        public override bool Evaluate(CompiledGeneticDrivers editorHandle, SingleChromosomeCopy[] fullChromosomes)
        {
            for (int geneIndex = 0; geneIndex < outputDrivers.Length; geneIndex++)
            {
                var switchOutput = outputDrivers[geneIndex];
                if (editorHandle.TryGetGeneticData(switchOutput, out var _))
                {
                    Debug.LogWarning($"Overwriting already set genetic driver {switchOutput} in gene {this}.");
                }

                var sampleOrigin = originIndex + volatility * geneIndex;
                var sampleSpan = new GeneSpan
                {
                    start = new GeneIndex(sampleOrigin),
                    end = new GeneIndex(sampleOrigin + volatility),
                };
                var dominantValue = dominantValues[geneIndex];
                bool geneOutput;
                if (dominantValue == true)
                    geneOutput = fullChromosomes.Any(x => HammingUtilities.EvenSplitHammingWeight(x.SampleBasePairs(sampleSpan)));
                else
                    geneOutput = fullChromosomes.All(x => HammingUtilities.EvenSplitHammingWeight(x.SampleBasePairs(sampleSpan)));
                editorHandle.SetGeneticDriverData(switchOutput, geneOutput);
            }
            return true;
        }

        public override IEnumerable<GeneticDriver> GetInputs()
        {
            yield break;
        }

        public override IEnumerable<GeneticDriver> GetOutputs()
        {
            return outputDrivers;
        }
        private void OnValidate()
        {
            if (outputDrivers.Length != dominantValues.Length)
            {
                dominantValues = dominantValues
                    .Concat(Enumerable.Repeat(false, outputDrivers.Length))
                    .Take(outputDrivers.Length)
                    .ToArray();
            }
        }
    }
}