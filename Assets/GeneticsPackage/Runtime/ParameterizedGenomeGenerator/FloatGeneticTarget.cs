using Dman.ObjectSets;
using Genetics;
using Genetics.GeneticDrivers;
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
            return new FloatGeneticTarget
            {
                minValue = Mathf.Round(minValue * 10) / 10f,
                maxValue = Mathf.Round((minValue + range) * 10) / 10f,
                targetDriver = driver
            };
        }
    }

    /// <summary>
    /// binary serialization compatabile and unity inspector compatible
    /// </summary>
    [System.Serializable]
    public class FloatGeneticTarget : ISerializable, IGeneticTarget
    {
        public FloatGeneticDriver targetDriver;
        public float minValue;
        public float maxValue;

        public FloatGeneticTarget()
        {
        }
        public FloatGeneticTarget(FloatGeneticDriver driver, float min, float max)
        {
            this.targetDriver = driver;
            this.minValue = min;
            this.maxValue = max;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("minValue", minValue);
            info.AddValue("maxValue", maxValue);
            info.AddValue("driverReference", new IDableSavedReference(targetDriver));
        }


        // The special constructor is used to deserialize values.
        private FloatGeneticTarget(SerializationInfo info, StreamingContext context)
        {
            minValue = info.GetSingle("minValue");
            maxValue = info.GetSingle("maxValue");
            var savedReference = info.GetValue("driverReference", typeof(IDableSavedReference)) as IDableSavedReference;
            targetDriver = savedReference?.GetObject<GeneticDriver>() as FloatGeneticDriver;
            if (targetDriver == null)
            {
                throw new SerializationException($"Could not deserialize a value for ${nameof(targetDriver)}");
            }
        }

        public bool Matches(CompiledGeneticDrivers geneticDrivers)
        {
            if (!geneticDrivers.TryGetGeneticData(targetDriver, out var floatValue))
            {
                return false;
            }
            return targetDriver.FallsInRange(minValue, maxValue, floatValue);
        }

        public string GetDescriptionOfTarget()
        {
            return targetDriver.DescribeRange(minValue, maxValue);
        }
    }
}