namespace GameCreator.Shooter
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Variables;
    using GameCreator.Characters;

    [AddComponentMenu("")]
    public class IgniterOnDraw : Igniter
    {
        public TargetGameObject shooter = new TargetGameObject(TargetGameObject.Target.Player);

        #if UNITY_EDITOR
        public new static string NAME = "Shooter/On Draw Weapon";
        #endif

        private void Start()
        {
            CharacterShooter charShooter = this.GetShooter();
            if (charShooter != null)
            {
                charShooter.eventEquipWeapon.AddListener(this.Callback);
            }
        }

        private void OnDestroy()
        {
            CharacterShooter charShooter = this.GetShooter();
            if (charShooter != null)
            {
                charShooter.eventEquipWeapon.RemoveListener(this.Callback);
            }
        }

        protected void Callback(Weapon weapon)
        {
            CharacterShooter charShooter = this.GetShooter();
            if (charShooter != null)
            {
                this.ExecuteTrigger(charShooter.gameObject);
            }
        }

        protected CharacterShooter GetShooter()
        {
            return this.shooter.GetComponent<CharacterShooter>(gameObject);
        }
    }
}