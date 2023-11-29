namespace GameCreator.Shooter
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Characters;

    [AddComponentMenu("Game Creator/Shooter/Player Shooter")]
    public class PlayerShooter : CharacterShooter
    {
        protected float currPrecision;
        protected float targPrecision;
        protected float veloPrecision;

        protected override void Update()
        {
            base.Update();

            if (!Application.isPlaying) return;

            if (this.isReloading || this.isHolstering || this.isDrawing)
            {
                this.targPrecision = 1f;
            }
            else
            {
                Character.State state = this.character.GetCharacterState();
                Vector3 movement = state.forwardSpeed + (Vector3.up * state.verticalSpeed);
                this.targPrecision = movement.magnitude / this.character.characterLocomotion.runSpeed;
            }

            float smoothDown = (
                this.currentAmmo != null &&
                this.currentAmmo.aimingMode == Ammo.AimType.Crosshair
                    ? this.currentAmmo.crosshairFocusTime
                    : WeaponCrosshair.SMOOTH_TIME_DN
            );

            this.currPrecision = Mathf.SmoothDamp(
                this.currPrecision,
                this.targPrecision,
                ref this.veloPrecision,
                (this.currPrecision > this.targPrecision
                    ? smoothDown : WeaponCrosshair.SMOOTH_TIME_UP
                )
            );
        }

        protected override void OnAfterShoot()
        {
            base.OnAfterShoot();

            this.currPrecision += this.currentAmmo.recoil;
            this.currPrecision = Mathf.Clamp01(this.currPrecision);
            this.veloPrecision = 0f;
        }

        protected override void OnStartAimWeapon()
        {
            base.OnStartAimWeapon();

            if (this.currentAmmo.aimingMode != Ammo.AimType.Crosshair) return;
            WeaponCrosshair.Create(this.currentAmmo.crosshair);
        }

        protected override void OnStopAimWeapon()
        {
            base.OnStopAimWeapon();
            WeaponCrosshair.Destroy();
        }

        public override float GetShootDeviation()
        {
            return this.currPrecision;
        }

        protected override string GetUniqueID()
        {
            return "player";
        }
    }
}