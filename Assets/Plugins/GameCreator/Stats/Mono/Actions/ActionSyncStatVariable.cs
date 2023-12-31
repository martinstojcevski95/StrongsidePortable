﻿namespace GameCreator.Stats
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using GameCreator.Core;
    using GameCreator.Variables;

	#if UNITY_EDITOR
	using UnityEditor;
	#endif

	[AddComponentMenu("")]
    public class ActionSyncStatVariable : IAction
	{
        public TargetGameObject target = new TargetGameObject(TargetGameObject.Target.Player);

        [StatSelector]
        public StatAsset stat;

        [VariableFilter(Variable.DataType.Number)]
        public VariableProperty variable = new VariableProperty(Variable.VarType.GlobalVariable);

        // EXECUTABLE: ----------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            GameObject targetGO = this.target.GetGameObject(target);
            if (targetGO == null)
            {
                Debug.LogError("Action Stat Sync: No target defined");
                return true;
            }

            Stats stats = targetGO.GetComponentInChildren<Stats>();
            if (stats == null)
            {
                Debug.LogError("Action Stat Sync: Could not get Stats component in target");
                return true;
            }

            this.variable.Set(stats.GetStat(this.stat.stat.uniqueName), target);
            return true;
        }

        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

        #if UNITY_EDITOR

        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Stats/Icons/Actions/";

        public static new string NAME = "Stats/Sync Stat To Variable";
        private const string NODE_TITLE = "Sync {0} to {1}";

		// PROPERTIES: ----------------------------------------------------------------------------

		private SerializedProperty spTarget;
        private SerializedProperty spStat;
        private SerializedProperty spVariable;

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle()
		{
            return string.Format(
                NODE_TITLE, 
                (this.stat == null ? "(none)" : this.stat.stat.uniqueName),
                this.variable.ToString()
            );
		}

		protected override void OnEnableEditorChild ()
		{
            this.spTarget = this.serializedObject.FindProperty("target");
            this.spStat = this.serializedObject.FindProperty("stat");
            this.spVariable = this.serializedObject.FindProperty("variable");
		}

		protected override void OnDisableEditorChild ()
		{
            this.spTarget = null;
            this.spStat = null;
            this.spVariable = null;
		}

		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spTarget);
            EditorGUILayout.PropertyField(this.spStat);
            EditorGUILayout.PropertyField(this.spVariable, true);

			this.serializedObject.ApplyModifiedProperties();
		}

		#endif
	}
}
