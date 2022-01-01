using Genetics.GeneticDrivers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Genetics.Genes
{
    [CreateAssetMenu(fileName = "MendelianBooleanGene", menuName = "Genetics/Genes/MendelianBoolean", order = 2)]
    public class MendelianBooleanSwitch : GeneEditor
    {
        public BooleanGeneticDriver switchOutput;

        public int originIndex = 0;
        [Tooltip("sets the base pair size of this gene. higher number increases the chance than a mutation will hit the gene")]
        [Range(1, 32)]
        public int volatility = 2;

        public override GeneSpan GeneUsage => new GeneSpan
        {
            start = new GeneIndex(originIndex),
            end = new GeneIndex(originIndex + volatility)
        };
        public override bool AlwaysValid => true;

        public override bool Evaluate(CompiledGeneticDrivers editorHandle, SingleChromosomeCopy[] fullChromosomes)
        {
            if (editorHandle.TryGetGeneticData(switchOutput, out var _))
            {
                Debug.LogWarning($"Overwriting already set genetic driver {switchOutput} in gene {this}.");
            }
            var span = GeneUsage;
            var booleanOutput = fullChromosomes.Any(x => HammingUtilities.EvenSplitHammingWeight(x.SampleBasePairs(span)));

            editorHandle.SetGeneticDriverData(switchOutput, booleanOutput);
            return true;
        }

        public override IEnumerable<GeneticDriver> GetInputs()
        {
            yield break;
        }

        public override IEnumerable<GeneticDriver> GetOutputs()
        {
            yield return switchOutput;
        }
    }
}
