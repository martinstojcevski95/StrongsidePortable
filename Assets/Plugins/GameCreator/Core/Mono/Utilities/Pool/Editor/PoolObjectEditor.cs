namespace GameCreator.Pool
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
     #if UNITY_EDITOR 
 using UnityEditor;
 #endif 

    [CustomEditor(typeof(PoolObject))]
    public class PoolObjectEditor : Editor
    {
        private SerializedProperty spInitCount;
        private SerializedProperty spDuration;

        private void OnEnable()
        {
            this.spInitCount = this.serializedObject.FindProperty("initCount");
            this.spDuration = this.serializedObject.FindProperty("duration");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spInitCount);
            EditorGUILayout.PropertyField(this.spDuration);

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}