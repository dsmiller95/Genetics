using Dman.Utilities;
using Genetics.GeneticDrivers;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Genetics
{
    [System.Serializable]
    public class SingleChromosomeCopy
    {
        public byte[] chromosomeData;

        public ulong SampleBasePairs(GeneSpan span)
        {
            if(span.Length > 32)
            {
                throw new System.Exception("gene span too big");
            }

            ulong result = 0;
            for (int i = 0; i < span.Length; i++)
            {
                var originIndex = span.start + new GeneIndex(i);
                var sample = SampleIndex(originIndex);
                result = (result << 2) | sample;
            }

            return result;
        }

        public byte SampleIndex(GeneIndex index)
        {
            var byteIndex = index.IndexToByteData;
            var shift = index.IndexInsideByte * 2;
            var mask = 0b11 << shift;
            return (byte)((chromosomeData[byteIndex] & mask) >> shift);
        }
        public void SetBasePairAtIndex(GeneIndex index, byte newBasePair)
        {
            var byteIndex = index.IndexToByteData;
            var shift = index.IndexInsideByte * 2;

            var mask = 0b11 << shift;
            chromosomeData[byteIndex] = (byte)((chromosomeData[byteIndex] & ~mask) | ((newBasePair << shift) & mask));
        }
    }

    [System.Serializable]
    public class Chromosome
    {
        /// <summary>
        /// array to represent all genes, and copies of genes inside this chromosome
        /// first dimension is for each unique gene. each gene contains all of the
        ///     chromosomal copies of that same gene
        /// </summary>
        public SingleChromosomeCopy[] allGeneData;

        public Chromosome(params Chromosome[] parentalChromosomes)
        {
            if (parentalChromosomes.Length == 0)
            {
                return;
            }

            var chromosomeSize = parentalChromosomes[0].allGeneData.Length;
            if (parentalChromosomes.Any(x => x.allGeneData.Length != chromosomeSize))
            {
                throw new System.ArgumentException("all chromosomes must be equal in size");
            }
            var chromosomalCopies = parentalChromosomes[0].allGeneData.Length;
            var parentSelections = ArrayExtensions.SelectIndexSources(chromosomalCopies, parentalChromosomes.Length);
            allGeneData = parentSelections
                .Select(parentIndex => parentalChromosomes[parentIndex].SampleSingleCopyFromChromosome())
                .ToArray();
        }

        /// <summary>
        /// Get a single copy of every individual gene. This step simulates the recombination which happens during
        ///     meosis: taking all genes which were originally passed down from the parents, and combining them into
        ///     a single instance of each gene to be passed on to children
        /// </summary>
        /// <returns></returns>
        private SingleChromosomeCopy SampleSingleCopyFromChromosome()
        {
            var chromosomeCopies = allGeneData.Length;
            var resultingChromosome = new byte[allGeneData[0].chromosomeData.Length];
            for (int i = 0; i < allGeneData[0].chromosomeData.Length; i++)
            {
                byte resultByte = 0;
                for (int filterWindow = 0b11; filterWindow < byte.MaxValue; filterWindow = filterWindow << 2)
                {
                    var geneSelection = Random.Range(0, chromosomeCopies);
                    int selectedBasePair = allGeneData[geneSelection].chromosomeData[i] & filterWindow;
                    resultByte = (byte)(resultByte | selectedBasePair);
                }
                resultingChromosome[i] = resultByte;
            }
            return new SingleChromosomeCopy
            {
                chromosomeData = resultingChromosome
            };
        }

        public static Chromosome GetBaseGenes(ChromosomeEditor geneGenerators, System.Random random)
        {
            var geneData = new SingleChromosomeCopy[geneGenerators.chromosomeCopies];

            var geneticSize = geneGenerators.ChromosomeGeneticSize();
            for (int chromosomeCopy = 0; chromosomeCopy < geneData.Length; chromosomeCopy++)
            {
                var bytes = new byte[geneticSize.IndexToByteData + 1];
                random.NextBytes(bytes);
                geneData[chromosomeCopy] = new SingleChromosomeCopy
                {
                    chromosomeData = bytes
                };
            }

            return new Chromosome
            {
                allGeneData = geneData
            };
        }
    }

    [CreateAssetMenu(fileName = "Chromosome", menuName = "Genetics/Chromosome", order = 2)]
    public class ChromosomeEditor : ScriptableObject
    {
        public GeneEditor[] genes;
        public int chromosomeCopies = 2;

        public Chromosome GenerateChromosomeData(System.Random random)
        {
            return Chromosome.GetBaseGenes(this, random);
        }

        public GeneIndex ChromosomeGeneticSize()
        {
            var aggregateSpan = genes
                .Select(x => x.GeneUsage)
                .Aggregate((accumulate, next) => new GeneSpan(accumulate, next));
            if(aggregateSpan.start.allelePosition > 0)
            {
                Debug.LogWarning($"chromosome {this.name} does not use the 0th allele, leading to wasted space");
            }
            return aggregateSpan.end;
        }

        public void CompileChromosomeIntoDrivers(Chromosome chromosome, CompiledGeneticDrivers drivers)
        {
            var expecteByteSize = ChromosomeGeneticSize().IndexToByteData + 1;
            if (expecteByteSize != chromosome.allGeneData[0].chromosomeData.Length)
            {
                Debug.LogError($"genome does not match current genes! Genome data size: {expecteByteSize}, current gene size: {chromosome.allGeneData[0].chromosomeData.Length}. Resetting genome data");
            }
            for (int geneIndex = 0; geneIndex < genes.Length; geneIndex++)
            {
                genes[geneIndex].Evaluate(drivers, chromosome.allGeneData);
            }
        }
    }
}