namespace GameCreator.Shooter
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	    #if UNITY_EDITOR
	using UnityEditor;
    #endif
	using GameCreator.Core;

    [CustomEditor(typeof(Weapon))]
	public class WeaponEditor : IShooterEditor
    {
        private static readonly GUIContent GC_NAME = new GUIContent("Name");
        private static readonly GUIContent GC_DESC = new GUIContent("Description");

        private const string ERR_MISSING_AMMO = "A weapon requires a default Ammo asset";
        private const string ERR_MISSING_MODEL = "A 3D model is required with a Muzzle Component";

        private const float SPACING = 2f;

        // PROPERTIES: ---------------------------------------------------------

        private Weapon weapon;
        private SerializedProperty spName;
        private SerializedProperty spDesc;

        private Section sectionGeneral;
        private Section sectionStateEase;
        private Section sectionStateAim;
        private Section sectionModel;

        private SerializedProperty spDefaultAmmo;
        private SerializedProperty spPitchOffset;
        private SerializedProperty spAudioDraw;
        private SerializedProperty spAudioHolster;

        private SerializedProperty spEase;
        private SerializedProperty spAiming;

        private SerializedProperty spPrefab;
        private SerializedProperty spAttachment;
        private SerializedProperty spPrefabPosition;
        private SerializedProperty spPrefabRotation;

        // INITIALIZERS: -------------------------------------------------------

        private void OnEnable()
        {
            this.weapon = this.target as Weapon;
            if (this.weapon == null) return;

            this.spName = this.serializedObject.FindProperty("weaponName");
            this.spDesc = this.serializedObject.FindProperty("weaponDesc");

            this.sectionGeneral = new Section("General", this.LoadIcon("General"), this.Repaint);
            this.sectionStateEase = new Section("State Ease", this.LoadIcon("State-Ease"), this.Repaint);
            this.sectionStateAim = new Section("State Aiming", this.LoadIcon("State-Aiming"), this.Repaint);
            this.sectionModel = new Section("3D Model", this.LoadIcon("3D-Model"), this.Repaint);

            this.spDefaultAmmo = this.serializedObject.FindProperty("defaultAmmo");
            this.spAudioDraw = this.serializedObject.FindProperty("audioDraw");
            this.spAudioHolster = this.serializedObject.FindProperty("audioHolster");

            this.spEase = this.serializedObject.FindProperty("ease");
            this.spAiming = this.serializedObject.FindProperty("aiming");
            this.spPitchOffset = this.serializedObject.FindProperty("pitchOffset");

            this.spPrefab = this.serializedObject.FindProperty("prefab");
            this.spAttachment = this.serializedObject.FindProperty("attachment");
            this.spPrefabPosition = this.serializedObject.FindProperty("prefabPosition");
            this.spPrefabRotation = this.serializedObject.FindProperty("prefabRotation");
        }

        // PAINT METHODS: ------------------------------------------------------

        public override void OnInspectorGUI()
        {
            if (this.weapon == null || this.serializedObject == null) return;
            this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spName, GC_NAME);
            EditorGUILayout.PropertyField(this.spDesc, GC_DESC);

            EditorGUILayout.Space();
            this.PaintSectionGeneral();

            GUILayout.Space(SPACING);
            this.PaintSectionStateResting();

            GUILayout.Space(SPACING);
            this.PaintSectionStateAiming();

            GUILayout.Space(SPACING);
            this.PaintSectionModel();

            GUILayout.Space(SPACING);
            this.PaintSectionWarnings();

            this.serializedObject.ApplyModifiedProperties();
        }

        private void PaintSectionGeneral()
        {
            this.sectionGeneral.PaintSection();
            using (var group = new EditorGUILayout.FadeGroupScope(this.sectionGeneral.state.faded))
            {
                if (group.visible)
                {
                    EditorGUILayout.BeginVertical(CoreGUIStyles.GetBoxExpanded());

                    EditorGUILayout.PropertyField(this.spDefaultAmmo);
                    EditorGUILayout.PropertyField(this.spAudioDraw);
                    EditorGUILayout.PropertyField(this.spAudioHolster);

                    EditorGUILayout.EndVertical();
                }
            }
        }

        private void PaintSectionStateResting()
        {
            this.sectionStateEase.PaintSection();
            using (var group = new EditorGUILayout.FadeGroupScope(this.sectionStateEase.state.faded))
            {
                if (group.visible)
                {
                    EditorGUILayout.BeginVertical(CoreGUIStyles.GetBoxExpanded());

                    EditorGUILayout.PropertyField(this.spEase, true);

                    EditorGUILayout.EndVertical();
                }
            }
        }

        private void PaintSectionStateAiming()
        {
            this.sectionStateAim.PaintSection();
            using (var group = new EditorGUILayout.FadeGroupScope(this.sectionStateAim.state.faded))
            {
                if (group.visible)
                {
                    EditorGUILayout.BeginVertical(CoreGUIStyles.GetBoxExpanded());

                    EditorGUILayout.PropertyField(this.spPitchOffset);
                    EditorGUILayout.PropertyField(this.spAiming, true);

                    EditorGUILayout.EndVertical();
                }
            }
        }

        private void PaintSectionModel()
        {
            this.sectionModel.PaintSection();
            using (var group = new EditorGUILayout.FadeGroupScope(this.sectionModel.state.faded))
            {
                if (group.visible)
                {
                    EditorGUILayout.BeginVertical(CoreGUIStyles.GetBoxExpanded());

                    EditorGUILayout.PropertyField(this.spPrefab);
                    EditorGUILayout.PropertyField(this.spAttachment);

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(this.spPrefabPosition);
                    EditorGUILayout.PropertyField(this.spPrefabRotation);

                    EditorGUILayout.EndVertical();
                }
            }
        }

        private void PaintSectionWarnings()
        {
            if (this.spDefaultAmmo.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox(ERR_MISSING_AMMO, MessageType.Error);
            }

            if (this.spPrefab.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox(ERR_MISSING_MODEL, MessageType.Error);
            }
        }
    }
}
 