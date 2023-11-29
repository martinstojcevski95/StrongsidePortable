namespace GameCreator.Shooter
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Core.Hooks;
    using GameCreator.Characters;

    public class AimingSideScrollPlane :  AimingBase
    {
        public enum Coordinates
		{
            XY,
            YZ
		}

        private Coordinates coordinates = Coordinates.XY;

        public AimingSideScrollPlane(CharacterShooter shooter) : base(shooter)
        {
            this.aimTo = AimTo.Direction;
        }

        public override void Setup(params object[] parameters)
        {
            this.coordinates = (Coordinates)parameters[0];
        }

        public override void Update()
        {
            Camera camera = HookCamera.Instance.Get<Camera>();

            Ray cursorFromCameraRay = camera.ScreenPointToRay(Input.mousePosition);
            Transform shooterPosition = this.shooter.transform;

            Plane plane = default(Plane);
            float rayDistance;

            WeaponMuzzle muzzle = this.GetMuzzle();
            this.pointShootingRaycast = muzzle.GetPosition();
            this.pointShootingWeapon = muzzle.GetPosition();

            switch (this.coordinates)
            {
                case Coordinates.XY:
                    plane = new Plane(Vector3.forward, shooterPosition.position);
                    this.pointShootingRaycast = new Vector3(
                        this.pointShootingRaycast.x,
                        this.pointShootingRaycast.y,
                        shooterPosition.position.z
                    );
                    break;

                case Coordinates.YZ:
                    plane = new Plane(Vector3.right, shooterPosition.position);
                    this.pointShootingRaycast = new Vector3(
                        shooterPosition.position.x,
                        this.pointShootingRaycast.y,
                        this.pointShootingRaycast.z
                    );
                    break;
            }

            Vector3 aimDirection = muzzle.GetDirection();
            if (plane.Raycast(cursorFromCameraRay, out rayDistance))
            {
                Vector3 cursor = cursorFromCameraRay.GetPoint(rayDistance);
                aimDirection = cursor - this.pointShootingRaycast;
            }

            float angle = this.GetPitch(aimDirection);
            this.shooter.SetPitch(angle);
            this.aimToVector = aimDirection.normalized * this.shooter.currentAmmo.distance;

            if (this.character)
            {
                this.character.characterLocomotion.overrideFaceDirection = (
                    CharacterLocomotion.OVERRIDE_FACE_DIRECTION.Target
                );

                switch (this.coordinates)
                {
                    case Coordinates.XY:
                        Vector3 targetPositionXY = (
                            this.shooter.transform.position +
                            (aimDirection.x > 0f ? Vector3.right : Vector3.left)
                        );

                        TargetPosition xyFace = new TargetPosition(TargetPosition.Target.Position);
                        xyFace.targetPosition = targetPositionXY;
                        this.character.characterLocomotion.overrideFaceDirectionTarget = xyFace;
                        break;

                    case Coordinates.YZ:
                        Vector3 targetPositionYZ = (
                            this.character.transform.position +
                            (aimDirection.z > 0f ? Vector3.forward : Vector3.back)
                        );

                        TargetPosition yzFace = new TargetPosition(TargetPosition.Target.Position);
                        yzFace.targetPosition = targetPositionYZ;

                        this.character.characterLocomotion.overrideFaceDirection = (
                            CharacterLocomotion.OVERRIDE_FACE_DIRECTION.Target
                        );
                        this.character.characterLocomotion.overrideFaceDirectionTarget = yzFace;
                        break;
                }
            }
        }
    }
}
 