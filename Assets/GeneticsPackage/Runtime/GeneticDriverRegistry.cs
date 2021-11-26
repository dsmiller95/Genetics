using Dman.ObjectSets;
using System.Collections.Generic;
using UnityEngine;

namespace Genetics
{
    [CreateAssetMenu(fileName = "GeneticDriverRegistry", menuName = "Genetics/GeneticDriverRegistry", order = 20)]
    public class GeneticDriverRegistry : UniqueObjectRegistryWithAccess<GeneticDriver>
    {
        public override void OnObjectSetChanged()
        {
            base.OnObjectSetChanged();
            var allNames = new Dictionary<string, string>();
            foreach (var driver in this.allObjects)
            {
                if (allNames.ContainsKey(driver.DriverName))
                {
                    var otherUser = allNames[driver.DriverName];
                    Debug.LogError($"Found a genetic driver with a duplicate name. '{driver.name}' has a name of '{driver.DriverName}', which is already in use by '{otherUser}'");
                }else
                {
                    allNames[driver.DriverName] = driver.name;
                }
            }
        }
    }
}