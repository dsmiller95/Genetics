﻿using Genetics;
using Genetics.Genes;
using Genetics.GeneticDrivers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Genetics.Genes
{
    /// <summary>
    /// Will output a single whole number value, based on the result of combining multiple boolean genes together. For ever True gene,
    ///     the output will increment by one. Producing a binomial distribution of output values when input genes are randomized
    /// </summary>
    [CreateAssetMenu(fileName = "MultiBooleanGeneAccumulationSelector", menuName = "Genetics/Genes/MultiBooleanGeneAccumulationSelector", order = 2)]
    public class MultiBooleanGeneAccumulationSelector : GeneEditor
    {
        [Tooltip("will populate with a whole number value anywhere from 0 to (booleanInputs.length)")]
        public FloatGeneticDriver floatOutput;
        public BooleanGeneticDriver[] booleanInputs;

        public override int GeneSize => 0;

        public override void Evaluate(CompiledGeneticDrivers editorHandle, GeneCopies[] data)
        {
            if (editorHandle.TryGetGeneticData(floatOutput, out var _))
            {
                Debug.LogWarning($"Overwriting already set genetic driver {floatOutput} in gene {this}.");
            }
            var outputInt = booleanInputs.Select(x =>
            {
                if (!editorHandle.TryGetGeneticData(x, out var boolVal))
                {
                    Debug.LogWarning($"Input genetic driver {x} is not set, this is due to either a missing gene in the genome, or a gene ordering problem");
                }
                return boolVal ? 1 : 0;
            }).Sum();
            editorHandle.SetGeneticDriverData(floatOutput, outputInt);
        }

        public override IEnumerable<GeneticDriver> GetInputs()
        {
            return booleanInputs;
        }

        public override IEnumerable<GeneticDriver> GetOutputs()
        {
            yield return floatOutput;
        }

        public override SingleGene[] GenerateGeneData(System.Random random)
        {
            return new SingleGene[0];
        }
    }
}