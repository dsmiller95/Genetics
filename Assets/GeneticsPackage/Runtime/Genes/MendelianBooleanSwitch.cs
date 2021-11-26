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

        public override int GeneSize => 1;

        public override void Evaluate(CompiledGeneticDrivers editorHandle, GeneCopies[] genes)
        {
            if (editorHandle.TryGetGeneticData(switchOutput, out var _))
            {
                Debug.LogWarning($"Overwriting already set genetic driver {switchOutput} in gene {this}.");
            }
            var gene = genes[0];
            var booleanOutput = gene.chromosomalCopies.Any(x => HammingUtilities.EvenSplitHammingWeight(x.Value));

            editorHandle.SetGeneticDriverData(switchOutput, booleanOutput);
        }

        public override SingleGene[] GenerateGeneData(System.Random randomGen)
        {
            ulong newGene = HammingUtilities.RandomEvenHammingWeight(randomGen);

            return new SingleGene[] { new SingleGene { Value = newGene } };
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
