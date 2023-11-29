#if UNITY_EDITOR
namespace UIWidgets.Examples.Shops
{
	using UIWidgets;
	    #if UNITY_EDITOR
	using UnityEditor;
    #endif
	/// <summary>
	/// TraderListView editor.
	/// </summary>
	[CanEditMultipleObjects]
	[CustomEditor(typeof(TraderListView), true)]
	public class TraderListViewEditor : ListViewCustomEditor
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TraderListViewEditor"/> class.
		/// </summary>
		public TraderListViewEditor()
		{
			Properties.Remove("customItems");
		}
	}
}
#endif