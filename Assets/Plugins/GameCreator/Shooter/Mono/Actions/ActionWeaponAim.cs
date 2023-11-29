namespace GameCreator.Shooter
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
    using GameCreator.Core;
    using GameCreator.Variables;
    using GameCreator.Characters;

    #if UNITY_EDITOR
    using UnityEditor;
    #endif

    [AddComponentMenu("")]
	public class ActionWeaponAim : IAction
	{
        public enum AimDirection
        {
            StopAiming,
            AimCameraDirection,
            AimAtTarget,
            AimAtPosition,
            AimMuzzleForward,
            AimTopDownPlane,
            AimTopDownFloor,
            AimSideScroll
        }

        public TargetGameObject shooter = new TargetGameObject(TargetGameObject.Target.Player);

        public AimDirection aim = AimDirection.StopAiming;

        public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.GameObject);
        public AimingSideScrollPlane.Coordinates axis = AimingSideScrollPlane.Coordinates.XY;

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            CharacterShooter charShooter = this.shooter.GetComponent<CharacterShooter>(target);
            if (charShooter == null)
            {
                Debug.LogError("Target Game Object does not have a CharacterShooter component");
                return true;
            }

            Transform targetTransform = this.target.GetTransform(target);
            TargetPosition targetPosition = new TargetPosition(TargetPosition.Target.Transform);
            targetPosition.targetTransform = targetTransform;

            switch (this.aim)
            {
                case AimDirection.StopAiming:
                    charShooter.StopAiming();
                    break;

                case AimDirection.AimCameraDirection:
                    charShooter.StartAiming(new AimingCameraDirection(charShooter));
                    break;

                case AimDirection.AimAtTarget:
                    AimingAtTarget aimingTarget = new AimingAtTarget(charShooter);
                    aimingTarget.Setup(this.target);
                    charShooter.StartAiming(aimingTarget);
                    break;

                case AimDirection.AimAtPosition:
                    AimingAtPosition aimingPosition = new AimingAtPosition(charShooter);
                    aimingPosition.Setup(this.target);
                    charShooter.StartAiming(aimingPosition);
                    break;

                case AimDirection.AimMuzzleForward:
                    AimingMuzzleForward aimingForward = new AimingMuzzleForward(charShooter);
                    charShooter.StartAiming(aimingForward);
                    break;

                case AimDirection.AimTopDownPlane:
                    AimingGroundPlane aimingGround = new AimingGroundPlane(charShooter);
                    charShooter.StartAiming(aimingGround);
                    break;

                case AimDirection.AimTopDownFloor:
                    AimingGroundFloor aimingFloor = new AimingGroundFloor(charShooter);
                    charShooter.StartAiming(aimingFloor);
                    break;

                case AimDirection.AimSideScroll:
                    AimingSideScrollPlane aimingSidescroll = new AimingSideScrollPlane(charShooter);
                    aimingSidescroll.Setup(this.axis);
                    charShooter.StartAiming(aimingSidescroll);
                    break;
            }

            return true;
        }

		#if UNITY_EDITOR
        public static new string NAME = "Shooter/Weapon Aim";
        private const string NODE_TITLE = "{0} {1}";

        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Shooter/Extra/Icons/Actions/";

        private SerializedProperty spShooter;

        private SerializedProperty spAim;

        private SerializedProperty spTarget;
        private SerializedProperty spAxis;

        public override string GetNodeTitle()
        {
            return string.Format(
                NODE_TITLE,
                this.shooter,
                ObjectNames.NicifyVariableName(this.aim.ToString())
            );
        }

        protected override void OnEnableEditorChild()
        {
            this.spShooter = this.serializedObject.FindProperty("shooter");
            this.spAim = this.serializedObject.FindProperty("aim");

            this.spTarget = this.serializedObject.FindProperty("target");
            this.spAxis = this.serializedObject.FindProperty("axis");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spShooter);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(this.spAim);

            switch ((AimDirection)this.spAim.enumValueIndex)
            {
                case AimDirection.AimAtTarget:
                case AimDirection.AimAtPosition:
                    EditorGUILayout.PropertyField(this.spTarget);
                    break;

                case AimDirection.AimSideScroll:
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(this.spAxis);
                    EditorGUI.indentLevel--;
                    break;
            }

            this.serializedObject.ApplyModifiedProperties();
        }

        #endif
    }
}
