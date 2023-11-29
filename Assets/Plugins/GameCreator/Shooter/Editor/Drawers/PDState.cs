namespace GameCreator.Shooter
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
     #if UNITY_EDITOR 
 using UnityEditor;
 #endif 
    using GameCreator.Core;

    [CustomPropertyDrawer(typeof(Weapon.State))]
    public class PDState : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty spState = property.FindPropertyRelative("state");
            SerializedProperty spMask = property.FindPropertyRelative("mask");

            SerializedProperty spUpBody = property.FindPropertyRelative("upperBodyRotation");
            SerializedProperty spLoBody = property.FindPropertyRelative("lowerBodyRotation");
            SerializedProperty spStability = property.FindPropertyRelative("stabilizeBody");

            Rect rect = new Rect(
                position.x,
                position.y,
                position.width,
                EditorGUIUtility.singleLineHeight
            );
            EditorGUI.PropertyField(rect, spState);

            rect.y += (EditorGUIUtility.standardVerticalSpacing + rect.height);
            EditorGUI.PropertyField(rect, spMask);

            rect.y += (EditorGUIUtility.standardVerticalSpacing + rect.height);
            EditorGUI.PropertyField(rect, spStability);

            rect.y += (EditorGUIUtility.standardVerticalSpacing + rect.height);
            rect.y += (EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(rect, spUpBody);

            rect.y += (EditorGUIUtility.standardVerticalSpacing + rect.height);
            EditorGUI.PropertyField(rect, spLoBody);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (
                EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing +
                EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing +
                EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing +
                EditorGUIUtility.singleLineHeight +
                EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing +
                EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing
            );
        }
    }
}