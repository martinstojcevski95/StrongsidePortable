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
    public class ActionNotifyShot : IAction
    {
        public enum TargetType
        {
            AreaOfEffect,
            Target
        }

        [Tooltip("Requires Character Shooter component")]
        public TargetGameObject shooter = new TargetGameObject(TargetGameObject.Target.Invoker);

        public TargetType targetType = TargetType.Target;

        public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.GameObject);

        public TargetPosition position = new TargetPosition(TargetPosition.Target.Invoker);
        public NumberProperty radius = new NumberProperty(5f);

        public CharacterShooter.ShotType shotType = CharacterShooter.ShotType.Normal;

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            CharacterShooter shooterValue = this.shooter.GetComponent<CharacterShooter>(target);
            if (shooterValue == null) return true;

            switch (this.targetType)
            {
                case TargetType.Target:
                    IgniterOnReceiveShot[] ignitersTarget = this.target.GetComponentsInChildren<IgniterOnReceiveShot>(target);
                    for (int i = 0; i < ignitersTarget.Length; ++i)
                    {
                        if (ignitersTarget[i] != null)
                        {
                            ignitersTarget[i].OnReceiveShot(
                                shooterValue, 
                                this.shotType,
                                ignitersTarget[i].transform.position
                            );
                        }
                    }
                    break;

                case TargetType.AreaOfEffect:
                    Vector3 pos = this.position.GetPosition(target);
                    float rad = this.radius.GetValue(target);
                    QueryTriggerInteraction query = QueryTriggerInteraction.Collide;
                    Collider[] colliders = Physics.OverlapSphere(pos, rad, -1, query);

                    for (int i = 0; i < colliders.Length; ++i)
                    {
                        IgniterOnReceiveShot[] colliderIgniters = colliders[i]
                            .gameObject.GetComponentsInChildren<IgniterOnReceiveShot>();

                        for (int j = 0; j < colliderIgniters.Length; ++j)
                        {
                            if (colliderIgniters[j] != null)
                            {
                                colliderIgniters[j].OnReceiveShot(
                                    shooterValue, 
                                    this.shotType,
                                    colliderIgniters[j].transform.position
                                );
                            }
                        }
                    }

                    break;

            }

            return true;
        }

        #if UNITY_EDITOR

        public static new string NAME = "Shooter/Notify Shot";
        private const string NODE_TITLE = "Notify shot to {0}";

        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Shooter/Extra/Icons/Actions/";

        private SerializedProperty spShooter;
        private SerializedProperty spTargetType;
        private SerializedProperty spTarget;
        private SerializedProperty spPosition;
        private SerializedProperty spRadius;
        private SerializedProperty spShotType;

        public override string GetNodeTitle()
        {
            return string.Format(
                NODE_TITLE,
                ObjectNames.NicifyVariableName(this.targetType.ToString())
            );
        }

        protected override void OnEnableEditorChild()
        {
            this.spShooter = this.serializedObject.FindProperty("shooter");
            this.spTargetType = this.serializedObject.FindProperty("targetType");
            this.spTarget = this.serializedObject.FindProperty("target");
            this.spPosition = this.serializedObject.FindProperty("position");
            this.spRadius = this.serializedObject.FindProperty("radius");
            this.spShotType = this.serializedObject.FindProperty("shotType");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spShooter);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(this.spTargetType);
            switch ((TargetType)this.spTargetType.enumValueIndex)
            {
                case TargetType.Target:
                    EditorGUILayout.PropertyField(this.spTarget);
                    break;

                case TargetType.AreaOfEffect:
                    EditorGUILayout.PropertyField(this.spPosition);
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(this.spRadius);
                    break;
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spShotType);

            this.serializedObject.ApplyModifiedProperties();
        }

#endif
    }
}
