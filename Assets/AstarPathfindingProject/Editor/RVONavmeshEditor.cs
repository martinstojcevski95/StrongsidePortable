#if UNITY_EDITOR 
 using UnityEditor;
 #endif 
using Pathfinding.RVO;

namespace Pathfinding {
	[CustomEditor(typeof(RVONavmesh))]
	public class RVONavmeshEditor : Editor {
		public override void OnInspectorGUI () {
			DrawDefaultInspector();
		}
	}
}
