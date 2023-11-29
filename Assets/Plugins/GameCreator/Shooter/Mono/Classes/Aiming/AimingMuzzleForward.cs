namespace GameCreator.Shooter
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Core.Hooks;
    using GameCreator.Characters;

    public class AimingMuzzleForward : AimingBase
    {
        public AimingMuzzleForward(CharacterShooter shooter) : base(shooter)
        {
            this.aimTo = AimTo.Direction;
        }

        public override void Update()
        {
            WeaponMuzzle muzzle = this.GetMuzzle();

            this.pointShootingRaycast = muzzle.GetPosition();
            this.pointShootingWeapon = muzzle.GetPosition();

            this.shooter.SetPitch(0f);
            this.aimToVector = muzzle.GetDirection() * this.shooter.currentAmmo.distance;
        }
    }
}