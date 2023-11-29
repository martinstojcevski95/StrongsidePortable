namespace GameCreator.Shooter
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Core.Hooks;
    using GameCreator.Characters;

    public class AimingAtTarget : AimingBase
    {
        protected TargetGameObject target;

        public AimingAtTarget(CharacterShooter shooter) : base(shooter)
        {
            this.aimTo = AimTo.Position;
        }

        public override void Setup(params object[] parameters)
        {
            this.target = parameters[0] as TargetGameObject;
        }

        public override void Update()
        {
            this.pointShootingRaycast = this.GetMuzzle().GetPosition();
            this.pointShootingWeapon = this.GetMuzzle().GetPosition();

            this.UpdateAimToVectorFromTarget();

            TargetPosition targetPosition = new TargetPosition(TargetPosition.Target.Position);
            targetPosition.targetPosition = this.aimToVector;

            if (this.character)
            {
                this.character.characterLocomotion.overrideFaceDirection = (
                    CharacterLocomotion.OVERRIDE_FACE_DIRECTION.Target
                );

                this.character.characterLocomotion.overrideFaceDirectionTarget = targetPosition;
            }

            Vector3 direction = this.aimToVector - this.shootingAnchor.position;
            float angle = this.GetPitch(direction);
            this.shooter.SetPitch(angle);
        }

        protected virtual void UpdateAimToVectorFromTarget()
        {
            Transform targetTransform = this.target.GetTransform(this.character.gameObject);
            if (targetTransform != null)
            {
                this.aimToVector = targetTransform.position;
                CharacterController targetCC = targetTransform.gameObject.GetComponent<CharacterController>();
                if (targetCC != null)
                {
                    this.aimToVector.y += (targetCC.height / 3f) * 2f;
                }
            }
            else
            {
                this.aimToVector = (
                    this.shootingAnchor.position +
                    this.shootingAnchor.TransformDirection(Vector3.forward)
                );
            }
        }
    }
}