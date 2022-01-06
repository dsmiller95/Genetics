using Genetics.GeneticDrivers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Genetics.ParameterizedGenomeGenerator
{
    [CreateAssetMenu(fileName = "GenomeTargetContainer", menuName = "Genetics/GenomeTargetContainer", order = 0)]
    public class GenomeTargetContainer : ScriptableObject
    {
        public GenomeEditor targetGenome;

        public List<BooleanGeneticTarget> booleanTargets = new List<BooleanGeneticTarget>();
        public List<FloatGeneticTarget> floatTargets = new List<FloatGeneticTarget>();

        private Dictionary<GeneticDriver, IGeneticTarget> _targetsByDriver;
        private Dictionary<GeneticDriver, IGeneticTarget> targetsByDriver
        {
            get
            {
                if (_targetsByDriver == null)
                {
                    _targetsByDriver = targetGenome.chromosomes.SelectMany(x => x.genes)
                        .Concat(targetGenome.geneInterpretors)
                        .SelectMany(x => x.GetInputs().Concat(x.GetOutputs()))
                        .Distinct()
                        .ToDictionary(x => x, x => null as IGeneticTarget);
                    foreach (var target in booleanTargets.Cast<IGeneticTarget>().Concat(floatTargets))
                    {
                        _targetsByDriver[target.TargetDriver] = target;
                    }
                }
                return _targetsByDriver;
            }
        }

        public IEnumerable<IGeneticTarget> AllTargets => booleanTargets.Cast<IGeneticTarget>().Concat(floatTargets);

        public bool DriversMatch(CompiledGeneticDrivers drivers)
        {
            foreach (var target in booleanTargets.Cast<IGeneticTarget>().Concat(floatTargets))
            {
                if (!target.Matches(drivers))
                {
                    return false;
                }
            }
            return true;
        }

        public void Reset()
        {
            booleanTargets.Clear();
            floatTargets.Clear();
            _targetsByDriver = null;
        }

        /// <summary>
        /// "include" the range of genomes described in the given genetic target. this means that this container will now
        ///     allow for the production of genomes with this target, in addition to all other targets defined already
        /// </summary>
        /// <param name="target"></param>
        public void IncludeTarget(IGeneticTarget target)
        {
            if (target is BooleanGeneticTarget boolTarget)
            {
                this.IncludeTarget(boolTarget);
            }
            else if (target is FloatGeneticTarget floatTarget)
            {
                this.IncludeTarget(floatTarget);
            }
            else
            {
                Debug.LogWarning("cannot include genetic target of type '" + target.GetType() + "'");
            }
        }

        private void IncludeTarget(BooleanGeneticTarget target)
        {
            var existing = targetsByDriver[target.TargetDriver] as BooleanGeneticTarget;
            if(existing == null)
            {
                // there are no restrictions on this driver, no need to add more
                return;
            }
            if (existing.targetValue != target.targetValue)
            {
                // remove the target completely, because both true and false are now allowed
                booleanTargets.Remove(existing);
                targetsByDriver.Remove(target.targetDriver);
            }
        }
        private void IncludeTarget(FloatGeneticTarget target)
        {
            var existing = targetsByDriver[target.TargetDriver] as FloatGeneticTarget;
            if (existing == null)
            {
                // there are no restrictions on this driver, no need to add more
                return;
            }
            existing.MergeIn(target);
        }
    }
}
