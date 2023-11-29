namespace GameCreator.Shooter
{
    using System.Collections;
    using System.Collections.Generic;
    using GameCreator.Characters;
    using UnityEngine;

    [AddComponentMenu("")]
    public class CharacterAimIK : MonoBehaviour
    {
        private const float PITCH_SMOOTH = 0.05f;
        private const float DEFAULT_SMOOTH = 0.1f;
        private const float MIN_ANGLE = -90f;
        private const float MAX_ANGLE = 90f;

        private static readonly Weapon.Bending[] STABILIZERS =
        {
            new Weapon.Bending() { bone = HumanBodyBones.Spine, weight = 1.0f },
            new Weapon.Bending() { bone = HumanBodyBones.Chest, weight = 0.0f },
            new Weapon.Bending() { bone = HumanBodyBones.UpperChest, weight = 0.0f }
        };

        private static readonly Weapon.Bending[] AIMING =
        {
            new Weapon.Bending() { bone = HumanBodyBones.Spine, weight = 0.2f },
            new Weapon.Bending() { bone = HumanBodyBones.Chest, weight = 0.3f },
            new Weapon.Bending() { bone = HumanBodyBones.LeftShoulder, weight = 0.5f },
            new Weapon.Bending() { bone = HumanBodyBones.RightShoulder, weight = 0.5f }
        };

        // PROPERTIES: --------------------------------------------------------------------

        private Animator animator;
        private CharacterShooter shooter;

        private float pitchCurrent;
        private float pitchTarget;
        private float pitchVelocity;

        public Weapon weapon;
        public Weapon.State state = new Weapon.State();
        public Ammo ammo;

        private Vector3 currentRotationOffset = Vector3.zero;
        private Vector3 targetRotationOffset = Vector3.zero;
        private Vector3 velocityRotationOffset = Vector3.zero;

        private float currentStability;
        private float targetStability;
        private float velocityStability;
        private float smoothStability = DEFAULT_SMOOTH;

        // INITIALIZERS: -------------------------------------------------------------------

        private void Awake()
        {
            this.animator = GetComponent<Animator>();
        }

        public void Setup(CharacterShooter characterShooter)
        {
            this.shooter = characterShooter;
        }

        // UPDATE METHODS: -----------------------------------------------------------------

        private void Update()
        {
            this.pitchCurrent = Mathf.SmoothDampAngle(
                this.pitchCurrent,
                (this.shooter.isAiming ? this.pitchTarget : 0f),
                ref this.pitchVelocity,
                PITCH_SMOOTH
            );

            this.currentRotationOffset = Vector3.SmoothDamp(
                this.currentRotationOffset,
                this.targetRotationOffset,
                ref this.velocityRotationOffset,
                DEFAULT_SMOOTH
            );

            this.currentStability = Mathf.SmoothDamp(
                this.currentStability,
                this.targetStability,
                ref this.velocityStability,
                this.smoothStability
            );

            float rotation = this.state.lowerBodyRotation;
            this.shooter.animator.SetRotationYaw(rotation);
        }

        protected void OnAnimatorIK()
        {
            this.targetRotationOffset = (this.weapon != null
                ? this.state.upperBodyRotation
                : Vector3.zero
            );

            for (int i = 0; i < STABILIZERS.Length; ++i)
            {
                if (this.animator.GetBoneTransform(STABILIZERS[i].bone) == null) continue;

                Vector3 rotation = this.currentRotationOffset * STABILIZERS[i].weight;
                this.AnimateBoneIK(STABILIZERS[i].bone, rotation);
            }
        }

        protected void AnimateBoneIK(HumanBodyBones bone, Vector3 offset)
        {
            Transform boneTarget = this.animator.GetBoneTransform(bone);
            Transform boneParent = boneTarget.parent;

            // rotation no longer has motion compensation. Look for a way to
            // fix it and use the inverse of the hips to compensate sway
            Quaternion stabilityRotation = Quaternion.identity;
            /*Quaternion stabilityRotation = Quaternion.Slerp(
                Quaternion.identity,
                Quaternion.Inverse(boneParent.localRotation),
                this.currentStability
            );*/

            // Compensate parent rotation:
            Quaternion rotStabilization = (
                boneTarget.localRotation *
                stabilityRotation
            );

            // Yaw posture
            Vector3 rotAbsoluteOffset = transform.parent.TransformDirection(offset);
            Quaternion rotOffset = Quaternion.Euler(boneTarget.InverseTransformDirection(rotAbsoluteOffset));

            Quaternion rotResult = rotStabilization * rotOffset;
            this.animator.SetBoneLocalRotation(bone, rotResult);
        }

        private void LateUpdate()
        {
            for (int i = 0; i < AIMING.Length; ++i)
            {
                this.UpdateAimBonePitch(
                    AIMING[i].bone,
                    this.pitchCurrent * AIMING[i].weight
                );
            }
        }

        private void UpdateAimBonePitch(HumanBodyBones bone, float pitch)
        {
            Transform boneTransform = this.animator.GetBoneTransform(bone);
            if (!boneTransform || !transform.parent) return;

            Vector3 parentRight = transform.parent.TransformDirection(Vector3.right);
            Vector3 boneRight = boneTransform.InverseTransformDirection(parentRight);

            boneTransform.Rotate(boneRight, pitch, Space.Self);
        }

        // PUBLIC METHODS: ----------------------------------------------------------------

        public void SetWeapon(Weapon weapon, Ammo ammo = null)
        {
            this.weapon = weapon;
            this.SetState(this.weapon != null
                ? this.weapon.ease
                : new Weapon.State()
            );

            this.ammo = ammo;
            this.SetAimAngle(0f);
        }

        public void SetState(Weapon.State state)
        {
            this.state = state;
            this.targetStability = this.state.stabilizeBody ? 1f : 0f;
        }

        public void SetStability(bool on, float smoothTime = DEFAULT_SMOOTH)
        {
            switch (on)
            {
                case true:
                    this.targetStability = 1.0f;
                    this.smoothStability = smoothTime;
                    break;

                case false:
                    this.targetStability = (this.state != null && this.state.stabilizeBody
                        ? 1f
                        : 0f
                    );
                    this.smoothStability = smoothTime;
                    break;
            }
        }

        public void SetAimAngle(float angle)
        {
            if (this.weapon != null) angle += this.weapon.pitchOffset;
            this.pitchTarget = Mathf.Clamp(angle, MIN_ANGLE, MAX_ANGLE);
        }

        public float GetAimAngle()
        {
            return this.pitchTarget;
        }

        public void AddAimAngle(float angle)
        {
            this.SetAimAngle(this.pitchTarget + angle);
        }
    }
}
