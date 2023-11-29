namespace GameCreator.Localization
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	 #if UNITY_EDITOR 
 using UnityEditor;
 #endif 
	using GameCreator.Core;

    [CustomPropertyDrawer(typeof(LocStringNoPostProcessAttribute))]
    public class LocStringNoPostProcessPropertyDrawer : LocStringPropertyDrawer
	{
        protected override bool PaintPostProcess()
        {
            return false;
        }
	}
}
