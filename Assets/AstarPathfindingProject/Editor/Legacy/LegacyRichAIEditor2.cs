 #if UNITY_EDITOR 
 using UnityEditor;
 #endif 

namespace Pathfinding.Legacy {
	[CustomEditor(typeof(LegacyRichAI))]
	[CanEditMultipleObjects]
	public class LegacyRichAIEditor : BaseAIEditor {
		protected override void Inspector () {
			base.Inspector();
			LegacyEditorHelper.UpgradeDialog(targets, typeof(RichAI));
		}
	}
}
