namespace GameCreator.Core
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	#if UNITY_EDITOR 
 using UnityEditor;
 #endif 

    [CustomEditor(typeof(RememberActive))]
    public class RememberActiveEditor : RememberEditor
    {
        protected override string Comment()
        {
            return "Automatically restores the state (active, inactive or destroyed) when loading a game";
        }

        protected override void OnPaint()
        { }
    }
}