namespace GameCreator.Shooter
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [AddComponentMenu("Game Creator/Shooter/Muzzle")]
    public class WeaponMuzzle : MonoBehaviour
    {
        private static readonly int ANIM_CHARGE = Animator.StringToHash("Charge");

        public float radius = 0.015f;
        public Animator animator;

        private CharacterShooter shooter;
        private bool useCharge = false;

        // INITIALIZERS: -------------------------------------------------------

        private void Start()
        {
            this.useCharge = this.HasCharge();
        }

        // PUBLIC METHODS: -----------------------------------------------------

        public void Setup(CharacterShooter shooter)
        {
            this.shooter = shooter;
        }

        public Vector3 GetDirection()
        {
            return transform.TransformDirection(Vector3.forward).normalized;
        }

        public Quaternion GetRotation()
        {
            return transform.rotation;
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        // PRIVATE METHODS: ----------------------------------------------------

        private void Update()
        {
            if (this.animator != null && this.useCharge)
            {
                this.animator.SetFloat(ANIM_CHARGE, shooter.GetCharge());
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, this.radius);
            Gizmos.DrawRay(
                transform.position,
                transform.TransformDirection(Vector3.forward)
            );
        }

        private bool HasCharge()
        {
            if (this.animator == null) return false;
            for (int i = 0; i < this.animator.parameters.Length; ++i)
            {
                if (this.animator.parameters[i].nameHash == ANIM_CHARGE) return true;
            }

            return false;
        }
    }
}
