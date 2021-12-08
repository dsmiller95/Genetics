using Genetics.ParameterizedGenomeGenerator;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Genetics.Editor
{
    public class GeneticTargetPropertyDrawer : PropertyDrawer
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var binaryValue = property.FindPropertyRelative("binarySerializedValue").stringValue;

        }
    }
}
