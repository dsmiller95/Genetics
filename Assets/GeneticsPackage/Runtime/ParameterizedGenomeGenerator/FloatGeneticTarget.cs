using Dman.ObjectSets;
using Genetics.GeneticDrivers;
using System.Collections.Generic;
using System.Linq;
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
            if (randProvider == null)
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
    public class FloatGeneticTarget : ISerializable, IGeneticTarget
    {
        public FloatGeneticDriver targetDriver;
        /// <summary>
        /// used for serialization in unity
        /// </summary>
        [SerializeField]
        public List<FloatRange> targetRanges;

        private BrokenFloatRange BrokenRangeRepresentation
        {
            get => new BrokenFloatRange(targetRanges, targetDriver.CompareRangeAsIntegers());
            set
            {
                targetRanges = value.GetRepresentativeRange().ToList();
            }
        }
        public GeneticDriver TargetDriver => targetDriver;

        private FloatGeneticTarget()
        {
        }
        public FloatGeneticTarget(FloatGeneticDriver driver, float min, float max)
        {
            this.targetDriver = driver;
            targetRanges = new List<FloatRange> { new FloatRange(min, max) };
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("targetRanges", targetRanges);
            info.AddValue("driverReference", new IDableSavedReference(targetDriver));
        }


        // The special constructor is used to deserialize values.
        private FloatGeneticTarget(SerializationInfo info, StreamingContext context)
        {
            targetRanges = info.GetValue("targetRanges", typeof(List<FloatRange>)) as List<FloatRange>;
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
            return BrokenRangeRepresentation.Matches(floatValue);
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

        public void Exclude(FloatGeneticTarget other)
        {
            var myRange = BrokenRangeRepresentation;
            myRange.Exclude(other.BrokenRangeRepresentation);
            BrokenRangeRepresentation = myRange;
        }

        public void MergeIn(FloatGeneticTarget other)
        {
            var myRange = BrokenRangeRepresentation;
            myRange.MergeIn(other.BrokenRangeRepresentation);
            BrokenRangeRepresentation = myRange;
        }

        public FloatGeneticTarget Invert()
        {
            return new FloatGeneticTarget
            {
                targetDriver = targetDriver,
                BrokenRangeRepresentation = BrokenRangeRepresentation.Invert()
            };
        }
        public IGeneticTarget Clone()
        {
            return new FloatGeneticTarget
            {
                targetDriver = targetDriver,
                targetRanges = targetRanges.ToList()
            };
        }
    }
}