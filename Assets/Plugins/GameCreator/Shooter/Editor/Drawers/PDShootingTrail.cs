namespace GameCreator.Shooter
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
     #if UNITY_EDITOR 
 using UnityEditor;
 #endif 
    using GameCreator.Core;

    [CustomPropertyDrawer(typeof(ShootingTrailRenderer.ShootingTrail))]
    public class PDShootingTrail : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty spUseShootingTrail = property.FindPropertyRelative("useShootingTrail");
            SerializedProperty spDuration = property.FindPropertyRelative("duration");

            SerializedProperty spMaterial = property.FindPropertyRelative("material");
            SerializedProperty spTextureMode = property.FindPropertyRelative("textureMode");
            SerializedProperty spAlignement = property.FindPropertyRelative("alignement");
            SerializedProperty spWidth = property.FindPropertyRelative("width");

            Rect rect = new Rect(
                position.x,
                position.y,
                position.width,
                EditorGUIUtility.singleLineHeight
            );

            EditorGUI.PropertyField(rect, spUseShootingTrail);
            EditorGUI.BeginDisabledGroup(!spUseShootingTrail.boolValue);
            EditorGUI.indentLevel++;

            rect.y += (EditorGUIUtility.standardVerticalSpacing + rect.height);
            EditorGUI.PropertyField(rect, spDuration);

            rect.y += (EditorGUIUtility.standardVerticalSpacing + rect.height);
            EditorGUI.PropertyField(rect, spMaterial);

            rect.y += (EditorGUIUtility.standardVerticalSpacing + rect.height);
            EditorGUI.PropertyField(rect, spWidth);

            rect.y += (EditorGUIUtility.standardVerticalSpacing + rect.height);
            EditorGUI.PropertyField(rect, spTextureMode);

            rect.y += (EditorGUIUtility.standardVerticalSpacing + rect.height);
            EditorGUI.PropertyField(rect, spAlignement);

            EditorGUI.indentLevel--;
            EditorGUI.EndDisabledGroup();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (
                EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing +
                EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing +
                EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing +
                EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing +
                EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing +
                EditorGUIUtility.singleLineHeight
            );
        }
    }
}