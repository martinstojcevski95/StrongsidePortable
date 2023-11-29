namespace GameCreator.Shooter
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Core.Hooks;
    using GameCreator.Characters;

    public class AimingCameraDirection : AimingBase
    {
        public AimingCameraDirection(CharacterShooter character) : base(character)
        {
            this.aimTo = AimTo.Direction;
        }

        public override void Update()
        {
            if (this.character)
            {
                this.character.characterLocomotion.overrideFaceDirection = (
                    CharacterLocomotion.OVERRIDE_FACE_DIRECTION.CameraDirection
                );
            }

            Vector3 cameraForward = HookCamera.Instance.transform.TransformDirection(Vector3.forward);

            float angle = this.GetPitch(cameraForward);
            this.shooter.SetPitch(angle);

            WeaponMuzzle muzzle = this.GetMuzzle();
            this.pointShootingWeapon = muzzle.GetPosition();
            this.pointShootingRaycast = HookCamera.Instance.transform.position;

            this.aimToVector = cameraForward;
        }
    }
}