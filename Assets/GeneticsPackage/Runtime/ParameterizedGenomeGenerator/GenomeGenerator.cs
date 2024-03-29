﻿using Genetics.GeneticDrivers;
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
        public GenomeTargetContainer aggregateTargets;
        public GenomeEditor genomeTarget;
        [UnityEngine.Tooltip("When true, chromosome copies will have different genes. when false, all chromosome copies will be identical")]
        public bool varianceOverHomologous = true;

        public GenomeGenerator()
        {

        }

        public GenomeGenerator(GenomeTargetContainer target, bool varianceOverHomologous = true)
        {
            this.aggregateTargets = target;
            this.genomeTarget = target.targetGenome;
            this.varianceOverHomologous = varianceOverHomologous;
        }

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
            var fertilityChecksInOrder = depTree.GetGeneticDriversSortedLeafFirst()
                .Where(x => !x.sourceEditor.AlwaysValid)
                .Distinct(new NodeEqualityComparitorByGene())
                .Select(x => new FertileGeneticTarget(x.driver))
                .ToList();

            while (true)
            {
                var nextGenome = genomeTarget.GenerateBaseGenomeData(randomSource);
                if (!varianceOverHomologous)
                {
                    nextGenome.EnforceInvarianceOverHomologousCopies();
                }

                // continue rerolling until no rerolls have to be made, meaning the entire genome matches
                while (CompileGenesConditionallyRestrictedByTargets(nextGenome, depTree, fertilityChecksInOrder, random))
                {
                    if (processingSinceLastSpacer++ >= nullProcessingSpacer)
                    {
                        yield return null;
                        processingSinceLastSpacer = 0;
                    }
                }
                yield return nextGenome;
            }
        }

        /// <summary>
        /// attempt to get the genome to match by rerolling each target indivudally, in some order
        /// </summary>
        /// <param name="genomeData"></param>
        /// <param name="depTree">structure holding the layout of the genetic drivers</param>
        /// <param name="randomSource"></param>
        /// <returns>true if rerolls were made to attempt to match. false if no rerolls happened, that is, the genome already matches every target and no changes were made</returns>
        private bool CompileGenesConditionallyRestrictedByTargets(
            Genome genomeData,
            GeneticDriverDependencyTree depTree,
            List<FertileGeneticTarget> fertitlityChecks,
            Random randomSource)
        {
            UnityEngine.Profiling.Profiler.BeginSample("rerolling and rechecking");
            var changeMade = false;
            foreach (var target in aggregateTargets.booleanTargets)
            {
                changeMade |= RerollTargetToMatch(genomeData, depTree, target, randomSource);
            }
            foreach (var target in aggregateTargets.floatTargets)
            {
                changeMade |= RerollTargetToMatch(genomeData, depTree, target, randomSource);
            }
            foreach (var target in fertitlityChecks)
            {
                changeMade |= RerollTargetToMatch(genomeData, depTree, target, randomSource);
            }
            UnityEngine.Profiling.Profiler.EndSample();
            return changeMade;
        }

        /// <summary>
        /// reroll the genetic data supporting the specific target until it matches
        /// </summary>
        /// <param name="genomeData">the genome data to reroll over</param>
        /// <param name="depTree">the dependency tree representing all the genes</param>
        /// <param name="target">the genetic target to match</param>
        /// <returns>true if any rerolls were perfomed. false otherwise, in which case the genome already matched the target</returns>
        private bool RerollTargetToMatch(Genome genomeData, GeneticDriverDependencyTree depTree, IGeneticTarget target, Random randomSource)
        {
            var node = depTree.GetNodeFromDriver(target.TargetDriver);
            var relevantSpan = node.GetBasisSpan();
            if (relevantSpan == GeneSpan.INVALID || relevantSpan.Length <= 0)
            {
                throw new ArgumentException($"genetic target {target.GetDescriptionOfTarget()} has no genetic base in this genome");
            }
            var rerollBuffer = new byte[relevantSpan.GetByteLength()];
            var relevantChromosome = genomeData.allChromosomes[node.sourceEditorChromosome];

            var infiniteProtecc = 5000;

            var rerolled = false;
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
#if UNITY_EDITOR
                // this could theoretically cause very unlikely crashes if included in a prod build. all genetic targets should be 
                //  verified as plausable before publishing the game
                if (infiniteProtecc-- <= 0)
                {
                    throw new Exception($"infinite loop protection. could not find valid driver match after 5000 rerolls on gene:{node.sourceEditor.name}");
                }
#endif
                rerolled = true;
            }
            return rerolled;
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
