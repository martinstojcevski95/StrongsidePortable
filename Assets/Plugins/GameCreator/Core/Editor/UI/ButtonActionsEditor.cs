namespace GameCreator.Core
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
     #if UNITY_EDITOR 
 using UnityEditor;
 using UnityEditor.UI;
 #endif 
    using UnityEngine.UI;
   

    [CustomEditor(typeof(ButtonActions))]
    public class ButtonActionsEditor : SelectableEditor
    {
        private ActionsEditor editorActions;

        protected override void OnEnable()
        {
            base.OnEnable();

            SerializedProperty spActions = serializedObject.FindProperty("actions");
            if (spActions.objectReferenceValue != null)
            {
                this.editorActions = Editor.CreateEditor(
                    spActions.objectReferenceValue
                ) as ActionsEditor;
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();
            if (this.editorActions != null)
            {
                this.editorActions.OnInspectorGUI();
            }
            serializedObject.ApplyModifiedProperties();
        }

        // CREATE: --------------------------------------------------------------------------------

        [MenuItem("GameObject/Game Creator/UI/Button", false, 30)]
        public static void CreateButtonActions()
        {
            GameObject canvas = CreateSceneObject.GetCanvasGameObject();
            GameObject buttonGO = DefaultControls.CreateButton(CreateSceneObject.GetStandardResources());
            buttonGO.transform.SetParent(canvas.transform, false);

            Button button = buttonGO.GetComponent<Button>();
            Graphic targetGraphic = button.targetGraphic;

            DestroyImmediate(button);
            ButtonActions buttonActions = buttonGO.AddComponent<ButtonActions>();
            buttonActions.targetGraphic = targetGraphic;
            Selection.activeGameObject = buttonGO;
        }
    }
}