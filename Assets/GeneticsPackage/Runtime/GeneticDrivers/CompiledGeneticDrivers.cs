using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Genetics.GeneticDrivers
{
    [Serializable]
    public class CompiledGeneticDrivers
    {
        private Dictionary<string, object> geneticDriverValues = new Dictionary<string, object>();

        private bool writable = true;

        public CompiledGeneticDrivers()
        {
        }

        /// <summary>
        /// Lock the genetic driver set, indicating that it is fully compiled and cannot be changed
        /// </summary>
        /// <returns>true if it became locked, false if the drivers were already locked</returns>
        public bool Lock()
        {
            return writable && !(writable = false);
        }

        public bool HasGeneticDriver(GeneticDriver driver)
        {
            return geneticDriverValues.ContainsKey(driver.DriverName);
        }

        public bool TryGetGeneticDataAsString(GeneticDriver driver, out string driverValue)
        {
            if (geneticDriverValues.TryGetValue(driver.DriverName, out var objectValue))
            {
                driverValue = objectValue.ToString();
                return true;
            }
            driverValue = null;
            return false;
        }
        public T GetGeneticData<T>(GeneticDriver<T> driver)
        {
            if (geneticDriverValues.TryGetValue(driver.DriverName, out var objectValue) && objectValue is T typedValue)
            {
                return typedValue;
            }
            throw new Exception($"Genetic driver {driver} is not set, this is due to either a missing gene in the genome, or a gene ordering problem");
        }

        public bool TryGetGeneticData<T>(GeneticDriver<T> driver, out T driverValue)
        {
            if (geneticDriverValues.TryGetValue(driver.DriverName, out var objectValue) && objectValue is T typedValue)
            {
                driverValue = typedValue;
                return true;
            }
            driverValue = default;
            return false;
        }

        public void SetGeneticDriverData<T>(GeneticDriver<T> driver, T value, bool forceWrite = false)
        {
            if (!writable && !forceWrite)
            {
                return;
            }
            geneticDriverValues[driver.DriverName] = value;
        }

        public override string ToString()
        {
            var result = new System.Text.StringBuilder();
            foreach (var driver in geneticDriverValues)
            {
                result.AppendLine($"{driver.Key,20}: {driver.Value}");
            }
            return result.ToString();
        }

        #region Serialization

        [Serializable]
        private class DriverValue
        {
            public string Key;
            public object Value;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var seriaizedDrivers = geneticDriverValues.Select(x => new DriverValue
            {
                Key = x.Key,
                Value = x.Value
            }).ToArray();

            info.AddValue("driverValues", seriaizedDrivers);
        }


        // The special constructor is used to deserialize values.
        private CompiledGeneticDrivers(SerializationInfo info, StreamingContext context)
        {
            var seriaizedDrivers = (DriverValue[])info.GetValue("driverValues", typeof(DriverValue[]));
            this.geneticDriverValues = seriaizedDrivers.ToDictionary(x => x.Key, x => x.Value);
            this.writable = false;
        }

        #endregion
    }
}
