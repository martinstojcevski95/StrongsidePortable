﻿namespace GameCreator.Core
{
    using UnityEngine;
     #if UNITY_EDITOR 
 using UnityEditor;
 #endif 
    using System.Collections.Generic;

    [CustomPropertyDrawer(typeof(LayerSelectorAttribute))]
    public class LayerSelectorPD : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.intValue = EditorGUI.LayerField(position, label, property.intValue);
        }
    }
}