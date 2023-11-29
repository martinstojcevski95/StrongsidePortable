namespace GameCreator.Shooter
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using GameCreator.Core;

	[AddComponentMenu("")]
	public class ConditionClipIsFull : ICondition
	{
        public enum State
        {
            IsFull,
            IsEmpty
        }

		public TargetGameObject character = new TargetGameObject();
		public State state = State.IsFull;

		public override bool Check(GameObject target)
		{
			CharacterShooter shooter = this.character.GetComponent<CharacterShooter>(target);
			if (shooter == null) return false;
			if (shooter.currentAmmo == null) return false;

			string ammoID = shooter.currentAmmo.ammoID;

			int capacity = shooter.currentAmmo.clipSize;
			int inClip = shooter.GetAmmoInClip(ammoID);

            switch (this.state)
            {
				case State.IsFull: return inClip >= capacity;
				case State.IsEmpty: return inClip <= 0;
			}

			return false;
		}
        
		#if UNITY_EDITOR

        public static new string NAME = "Shooter/Current Weapon Clip";
		private const string NODE_TITLE = "Current {0} weapon {1}";

        public override string GetNodeTitle()
        {
			return string.Format(NODE_TITLE, this.character, this.state);
        }

        #endif
    }
}
