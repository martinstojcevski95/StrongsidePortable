namespace GameCreator.Core
{
    using UnityEngine;
    	 #if UNITY_EDITOR 
 using UnityEditor;
 #endif 
    using System.Collections.Generic;

    [CustomPropertyDrawer(typeof(IndentAttribute))]
    public class IndentPD : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.indentLevel--;
        }
    }
}