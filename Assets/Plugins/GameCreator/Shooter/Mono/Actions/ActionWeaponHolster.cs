namespace GameCreator.Shooter
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
    using GameCreator.Core;
    using GameCreator.Characters;

	[AddComponentMenu("")]
	public class ActionWeaponHolster : IAction
	{
        public TargetGameObject shooter = new TargetGameObject(TargetGameObject.Target.Player);

        public override IEnumerator Execute(GameObject target, IAction[] actions, int index)
        {
            CharacterShooter charShooter = this.shooter.GetComponent<CharacterShooter>(target);
            if (charShooter != null)
            {
                yield return charShooter.ChangeWeapon(null);
            }
            
            yield return 0;
        }

        #if UNITY_EDITOR

        public static new string NAME = "Shooter/Holster Weapon";
        private const string NODE_TITLE = "{0} holster weapon";

        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Shooter/Extra/Icons/Actions/";

        public override string GetNodeTitle()
        {
            return string.Format(
                NODE_TITLE,
                this.shooter
            );
        }

        #endif
    }
}
