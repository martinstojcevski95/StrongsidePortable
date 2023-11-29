namespace GameCreator.Shooter
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	 #if UNITY_EDITOR 
 using UnityEditor;
 #endif 
    using GameCreator.Core;

    [CustomEditor(typeof(PlayerShooter))]
	public class PlayerShooterEditor : CharacterShooterEditor
	{
        protected override void PaintID()
        {
            return;
        }
    }
}