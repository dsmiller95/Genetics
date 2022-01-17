using Dman.ObjectSets;
using Genetics;
using Genetics.GeneticDrivers;
using System.Runtime.Serialization;
using UnityEngine;

namespace Genetics.ParameterizedGenomeGenerator
{

    /// <summary>
    /// simple target which simply makes sure that the provided driver has some value
    /// </summary>
    public class FertileGeneticTarget : IGeneticTarget
    {
        public GeneticDriver targetDriver;
        public GeneticDriver TargetDriver => targetDriver;

        public FertileGeneticTarget(GeneticDriver driver)
        {
            this.targetDriver = driver;
        }

        public bool Matches(CompiledGeneticDrivers geneticDrivers)
        {
            if (!geneticDrivers.HasGeneticDriver(targetDriver))
            {
                return false;
            }
            return true;
        }

        public string GetDescriptionOfTarget()
        {
            return targetDriver.DriverName;
        }

        public IGeneticTarget Clone()
        {
            return new FertileGeneticTarget(targetDriver);
        }
    }

    public class NodeEqualityComparitorByGene : System.Collections.Generic.IEqualityComparer<GeneticDriverDependencyTree.GeneticDriverNode>
    {
        public bool Equals(GeneticDriverDependencyTree.GeneticDriverNode x, GeneticDriverDependencyTree.GeneticDriverNode y)
        {
            return x.sourceEditor.Equals(y.sourceEditor);
        }

        public int GetHashCode(GeneticDriverDependencyTree.GeneticDriverNode obj)
        {
            return obj.sourceEditor.GetHashCode();
        }
    }
}