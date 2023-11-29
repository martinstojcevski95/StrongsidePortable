namespace GameCreator.Shooter
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	 #if UNITY_EDITOR 
 using UnityEditor;
 #endif 
    using GameCreator.Core;

    [CustomEditor(typeof(CharacterShooter))]
	public class CharacterShooterEditor : Editor
	{
        // PROPERTIES: ---------------------------------------------------------

        protected CharacterShooter instance;

        public SerializedProperty spCurrentWeapon;
        public SerializedProperty spCurrentAmmo;

        // INITIALIZERS: -------------------------------------------------------

        protected void OnEnable()
        {
            this.instance = this.target as CharacterShooter;

            this.spCurrentWeapon = this.serializedObject.FindProperty("currentWeapon");
            this.spCurrentAmmo = this.serializedObject.FindProperty("currentAmmo");
        }

        // PAINT METHODS: ------------------------------------------------------

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(true);

            EditorGUILayout.PropertyField(this.spCurrentWeapon);
            EditorGUILayout.PropertyField(this.spCurrentAmmo);

            EditorGUI.EndDisabledGroup();

            this.PaintID();

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void PaintID()
        {
            EditorGUILayout.Space();
            GlobalEditorID.Paint(this.instance);
        }
    }
}