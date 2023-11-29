namespace GameCreator.Shooter
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
    using GameCreator.Core;
    using GameCreator.Characters;
    using GameCreator.Variables;

	[AddComponentMenu("")]
	public class ActionWeaponChangeAmmo : IAction
	{
        public TargetGameObject shooter = new TargetGameObject(TargetGameObject.Target.Player);
        
        [Space] public Ammo ammo;

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            if (ammo == null) return true;

            CharacterShooter charShooter = this.shooter.GetComponent<CharacterShooter>(target);
            if (!charShooter)
            {
                Debug.LogError("Target Game Object does not have a CharacterShooter component");
                return true;
            }

            charShooter.ChangeAmmo(this.ammo);
            return true;
        }

		#if UNITY_EDITOR
        public static new string NAME = "Shooter/Change Weapon Ammo";
        private const string NODE_TITLE = "Change {0} Ammo to {1}";

        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Shooter/Extra/Icons/Actions/";

        public override string GetNodeTitle()
        {
            return string.Format(
                NODE_TITLE,
                this.shooter,
                this.ammo == null ? "(none)" : this.ammo.ammoID
            );
        }
        #endif
    }
}
