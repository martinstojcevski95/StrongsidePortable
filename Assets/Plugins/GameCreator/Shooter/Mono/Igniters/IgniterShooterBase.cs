namespace GameCreator.Shooter
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Variables;
    using GameCreator.Characters;

    [AddComponentMenu("")]
    public abstract class IgniterShooterBase : Igniter 
	{
		public TargetGameObject shooter = new TargetGameObject(TargetGameObject.Target.Player);

        protected CharacterShooter GetShooter()
        {
            return this.shooter.GetComponent<CharacterShooter>(gameObject);
        }

        protected void Callback(Weapon weapon, Ammo ammo)
        {
            CharacterShooter charShooter = this.GetShooter();
            if (charShooter != null)
            {
                this.ExecuteTrigger(charShooter.gameObject);
            }
        }
    }
}