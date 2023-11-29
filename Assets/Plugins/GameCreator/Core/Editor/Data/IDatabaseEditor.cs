namespace GameCreator.Core
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
		 #if UNITY_EDITOR 
 using UnityEditor;
 #endif 

	public abstract class IDatabaseEditor : Editor 
	{
		// ABSTRACT METHODS: ----------------------------------------------------------------------

		public abstract string GetName();

		// VIRTUAL METHODS: -----------------------------------------------------------------------

		public virtual void OnPreferencesWindowGUI() { this.OnInspectorGUI(); }
		public virtual string GetDocumentationURL() {return "http://docs.gamecreator.io";}
		public virtual int GetPanelWeight() { return 50; }
        public virtual bool CanBeDecoupled() { return false; }
	}
}