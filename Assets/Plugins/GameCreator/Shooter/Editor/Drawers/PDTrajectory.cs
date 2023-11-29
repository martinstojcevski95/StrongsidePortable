namespace GameCreator.Shooter
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
     #if UNITY_EDITOR 
 using UnityEditor;
 #endif 
    using GameCreator.Core;

    [CustomPropertyDrawer(typeof(TrajectoryRenderer.Trajectory))]
    public class PDTrajectory : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty spMaterial = property.FindPropertyRelative("material");
            SerializedProperty spTextureMode = property.FindPropertyRelative("textureMode");
            SerializedProperty spAlignement = property.FindPropertyRelative("alignement");
            SerializedProperty spWidth = property.FindPropertyRelative("width");
            SerializedProperty spMode = property.FindPropertyRelative("mode");
            SerializedProperty spOffset = property.FindPropertyRelative("offset");
            SerializedProperty spMinVelocity = property.FindPropertyRelative("minVelocity");
            SerializedProperty spMaxVelocity = property.FindPropertyRelative("maxVelocity");
            SerializedProperty spResolution = property.FindPropertyRelative("resolution");

            Rect rect = new Rect(
                position.x,
                position.y,
                position.width,
                EditorGUIUtility.singleLineHeight
            );

            EditorGUI.PropertyField(rect, spMode);
            EditorGUI.indentLevel++;

            rect.y += (EditorGUIUtility.standardVerticalSpacing + rect.height);
            EditorGUI.PropertyField(rect, spOffset);

            rect.y += (EditorGUIUtility.standardVerticalSpacing + rect.height);
            EditorGUI.PropertyField(rect, spMinVelocity);

            rect.y += (EditorGUIUtility.standardVerticalSpacing + rect.height);
            EditorGUI.PropertyField(rect, spMaxVelocity);

            if (spMode.enumValueIndex == (int)TrajectoryRenderer.Mode.Curve)
            {
                rect.y += (EditorGUIUtility.standardVerticalSpacing + rect.height);
                EditorGUI.PropertyField(rect, spResolution);
            }

            EditorGUI.indentLevel--;

            rect.y += (EditorGUIUtility.standardVerticalSpacing + rect.height);
            EditorGUI.PropertyField(rect, spMaterial);

            rect.y += (EditorGUIUtility.standardVerticalSpacing + rect.height);
            EditorGUI.PropertyField(rect, spWidth);

            rect.y += (EditorGUIUtility.standardVerticalSpacing + rect.height);
            EditorGUI.PropertyField(rect, spTextureMode);

            rect.y += (EditorGUIUtility.standardVerticalSpacing + rect.height);
            EditorGUI.PropertyField(rect, spAlignement);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty spMode = property.FindPropertyRelative("mode");
            float modeHeight = (spMode.enumValueIndex == (int)TrajectoryRenderer.Mode.Curve
                ? EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing
                : 0f
            );

            return (
                EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing +
                EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing +
                EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing +
                EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing +
                EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing +
                EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing +
                EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing +
                EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing +
                modeHeight
            );
        }
    }
}