﻿ #if UNITY_EDITOR 
 using UnityEditor;
 #endif 

namespace Pathfinding.Legacy {
	[CustomEditor(typeof(LegacyRVOController))]
	[CanEditMultipleObjects]
	public class LegacyRVOControllerEditor : Pathfinding.RVO.RVOControllerEditor {
		protected override void Inspector () {
			DrawDefaultInspector();
			LegacyEditorHelper.UpgradeDialog(targets, typeof(Pathfinding.RVO.RVOController));
		}
	}
}
