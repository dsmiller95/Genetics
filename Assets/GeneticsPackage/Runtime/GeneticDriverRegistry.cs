using Dman.ObjectSets;
using UnityEngine;

namespace Genetics
{
    [CreateAssetMenu(fileName = "GeneticDriverRegistry", menuName = "Genetics/GeneticDriverRegistry", order = 20)]
    public class GeneticDriverRegistry : UniqueObjectRegistryWithAccess<GeneticDriver>
    {
    }
}