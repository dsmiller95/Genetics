using Genetics.GeneticDrivers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Genetics.ParameterizedGenomeGenerator
{
    /// <summary>
    /// Want to define certain parameters within which the genetic drivers must fall.
    /// Naive implementation will be randomly generating values, and filtering out those which
    ///     do not match filter parameters
    /// binary serialization compatabile and unity inspector compatible
    /// </summary>
    [System.Serializable]
    public class GenomeGenerator
    {
        public BooleanGeneticTarget[] booleanTargets = new BooleanGeneticTarget[0];
        public FloatGeneticTarget[] floatTargets = new FloatGeneticTarget[0];
        public GenomeEditor genomeTarget;
        [UnityEngine.Tooltip("When true, chromosome copies will have different genes. when false, all chromosome copies will be identical")]
        public bool varianceOverHomologous = true;

        /// <summary>
        /// Generate genomes infinitely. Will retry randomly generating genomes, and only return those which match the
        ///     target definitions. Will only run the generation algorythm <paramref name="nullProcessingSpacer"/> times
        ///     before yielding a null. In this way, null values can be used to space out processing over multiple frames
        /// </summary>
        /// <param name="randomSource"></param>
        /// <param name="nullProcessingSpacer"></param>
        /// <returns>An infinite series of genomes which match the target params, and nulls to space out results based on processing time</returns>
        public IEnumerable<Genome> GenerateGenomes(
            Random randomSource,
            int nullProcessingSpacer)
        {
            var processingSinceLastSpacer = 0;

            var depTree = new GeneticDriverDependencyTree(genomeTarget);
            var random = new Random(UnityEngine.Random.Range(1, int.MaxValue));

            while (true)
            {
                var nextGenome = genomeTarget.GenerateBaseGenomeData(randomSource);
                if (!varianceOverHomologous)
                {
                    nextGenome.EnforceInvarianceOverHomologousCopies();
                }

                var targetsMatch = CompileGenesConditionallyRestrictedByTargets(nextGenome, depTree, random);
                if (targetsMatch)
                {
                    UnityEngine.Profiling.Profiler.BeginSample("test matching genome");
                    // make sure the full genome is valid
                    targetsMatch = genomeTarget.CompileGenome(nextGenome) != null;
                    UnityEngine.Profiling.Profiler.EndSample();
                }
                processingSinceLastSpacer++;

                if (targetsMatch)
                {
                    yield return nextGenome;
                }
                if (processingSinceLastSpacer >= nullProcessingSpacer)
                {
                    yield return null;
                    processingSinceLastSpacer = 0;
                }
            }
        }

        private bool CompileGenesConditionallyRestrictedByTargets(Genome genomeData, GeneticDriverDependencyTree depTree, Random randomSource)
        {
            foreach (var target in booleanTargets)
            {
                RerollTargetToMatch(genomeData, depTree, target, randomSource);
            }
            foreach (var target in floatTargets)
            {
                RerollTargetToMatch(genomeData, depTree, target, randomSource);
            }
            // TODO: ensure all targets still match after the rerolls, in case of overlaps
            return true;
        }

        /// <summary>
        /// reroll the genetic data supporting the specific target until it matches
        /// </summary>
        /// <param name="genomeData">the genome data to reroll over</param>
        /// <param name="depTree">the dependency tree representing all the genes</param>
        /// <param name="target">the genetic target to match</param>
        /// <returns></returns>
        private void RerollTargetToMatch(Genome genomeData, GeneticDriverDependencyTree depTree, IGeneticTarget target, Random randomSource)
        {
            var node = depTree.GetNodeFromDriver(target.TargetDriver);
            var relevantSpan = node.GetBasisSpan();
            var rerollBuffer = new byte[relevantSpan.GetByteLength()];
            var relevantChromosome = genomeData.allChromosomes[node.sourceEditorChromosome];

            var infiniteProtecc = 1000;

            while (!GenomeMatchesAndIsValid(genomeData, depTree, target))
            {
                if (!varianceOverHomologous)
                {
                    randomSource.NextBytes(rerollBuffer);
                }
                foreach (var chromosome in relevantChromosome.allGeneData)
                {
                    if (varianceOverHomologous)
                    {
                        randomSource.NextBytes(rerollBuffer);
                    }
                    chromosome.WriteIntoGeneSpan(relevantSpan, rerollBuffer);
                }
                if(infiniteProtecc-- <= 0)
                {
                    throw new Exception("infinite loop protection. could not find valid genome match");
                }
            }
        }

        private bool GenomeMatchesAndIsValid(Genome genomeData, GeneticDriverDependencyTree depTree, IGeneticTarget target)
        {
            var drivers = new CompiledGeneticDrivers();
            var node = depTree.GetNodeFromDriver(target.TargetDriver);
            if (!RecursivelyEvaluate(genomeData, drivers, node))
            {
                return false;
            }
            if (!target.Matches(drivers))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="genomeData"></param>
        /// <param name="drivers"></param>
        /// <param name="currentNode"></param>
        /// <returns>whether the zygote is still viable or not</returns>
        private bool RecursivelyEvaluate(Genome genomeData, CompiledGeneticDrivers drivers, GeneticDriverDependencyTree.GeneticDriverNode currentNode)
        {
            if (drivers.HasGeneticDriver(currentNode.driver))
            {
                return true;
            }
            foreach (var input in currentNode.inputs)
            {
                RecursivelyEvaluate(genomeData, drivers, input);
            }
            return currentNode.TriggerEvaluate(genomeData, drivers);
        }
    }
}
