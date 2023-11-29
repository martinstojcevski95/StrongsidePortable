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
	public class ActionAmmo : IAction
	{
        public enum Operation
        {
            AddToStorage,
            SetStorage,
            AddToMagazine,
            SetMagazine
        }

        public TargetGameObject shooter = new TargetGameObject(TargetGameObject.Target.Player);

        [Space] public Ammo ammo;
        public Operation operation = Operation.AddToStorage;
        public NumberProperty amount = new NumberProperty(10);

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            if (ammo == null) return true;

            CharacterShooter charShooter = this.shooter.GetComponent<CharacterShooter>(target);
            if (!charShooter)
            {
                Debug.LogError("Target Game Object does not have a CharacterShooter component");
                return true;
            }

            switch (this.operation)
            {
                case Operation.AddToStorage:
                    charShooter.AddAmmoToStorage(this.ammo.ammoID, this.amount.GetInt(target));
                    break;

                case Operation.SetStorage:
                    charShooter.SetAmmoToStorage(this.ammo.ammoID, this.amount.GetInt(target));
                    break;
                
                case Operation.AddToMagazine:
                    charShooter.AddAmmoToClip(this.ammo.ammoID, this.amount.GetInt(target));
                    break;
                
                case Operation.SetMagazine:
                    charShooter.SetAmmoToClip(this.ammo.ammoID, this.amount.GetInt(target));
                    break;
            }

            return true;
        }

		#if UNITY_EDITOR
        public static new string NAME = "Shooter/Give Ammo";
        private const string NODE_TITLE = "{0} Ammo {1} to {2}";

        public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Shooter/Extra/Icons/Actions/";

        public override string GetNodeTitle()
        {
            return string.Format(
                NODE_TITLE,
                this.operation,
                (this.ammo == null ? "(none)" : this.ammo.ammoID),
                this.shooter
            );
        }
        #endif
    }
}
