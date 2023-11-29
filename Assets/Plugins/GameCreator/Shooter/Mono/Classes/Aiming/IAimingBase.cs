namespace GameCreator.Shooter
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Characters;

    public abstract class AimingBase
    {
        public enum AimTo
        {
            Direction,
            Position
        }

        protected const float INFINITY = 1000f;

        protected Character character;
        protected CharacterShooter shooter;

        protected GameObject turret;

        protected Transform shootingAnchor;

        public Vector3 pointShootingRaycast { get; protected set; }
        public Vector3 pointShootingWeapon { get; protected set; }

        public AimTo aimTo { get; protected set; }
        public Vector3 aimToVector;

        protected AimingBase(CharacterShooter characterShooter)
        {
            this.shooter = characterShooter;

            switch (this.shooter.character != null)
            {
                case true:
                    this.character = this.shooter.character;
                    CharacterAnimator characterAnimator = this.character.GetCharacterAnimator();

                    Animator animator = characterAnimator.animator;
                    this.shootingAnchor = animator.GetBoneTransform(HumanBodyBones.Chest);
                    break;

                case false:
                    this.turret = this.shooter.gameObject;
                    this.shootingAnchor = this.turret.transform;
                    break;
            }
        }

        public virtual void Setup(params object[] parameters)
        {
            return;
        }

        public virtual void Stop()
        {
            if (!this.character) return;
            this.character.characterLocomotion.overrideFaceDirection = (
                CharacterLocomotion.OVERRIDE_FACE_DIRECTION.None
            );
        }

        public abstract void Update();

        public Vector3 GetAimingDirection()
        {
            switch (this.aimTo)
            {
                case AimTo.Direction: return this.aimToVector;
                case AimTo.Position: return this.aimToVector - this.pointShootingWeapon;
            }

            return this.GetMuzzle().GetDirection();
        }

        public Vector3 GetAimingPosition()
        {
            switch (this.aimTo)
            {
                case AimTo.Direction: return this.pointShootingWeapon + (this.aimToVector.normalized * INFINITY);
                case AimTo.Position: return this.aimToVector;
            }

            return default(Vector3);
        }

        // PRIVATE METHODS: ---------------------------------------------------------------------

        protected float GetPitch(Vector3 worldDirection)
        {
            Vector3 characterForward = this.character
                ? this.character.transform.TransformDirection(Vector3.forward)
                : this.turret.transform.TransformDirection(Vector3.forward);

            Vector3 characterRight = this.character
                ? this.character.transform.TransformDirection(Vector3.right)
                : this.turret.transform.TransformDirection(Vector3.right);

            Vector3 cameraForward = worldDirection;

            Vector3 cameraProjection = Vector3.ProjectOnPlane(cameraForward, characterRight);

            float dot = Vector3.Dot(cameraProjection, characterForward);
            float angle = Vector3.SignedAngle(
                characterForward,
                cameraProjection,
                characterRight
            );

            return dot < 0f ? 0f : angle;
        }

        protected WeaponMuzzle GetMuzzle()
        {
            if (this.shooter && this.shooter.muzzle == null)
            {
                Debug.LogError("<b>Shooter Module Error:</b> Weapon does not have a muzzle component");
                return null;
            }

            return this.shooter.muzzle;
        }
    }
}