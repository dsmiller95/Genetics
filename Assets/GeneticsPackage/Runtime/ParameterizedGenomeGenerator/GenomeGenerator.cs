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
            return this.GenerateGenomes(
                nullProcessingSpacer,
                () => genomeTarget.GenerateBaseGenomeData(randomSource),
                genome => genome);
        }

        /// <summary>
        /// this base implementation uses callbacks to allow other gene carriers to be used as the base unit,
        ///     such as Seeds or any other object which may wrap a genome
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nullProcessingSpacer"></param>
        /// <param name="generateGeneCarrier"></param>
        /// <param name="selectGenomeFromCarrier"></param>
        /// <returns></returns>
        public IEnumerable<T> GenerateGenomes<T>(
            int nullProcessingSpacer,
            Func<T> generateGeneCarrier,
            Func<T, Genome> selectGenomeFromCarrier)
            where T : class
        {
            var processingSinceLastSpacer = 0;

            var depTree = new GeneticDriverDependencyTree(genomeTarget);

            while (true)
            {
                var nextGeneCarrier = generateGeneCarrier();
                var nextGenome = selectGenomeFromCarrier(nextGeneCarrier);
                if (!varianceOverHomologous)
                {
                    nextGenome.EnforceInvarianceOverHomologousCopies();
                }

                var targetsMatch = CompileGenesConditionallyRestrictedByTargets(nextGenome, depTree);
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
                    yield return nextGeneCarrier;
                }
                if (processingSinceLastSpacer >= nullProcessingSpacer)
                {
                    yield return null;
                    processingSinceLastSpacer = 0;
                }
            }
        }

        private bool CompileGenesConditionallyRestrictedByTargets(Genome genomeData, GeneticDriverDependencyTree depTree)
        {
            var drivers = new CompiledGeneticDrivers();

            foreach (var target in booleanTargets)
            {
                if (!CheckTarget(genomeData, depTree, drivers, target))
                {
                    return false;
                }
            }
            foreach (var target in floatTargets)
            {
                if (!CheckTarget(genomeData, depTree, drivers, target))
                {
                    return false;
                }
            }
            return true;
        }

        private bool CheckTarget(Genome genomeData, GeneticDriverDependencyTree depTree, CompiledGeneticDrivers drivers, IGeneticTarget target)
        {
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
