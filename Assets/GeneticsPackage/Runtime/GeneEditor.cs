using Genetics.GeneticDrivers;
using System.Collections.Generic;
using UnityEngine;

namespace Genetics
{
    [System.Serializable]
    [System.Obsolete]
    public struct SingleGene
    {
        private static char[] Replacements = new[] { 'A', 'C', 'T', 'G' };
        public ulong Value;
        public override string ToString()
        {
            return IntToString(Value, Replacements)
                .PadLeft(32, Replacements[0]);
        }

        private static string IntToString(ulong value, char[] baseChars)
        {
            string result = string.Empty;
            ulong targetBase = (ulong)baseChars.Length;

            do
            {
                result = baseChars[value % targetBase] + result;
                value = value / targetBase;
            }
            while (value > 0);

            return result;
        }
    }

    /// <summary>
    /// Represents zero or one genes, and how the gene and other genetic drives should be interpreted
    /// </summary>
    public abstract class GeneEditor : ScriptableObject
    {
        /// <summary>
        /// Used to determine evaluation order of genes. return nothing if this gene is not effected by other genes
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<GeneticDriver> GetInputs();
        /// <summary>
        /// Used to determine evaluation order of genes. This indicates what data the gene editor will set
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<GeneticDriver> GetOutputs();

        public abstract GeneSpan GeneUsage { get; }

        /// <summary>
        /// If this gene will always be valid regardles of underlying genetic information
        /// </summary>
        public abstract bool AlwaysValid { get; }

        /// <summary>
        /// evaluate this gene, put output into the genetic drivers handle. Can return false to indicate this gene causes the 
        ///     zigote to be non viable
        /// </summary>
        /// <param name="editorHandle">a handle of data</param>
        /// <param name="data">gene data including chromosomal duplicates. first dimension is unique genes, length 
        ///     equal to <see cref="GeneSize"/>. Second dimension is duplicate copies of the chromosome</param>
        /// <returns>whether the gene is valid or not</returns>
        public abstract bool Evaluate(CompiledGeneticDrivers editorHandle, SingleChromosomeCopy[] fullChromosomes);
    }
}