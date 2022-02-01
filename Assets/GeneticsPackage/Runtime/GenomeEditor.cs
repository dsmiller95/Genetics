using Genetics.GeneticDrivers;
using System.Linq;
using UnityEngine;

namespace Genetics
{
    [System.Serializable]
    public class Genome
    {
        public Chromosome[] allChromosomes;

        public static Genome GetBaseGenes(ChromosomeEditor[] chromosomeEditors, System.Random random)
        {
            var chroms = new Chromosome[chromosomeEditors.Length];
            for (int i = 0; i < chromosomeEditors.Length; i++)
            {
                chroms[i] = chromosomeEditors[i].GenerateChromosomeData(random);
            }
            return new Genome
            {
                allChromosomes = chroms
            };
        }

        public void EnforceInvarianceOverHomologousCopies()
        {
            foreach (var chromosome in allChromosomes)
            {
                for (int i = 1; i < chromosome.allGeneData.Length; i++)
                {
                    chromosome.allGeneData[0].chromosomeData.CopyTo(chromosome.allGeneData[i].chromosomeData, 0);
                }
            }
        }

        /// <summary>
        /// Select all genes randomly from the breeding genomes
        /// </summary>
        /// <param name="breedingGenome"></param>
        public Genome(params Genome[] breedingGenome)
        {
            if (breedingGenome.Length == 0)
            {
                return;
            }

            allChromosomes = new Chromosome[breedingGenome[0].allChromosomes.Length];
            if (breedingGenome.Any(x => x.allChromosomes.Length != allChromosomes.Length))
            {
                throw new System.ArgumentException("breeding genomes must have equal number of chromosomes");
            }
            for (int chromosomeIndex = 0; chromosomeIndex < allChromosomes.Length; chromosomeIndex++)
            {
                // generate a new chromosome, using all copies of this chromosome
                allChromosomes[chromosomeIndex] = new Chromosome(breedingGenome.Select(x => x.allChromosomes[chromosomeIndex]).ToArray());
            }
        }
    }

    [CreateAssetMenu(fileName = "Genome", menuName = "Genetics/Genome", order = 1)]
    public class GenomeEditor : ScriptableObject
    {
        public ChromosomeEditor[] chromosomes;

        public GeneEditor[] geneInterpretors = new GeneEditor[0];

        public Genome GenerateBaseGenomeData(System.Random random)
        {
            return Genome.GetBaseGenes(chromosomes, random);
        }

        /// <summary>
        /// compiles a genome into all relevant gentic drivers
        /// </summary>
        /// <param name="genomeData">the raw genome data</param>
        /// <returns>a set of drivers if fertile. if infertile, returns null</returns>
        public CompiledGeneticDrivers CompileGenome(Genome genomeData)
        {
            var drivers = new CompiledGeneticDrivers();

            if (genomeData.allChromosomes.Length != chromosomes.Length)
            {
                Debug.LogError($"Chromosome number mismatch! Chromosomes in data: {genomeData.allChromosomes.Length}, current chromosome count: {chromosomes.Length}.");
            }

            for (int chromosomeIndex = 0; chromosomeIndex < chromosomes.Length; chromosomeIndex++)
            {
                var chromosome = chromosomes[chromosomeIndex];
                var chromosomeData = genomeData.allChromosomes[chromosomeIndex];
                if (!chromosome.CompileChromosomeIntoDrivers(chromosomeData, drivers))
                {
                    return null;
                }
            }

            foreach (var interpretor in geneInterpretors)
            {
                if (!interpretor.Evaluate(drivers, new SingleChromosomeCopy[0]))
                {
                    return null;
                }
            }

            drivers.Lock();
            return drivers;
        }
    }
}