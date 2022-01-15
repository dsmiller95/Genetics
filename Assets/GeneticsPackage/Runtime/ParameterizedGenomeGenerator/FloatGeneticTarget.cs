using Dman.ObjectSets;
using Genetics;
using Genetics.GeneticDrivers;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Genetics.ParameterizedGenomeGenerator
{

    /// <summary>
    /// binary serialization compatabile and unity inspector compatible
    /// </summary>
    [System.Serializable]
    public class FloatGeneticTargetGenerator
    {
        public FloatGeneticDriver driver;
        [Header("Extremes which the range will be confined inside")]
        public float absoluteMin;
        public float absoluteMax;
        [Header("Range for the width of the driver valid range")]
        public float rangeMin;
        public float rangeMax;
        public FloatGeneticTarget GenerateTarget(System.Random randProvider = null)
        {
            if(randProvider == null)
            {
                randProvider = new System.Random(UnityEngine.Random.Range(1, int.MaxValue));
            }
            var range = (float)(randProvider.NextDouble() * (rangeMax - rangeMin) + rangeMin);
            var minValue = (float)(randProvider.NextDouble() * (absoluteMax - absoluteMin - range) + absoluteMin);
            return new FloatGeneticTarget(driver, Mathf.Round(minValue * 10) / 10f, Mathf.Round((minValue + range) * 10) / 10f);
        }
    }

    /// <summary>
    /// binary serialization compatabile and unity inspector compatible
    /// </summary>
    [System.Serializable]
    public struct TargetRange
    {
        public float minValue;
        public float maxValue;

        public TargetRange(float min, float max)
        {
            minValue = min;
            maxValue = max;
        }
    }

    /// <summary>
    /// binary serialization compatabile and unity inspector compatible
    /// </summary>
    [System.Serializable]
    public class FloatGeneticTarget : ISerializable, IGeneticTarget
    {
        public FloatGeneticDriver targetDriver;
        public List<TargetRange> targetRanges;
        public GeneticDriver TargetDriver => targetDriver;

        public FloatGeneticTarget()
        {
        }
        public FloatGeneticTarget(FloatGeneticDriver driver, float min, float max)
        {
            this.targetDriver = driver;
            targetRanges = new List<TargetRange> { new TargetRange(min, max) };
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("targetRanges", targetRanges);
            info.AddValue("driverReference", new IDableSavedReference(targetDriver));
        }


        // The special constructor is used to deserialize values.
        private FloatGeneticTarget(SerializationInfo info, StreamingContext context)
        {
            targetRanges = info.GetValue("targetRanges", typeof(List<TargetRange>)) as List<TargetRange>;
            var savedReference = info.GetValue("driverReference", typeof(IDableSavedReference)) as IDableSavedReference;
            targetDriver = savedReference?.GetObject<GeneticDriver>() as FloatGeneticDriver;
            if (targetDriver == null)
            {
                throw new SerializationException($"Could not deserialize a value for ${nameof(targetDriver)}");
            }
        }

        /// <summary>
        /// merge the other target into this one, such that this target will now match based on the criteria it used to have
        ///     and the criteria of the other target
        /// </summary>
        /// <param name="otherTarget"></param>
        public void MergeIn(FloatGeneticTarget otherTarget)
        {
            //TODO: optimize
            targetRanges.AddRange(otherTarget.targetRanges);
        }

        public bool Matches(CompiledGeneticDrivers geneticDrivers)
        {
            if (!geneticDrivers.TryGetGeneticData(targetDriver, out var floatValue))
            {
                return false;
            }
            foreach (var range in targetRanges)
            {
                if(targetDriver.FallsInRange(range.minValue, range.maxValue, floatValue))
                {
                    return true;
                }
            }
            return false;
        }

        public string GetDescriptionOfTarget()
        {
            var description = new System.Text.StringBuilder();
            foreach (var range in targetRanges)
            {
                description.Append(targetDriver.DescribeRange(range.minValue, range.maxValue));
                description.Append(", ");
            }
            return description.ToString();
        }
    }
}