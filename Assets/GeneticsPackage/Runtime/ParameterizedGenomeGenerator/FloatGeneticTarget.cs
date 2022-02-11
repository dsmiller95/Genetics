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
    public class FloatGeneticTarget : ISerializable, IGeneticTarget, ISerializationCallbackReceiver
    {
        public FloatGeneticDriver targetDriver;
        /// <summary>
        /// used for serialization in unity
        /// </summary>
        [SerializeField]
        private List<FloatRange> targetRanges;

        [SerializeField]
        [HideInInspector]
        private int serializedVersion = 0;

        public BrokenFloatRange BrokenRangeRepresentation
        {
            get;
            private set;
        }
        public GeneticDriver TargetDriver => targetDriver;

        private FloatGeneticTarget()
        {
        }
        public FloatGeneticTarget(FloatGeneticDriver driver, float min, float max)
        {
            this.targetDriver = driver;
            BrokenRangeRepresentation = new BrokenFloatRange(min, max, driver.CompareRangeAsIntegers());
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("targetRanges", BrokenRangeRepresentation);
            info.AddValue("driverReference", new IDableSavedReference(targetDriver));
        }


        // The special constructor is used to deserialize values.
        private FloatGeneticTarget(SerializationInfo info, StreamingContext context)
        {
            BrokenRangeRepresentation = info.GetValue("targetRanges", typeof(BrokenFloatRange)) as BrokenFloatRange;
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
            foreach (var range in BrokenRangeRepresentation.GetRepresentativeRange())
            {
                description.Append(targetDriver.DescribeRange(range.minValue, range.maxValue));
                description.Append(", ");
            }
            return description.ToString();
        }

        public void Exclude(FloatGeneticTarget other)
        {
            BrokenRangeRepresentation.Exclude(other.BrokenRangeRepresentation);
        }

        public void MergeIn(FloatGeneticTarget other)
        {
            BrokenRangeRepresentation.MergeIn(other.BrokenRangeRepresentation);
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
                BrokenRangeRepresentation = BrokenRangeRepresentation.Clone()
            };
        }

        public void OnBeforeSerialize()
        {
            if (BrokenRangeRepresentation != null)
            {
                targetRanges = BrokenRangeRepresentation.GetRepresentativeRange().ToList();
                serializedVersion = 1;
            }
        }

        public void OnAfterDeserialize()
        {
            BrokenRangeRepresentation = new BrokenFloatRange(targetRanges, targetDriver?.CompareRangeAsIntegers() ?? true); // default to true, avoids deleting the default range
        }
    }
}