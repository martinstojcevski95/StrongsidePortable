namespace GameCreator.Shooter
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
    using GameCreator.Core;
    using GameCreator.Characters;

	[AddComponentMenu("")]
	public class ActionWeaponDraw : IAction
	{
        public TargetGameObject shooter = new TargetGameObject(TargetGameObject.Target.Player);

        public Weapon weapon;
        public Ammo ammo;

        public override IEnumerator Execute(GameObject target, IAction[] actions, int index)
        {
            CharacterShooter charShooter = this.shooter.GetComponent<CharacterShooter>(target);
            if (charShooter != null)
            {
                yield return charShooter.ChangeWeapon(this.weapon, this.ammo);
            }

            yield return 0;
        }

        #if UNITY_EDITOR

        public static new string NAME = "Shooter/Draw Weapon";
        private const string NODE_TITLE = "{0} draw {1} {2}";

        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Shooter/Extra/Icons/Actions/";

        public override string GetNodeTitle()
        {
            return string.Format(
                NODE_TITLE,
                this.shooter,
                this.weapon == null ? "(none)" : this.weapon.name,
                this.ammo == null ? "" : "with " + this.ammo.name
            );
        }

        #endif
    }
}
