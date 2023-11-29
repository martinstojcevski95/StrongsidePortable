namespace GameCreator.Shooter
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Variables;
    using GameCreator.Characters;

    [AddComponentMenu("")]
    public class IgniterOnAim : IgniterShooterBase
    {
        public enum Aim
        {
            OnStartAiming,
            OnStopAiming
        }

        #if UNITY_EDITOR
        public new static string NAME = "Shooter/On Aim";
        #endif

        public Aim aim = Aim.OnStartAiming;

        private void Start()
        {
            CharacterShooter charShooter = this.GetShooter();
            if (charShooter != null)
            {
                charShooter.eventOnAim.AddListener(this.OnAimChange);
            }
        }

        private void OnDestroy()
        {
            CharacterShooter charShooter = this.GetShooter();
            if (charShooter != null)
            {
                charShooter.eventOnAim.RemoveListener(this.OnAimChange);
            }
        }

        private void OnAimChange(bool aim)
        {
            bool conditions =
                (aim && this.aim == Aim.OnStartAiming) ||
                (!aim && this.aim == Aim.OnStopAiming);

            CharacterShooter charShooter = this.GetShooter();
            if (charShooter != null && conditions)
            {
                this.ExecuteTrigger(charShooter.gameObject);
            }
        }
    }
}