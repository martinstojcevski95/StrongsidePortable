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
	public class ActionWeaponReload : IAction
	{
        public TargetGameObject shooter = new TargetGameObject(TargetGameObject.Target.Player);
        [Indent] public bool waitTillComplete;

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            if (this.waitTillComplete) return false;

            CharacterShooter charShooter = this.shooter.GetComponent<CharacterShooter>(target);
            if (charShooter)
            {
                CoroutinesManager.Instance.StartCoroutine(charShooter.Reload());
            }

            return true;
        }

        public override IEnumerator Execute(GameObject target, IAction[] actions, int index)
        {
            CharacterShooter charShooter = this.shooter.GetComponent<CharacterShooter>(target);
            if (charShooter != null) yield return charShooter.Reload();

            yield return 0;
        }

        #if UNITY_EDITOR
        public static new string NAME = "Shooter/Reload Weapon";
        private const string NODE_TITLE = "Reload {0} weapon {1}";

        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Shooter/Extra/Icons/Actions/";

        public override string GetNodeTitle()
        {
            return string.Format(
                NODE_TITLE,
                this.shooter,
                (this.waitTillComplete ? "(wait)" : "")
            );
        }

        #endif
    }
}
