namespace GameCreator.Shooter
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Variables;
    using GameCreator.Characters;

    [AddComponentMenu("")]
    public class IgniterOnShoot : IgniterShooterBase
    {
        #if UNITY_EDITOR
        public new static string NAME = "Shooter/On Shoot";
        #endif

        private void Start()
        {
            CharacterShooter charShooter = this.GetShooter();
            if (charShooter != null)
            {
                charShooter.eventShoot.AddListener(this.Callback);
            }
        }

        private void OnDestroy()
        {
            CharacterShooter charShooter = this.GetShooter();
            if (charShooter != null)
            {
                charShooter.eventShoot.RemoveListener(this.Callback);
            }
        }
    }
}