using Genetics.GeneticDrivers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Genetics.ParameterizedGenomeGenerator
{
    public class GeneticDriverDependencyTree
    {
        public class GeneticDriverNode : IEquatable<GeneticDriverNode>
        {
            public GeneticDriver driver;

            public int sourceEditorChromosome;
            public GeneEditor sourceEditor;

            /// <summary>
            /// a list of other nodes whose drivers are used to derive the value of
            ///     this node's driver
            /// </summary>
            public List<GeneticDriverNode> inputs;

            public GeneticDriverNode(GeneticDriver driver)
            {
                this.driver = driver;
                this.inputs = new List<GeneticDriverNode>();
            }

            public bool TriggerEvaluate(Genome genome, CompiledGeneticDrivers drivers)
            {
                if (sourceEditorChromosome == -1)
                {
                    return sourceEditor.Evaluate(drivers, new SingleChromosomeCopy[0]);
                }
                else
                {
                    return sourceEditor.Evaluate(drivers, genome.allChromosomes[sourceEditorChromosome].allGeneData);
                }
            }

            public GeneSpan GetBasisSpan()
            {
                var sourceSpan = sourceEditor.GeneUsage;
                foreach (var input in inputs)
                {
                    sourceSpan += input.GetBasisSpan();
                }
                return sourceSpan;
            }

            public override bool Equals(object obj)
            {
                if (obj is GeneticDriverNode node)
                {
                    return this.Equals(node);
                }
                return false;
            }

            public override int GetHashCode()
            {
                return driver.GetInstanceID();
            }

            public bool Equals(GeneticDriverNode other)
            {
                return other.driver == this.driver;
            }
        }

        /// <summary>
        /// a dictionary keyed by Unity's Instance ID of the genetic drivers
        ///     immutable after object creation
        /// </summary>
        private Dictionary<int, GeneticDriverNode> AllNodes;

        public GeneticDriverDependencyTree(ChromosomeEditor partialGenes, int chromosomeIndex)
        {
            this.AllNodes = new Dictionary<int, GeneticDriverNode>();

            foreach (var geneEditor in partialGenes.genes)
            {
                this.IntegrateGeneEditor(geneEditor, chromosomeIndex);
            }
        }
        public GeneticDriverDependencyTree(GenomeEditor AllGenes)
        {
            this.AllNodes = new Dictionary<int, GeneticDriverNode>();

            for (int i = 0; i < AllGenes.chromosomes.Length; i++)
            {
                var chromosome = AllGenes.chromosomes[i];
                foreach (var geneEditor in chromosome.genes)
                {
                    this.IntegrateGeneEditor(geneEditor, i);
                }
            }

            foreach (var geneEditor in AllGenes.geneInterpretors)
            {
                this.IntegrateGeneEditor(geneEditor, -1);
            }
        }

        private void IntegrateGeneEditor(GeneEditor geneEditor, int sourceChromosomeIndex)
        {
            var inputNodes = GetOrCreateNodes(geneEditor.GetInputs()).ToList();
            var outputNodes = GetOrCreateNodes(geneEditor.GetOutputs());
            foreach (var output in outputNodes)
            {
                if (output.sourceEditor != null)
                {
                    Debug.LogWarning($"multiple outputs defined pointing to one genetic driver: {output.sourceEditor} and {geneEditor} both output to {output}");
                }
                output.sourceEditor = geneEditor;
                output.sourceEditorChromosome = sourceChromosomeIndex;
                foreach (var input in inputNodes)
                {
                    output.inputs.Add(input);
                }
            }
        }

        public GeneticDriverNode GetNodeFromDriver(GeneticDriver driver)
        {
            return AllNodes[driver.GetInstanceID()];
        }

        public List<GeneticDriverNode> GetGeneticDriversSortedLeafFirst()
        {
            var roots = new HashSet<GeneticDriverNode>(AllNodes.Values);
            foreach (var node in AllNodes.Values)
            {
                foreach (var dependent in node.inputs)
                {
                    roots.Remove(dependent);
                }
            }

            var result = new List<GeneticDriverNode>();
            var iterationStack = new Stack<GeneticDriverNode>(roots);
            while (iterationStack.Count > 0)
            {
                var nextNode = iterationStack.Pop();
                result.Add(nextNode);
                foreach (var dependent in nextNode.inputs)
                {
                    iterationStack.Push(dependent);
                }
            }
            result.Reverse();
            return result;
        }

        private IEnumerable<GeneticDriverNode> GetOrCreateNodes(IEnumerable<GeneticDriver> drivers)
        {
            foreach (var driver in drivers)
            {
                if (!AllNodes.TryGetValue(driver.GetInstanceID(), out var node))
                {
                    node = new GeneticDriverNode(driver);
                    AllNodes[driver.GetInstanceID()] = node;
                }
                yield return node;
            }
        }
    }

}
