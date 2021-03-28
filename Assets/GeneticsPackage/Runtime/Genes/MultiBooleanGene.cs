using Genetics;
using Genetics.Genes;
using Genetics.GeneticDrivers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Plants
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

        public override int GeneSize => outputDrivers.Length;

        public override void Evaluate(CompiledGeneticDrivers editorHandle, GeneCopies[] genes)
        {
            for (int geneIndex = 0; geneIndex < genes.Length; geneIndex++)
            {
                var switchOutput = outputDrivers[geneIndex];
                if (editorHandle.TryGetGeneticData(switchOutput, out var _))
                {
                    Debug.LogWarning($"Overwriting already set genetic driver {switchOutput} in gene {this}.");
                }

                var gene = genes[geneIndex];
                var dominantValue = dominantValues[geneIndex];
                bool geneOutput;
                if (dominantValue == true)
                    geneOutput = gene.chromosomalCopies.Any(x => EvaluateSingleGene(x));
                else
                    geneOutput = gene.chromosomalCopies.All(x => EvaluateSingleGene(x));
                editorHandle.SetGeneticDriverData(switchOutput, geneOutput);
            }
        }

        private bool EvaluateSingleGene(SingleGene gene)
        {
            return MendelianBooleanSwitch.HammingWeight(gene.Value) > 32;
        }

        public override IEnumerable<GeneticDriver> GetInputs()
        {
            yield break;
        }

        public override IEnumerable<GeneticDriver> GetOutputs()
        {
            return outputDrivers;
        }

        public override SingleGene[] GenerateGeneData()
        {
            var genes = new SingleGene[GeneSize];
            for (int i = 0; i < genes.Length; i++)
            {
                genes[i] = new SingleGene
                {
                    Value = GenerateRandomWithUniformHammingWeightDistribution()
                };
            }
            return genes;
        }

        private ulong GenerateRandomWithUniformHammingWeightDistribution()
        {
            ulong newGene = 0;
            var randomGen = new System.Random(Random.Range(int.MinValue, int.MaxValue));
            var binaryProportionalChance = randomGen.NextDouble();
            for (int i = 0; i < sizeof(ulong) * 8; i++)
            {
                var nextBit = randomGen.NextDouble() > binaryProportionalChance;
                if (nextBit)
                {
                    newGene |= ((ulong)1) << i;
                }
            }
            return newGene;
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