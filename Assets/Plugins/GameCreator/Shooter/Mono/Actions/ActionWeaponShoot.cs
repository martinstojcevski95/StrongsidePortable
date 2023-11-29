namespace GameCreator.Shooter
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
    using GameCreator.Core;
    using GameCreator.Characters;
    using GameCreator.Variables;

    #if UNITY_EDITOR
    using UnityEditor;
    #endif

    [AddComponentMenu("")]
	public class ActionWeaponShoot : IAction
	{
        public enum Mode
        {
            Single,
            Burst
        }

        public TargetGameObject shooter = new TargetGameObject(TargetGameObject.Target.Player);

        [Space] public Mode mode = Mode.Single;
        public NumberProperty burstAmount = new NumberProperty(5f);

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            if (this.mode == Mode.Burst) return false;

            CharacterShooter charShooter = this.shooter.GetComponent<CharacterShooter>(target);
            if (charShooter == null)
            {
                Debug.LogError("Target Game Object does not have a CharacterShooter component");
                return true;
            }

            charShooter.Shoot();
            return true;
        }

        public override IEnumerator Execute(GameObject target, IAction[] actions, int index)
        {
            CharacterShooter charShooter = this.shooter.GetComponent<CharacterShooter>(target);
            if (charShooter == null || charShooter.currentWeapon == null || charShooter.currentAmmo == null)
            {
                Debug.LogError("Target Game Object does not have a CharacterShooter component");
                yield break;
            }

            int burstCount = Mathf.Min(
                charShooter.GetAmmoInClip(charShooter.currentAmmo.ammoID),
                this.burstAmount.GetInt(target)
            );

            if (burstCount <= 0) burstCount = 1;

            float fireRate = charShooter.currentAmmo.fireRate.GetValue(target);
            for (int i = 0; i < burstCount; ++i)
            {
                while (!charShooter.CanShootFireRate(charShooter.currentAmmo.ammoID, fireRate))
                {
                    yield return null;
                }

                charShooter.Shoot();
            }

            yield return 0;
        }

        #if UNITY_EDITOR

        public static new string NAME = "Shooter/Weapon Shoot";
        private const string NODE_TITLE = "Character {0} {1} shoot";

        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Shooter/Extra/Icons/Actions/";

        private SerializedProperty spShooter;
        private SerializedProperty spMode;
        private SerializedProperty spAmount;

        public override string GetNodeTitle()
        {
            return string.Format(
                NODE_TITLE,
                this.shooter,
                this.mode.ToString()
            );
        }

        protected override void OnEnableEditorChild()
        {
            base.OnEnableEditorChild();
            this.spShooter = this.serializedObject.FindProperty("shooter");
            this.spMode = this.serializedObject.FindProperty("mode");
            this.spAmount = this.serializedObject.FindProperty("burstAmount");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spShooter);
            EditorGUILayout.PropertyField(this.spMode);

            if (this.spMode.enumValueIndex == (int) Mode.Burst)
            {
                EditorGUILayout.PropertyField(this.spAmount);
            }

            this.serializedObject.ApplyModifiedProperties();
        }

        #endif
    }
}