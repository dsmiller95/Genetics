﻿using Dman.Utilities;
using Genetics.GeneticDrivers;
using System.Linq;
using UnityEngine;

namespace Genetics
{
    [System.Serializable]
    [System.Diagnostics.DebuggerDisplay("{ToString(),nq}")]
    public class SingleChromosomeCopy
    {
        public static char[] AlleleCharacters = new[] { 'A', 'T', 'C', 'G' };

        public byte[] chromosomeData;
        public GeneIndex endIndex;

        /// <summary>
        /// create a new chromosome copy
        /// </summary>
        /// <param name="chromosomeData">the raw chromosome data</param>
        /// <param name="endIndex">the index after the last valid gene index. used to indicate the true size of this chromosome, and potentially
        ///     ignore a partial byte at the end</param>
        public SingleChromosomeCopy(byte[] chromosomeData, GeneIndex endIndex)
        {
            this.chromosomeData = chromosomeData;
            this.endIndex = endIndex;
        }
        public SingleChromosomeCopy(SingleChromosomeCopy[] sourceChromosomes, System.Random random)
        {
            var chromosomeCopies = sourceChromosomes.Length;
            this.chromosomeData = new byte[sourceChromosomes[0].chromosomeData.Length];
            this.endIndex = sourceChromosomes[0].endIndex;
            for (int i = 0; i < sourceChromosomes.Length; i++)
            {
                if (sourceChromosomes[i].endIndex != this.endIndex)
                {
                    throw new System.Exception("invalid chromosome set to select from. all chromosomes must be of uniform size");
                }
            }
            for (int i = 0; i < sourceChromosomes[0].chromosomeData.Length; i++)
            {
                byte resultByte = 0;
                for (int filterWindow = 0b11; filterWindow < byte.MaxValue; filterWindow = filterWindow << 2)
                {
                    var geneSelection = random.Next(0, chromosomeCopies);
                    int selectedBasePair = sourceChromosomes[geneSelection].chromosomeData[i] & filterWindow;
                    resultByte = (byte)(resultByte | selectedBasePair);
                }
                chromosomeData[i] = resultByte;
            }
        }

        public ulong SampleBasePairs(GeneSpan span)
        {
            if (span.Length > 32)
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
            var shift = (3 - index.IndexInsideByte) * 2;
            var mask = 0b11 << shift;
            return (byte)((chromosomeData[byteIndex] & mask) >> shift);
        }
        public void SetBasePairAtIndex(GeneIndex index, byte newBasePair)
        {
            var byteIndex = index.IndexToByteData;
            var shift = (3 - index.IndexInsideByte) * 2;

            var mask = 0b11 << shift;
            chromosomeData[byteIndex] = (byte)((chromosomeData[byteIndex] & ~mask) | ((newBasePair << shift) & mask));
        }

        public void WriteIntoGeneSpan(GeneSpan span, byte[] buffer)
        {
            var targetStartIndex = span.start.IndexToByteData;
            var sourceStartIndex = 0;

            if (span.start.IndexToByteData == span.end.IndexToByteData)
            {
                var startCopyMask = (0b11111111 >> (span.start.IndexInsideByte * 2));
                var endCopyMask = (0b11111111 << ((4 - span.end.IndexInsideByte) * 2));
                var compositeCopyMask = startCopyMask & endCopyMask;
                chromosomeData[targetStartIndex] = CopyMasked(chromosomeData[targetStartIndex], buffer[sourceStartIndex], compositeCopyMask);
                return;
            }

            // copy prefix sub-byte chunks
            if (span.start.IndexInsideByte != 0)
            {
                var copyMask = (0b11111111 >> (span.start.IndexInsideByte * 2));
                chromosomeData[targetStartIndex] = CopyMasked(chromosomeData[targetStartIndex], buffer[sourceStartIndex], copyMask);
                targetStartIndex++;
                sourceStartIndex++;
            }


            // copy suffix sub-byte chunks
            var targetEndIndex = (span.end - 1).IndexToByteData;
            var sourceEndIndex = buffer.Length - 1;
            var copyLength = targetEndIndex - targetStartIndex + 1;
            if (copyLength <= 0)
            {
                return;
            }
            if (span.end.IndexInsideByte != 0)
            {
                var copyMask = (0b11111111 << ((4 - span.end.IndexInsideByte) * 2));
                chromosomeData[targetEndIndex] = CopyMasked(chromosomeData[targetEndIndex], buffer[sourceEndIndex], copyMask);
                targetEndIndex--;
                sourceEndIndex--;
            }

            copyLength = targetEndIndex - targetStartIndex + 1;
            if (copyLength <= 0)
            {
                return;
            }
            if (copyLength > 0)
            {
                System.Array.Copy(buffer, sourceStartIndex, chromosomeData, targetStartIndex, targetEndIndex - targetStartIndex + 1);
            }
        }

        /// <summary>
        /// copies bits from <paramref name="sourceData"/> into <paramref name="targetData"/> where <paramref name="copyMask"/> is 1
        /// </summary>
        /// <param name="targetData"></param>
        /// <param name="sourceData"></param>
        /// <param name="copyMask"></param>
        /// <returns></returns>
        private byte CopyMasked(byte targetData, byte sourceData, int copyMask)
        {
            return (byte)((targetData & ~copyMask) | (sourceData & copyMask));
        }

        public override string ToString()
        {
            var chars = new char[endIndex.allelePosition];
            for (var geneIndex = new GeneIndex(0); geneIndex < endIndex; geneIndex++)
            {
                chars[geneIndex.allelePosition] = AlleleCharacters[SampleIndex(geneIndex)];
            }
            return new string(chars);
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
            var randomSource = new System.Random(Random.Range(1, int.MaxValue));
            return new SingleChromosomeCopy(allGeneData, randomSource);
        }

        public static Chromosome GetBaseGenes(ChromosomeEditor geneGenerators, System.Random random)
        {
            var geneData = new SingleChromosomeCopy[geneGenerators.chromosomeCopies];

            var geneticSize = geneGenerators.ChromosomeGeneticSize();
            var byteSize = geneGenerators.BytesRequiredForChromosome();
            for (int chromosomeCopy = 0; chromosomeCopy < geneData.Length; chromosomeCopy++)
            {
                var bytes = new byte[byteSize];
                random.NextBytes(bytes);
                geneData[chromosomeCopy] = new SingleChromosomeCopy(bytes, geneticSize);
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
            if (aggregateSpan.start.allelePosition > 0)
            {
                Debug.LogWarning($"chromosome {this.name} does not use the 0th allele, leading to wasted space");
            }
            return aggregateSpan.end;
        }
        public int BytesRequiredForChromosome()
        {
            return (ChromosomeGeneticSize() - 1).IndexToByteData + 1;
        }

        /// <summary>
        /// compile chromosome data into genetic drivers
        /// </summary>
        /// <param name="chromosome"></param>
        /// <param name="drivers"></param>
        /// <returns>true if the zygote is viable, false if fertalization fails</returns>
        public bool CompileChromosomeIntoDrivers(Chromosome chromosome, CompiledGeneticDrivers drivers)
        {
            var expecteByteSize = BytesRequiredForChromosome();
            if (expecteByteSize != chromosome.allGeneData[0].chromosomeData.Length)
            {
                Debug.LogError($"genome does not match current genes! Genome data size: {expecteByteSize}, current gene size: {chromosome.allGeneData[0].chromosomeData.Length}. Resetting genome data");
            }
            for (int geneIndex = 0; geneIndex < genes.Length; geneIndex++)
            {
                if (!genes[geneIndex].Evaluate(drivers, chromosome.allGeneData))
                {
                    return false;
                }
            }
            return true;
        }
    }
}