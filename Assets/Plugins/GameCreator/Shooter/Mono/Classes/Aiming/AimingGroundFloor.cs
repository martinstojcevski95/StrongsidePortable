namespace GameCreator.Shooter
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Core.Hooks;
    using GameCreator.Characters;

    public class AimingGroundFloor :  AimingBase
    {
        public AimingGroundFloor(CharacterShooter shooter) : base(shooter)
        {
            this.aimTo = AimTo.Position;
        }

        private RaycastHit[] buffer = new RaycastHit[1];

        public override void Update()
        {
            if (this.character)
            {
                this.character.characterLocomotion.overrideFaceDirection = (
                    CharacterLocomotion.OVERRIDE_FACE_DIRECTION.GroundPlaneCursor
                );
            }

            Camera camera = HookCamera.Instance.Get<Camera>();
            Ray cameraRay = camera.ScreenPointToRay(Input.mousePosition);

            QueryTriggerInteraction query = QueryTriggerInteraction.Ignore;
            if (Physics.RaycastNonAlloc(cameraRay, buffer, Mathf.Infinity, -1, query) > 0)
            {
                this.aimToVector = buffer[0].point;
            }

            WeaponMuzzle muzzle = this.GetMuzzle();
            this.pointShootingRaycast = muzzle.GetPosition();
            this.pointShootingWeapon = muzzle.GetPosition();

            float angle = this.GetPitch((this.aimToVector - this.pointShootingWeapon).normalized);
            this.shooter.SetPitch(angle);
        }
    }
}
 