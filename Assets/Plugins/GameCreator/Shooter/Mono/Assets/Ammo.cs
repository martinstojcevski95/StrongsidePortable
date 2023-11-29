namespace GameCreator.Shooter
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Characters;
    using GameCreator.Variables;
    using GameCreator.Localization;
    using GameCreator.Pool;

    [CreateAssetMenu(fileName = "New Ammo", menuName = "Game Creator/Shooter/Ammo")]
    public class Ammo : ScriptableObject
    {
        public enum AimType
        {
            None,
            Crosshair,
            Trajectory
        }

        public enum ShootType
        {
            Projectile,
            Raycast,
            RaycastAll,
            TrajectoryCast,
            SphereCast,
            SphereCastAll
        }

        public enum TriggerType
        {
            DisableCharge  = 1, // 01
            RequireCharge  = 2, // 10
            OptionalCharge = 3  // 11
        }

        private enum CastType
        {
            RaycastOne    = 0, // 00 [Type, Reach]
            RaycastAll    = 1, // 01 [Type, Reach]
            SphereCastOne = 2, // 10 [Type, Reach]
            SphereCastAll = 3, // 10 [Type, Reach]
        }

        private class ShootData
        {
            public Vector3 originRaycast;
            public Vector3 originWeapon;
            public Vector3 destination;

            public ShootData(Vector3 originRaycast, Vector3 originWeapon, Vector3 destination)
            {
                this.originRaycast = originRaycast;
                this.originWeapon = originWeapon;
                this.destination = destination;
            }
        }

        private class RaycastComparer : IComparer<RaycastHit>
        {
            public int Compare(RaycastHit x, RaycastHit y)
            {
                return x.distance.CompareTo(y.distance);
            }
        }

        private static readonly RaycastComparer SHOT_COMPARE = new RaycastComparer();

        // PROPERTIES: ----------------------------------------------------------------------------

        public string ammoID = "";
        [LocStringNoPostProcess] public LocString ammoName = new LocString("Ammo Name");
        [LocStringNoPostProcess] public LocString ammoDesc = new LocString("Ammo Description");

        // general
        public NumberProperty fireRate = new NumberProperty(5);
        public bool infiniteAmmo = false;
        public int clipSize = 10;
        public bool autoReload = true;
        public float reloadDuration = 1f;

        // aiming mode
        public AimType aimingMode = AimType.Crosshair;
        public TrajectoryRenderer.Trajectory trajectory = new TrajectoryRenderer.Trajectory();
        public GameObject crosshair;
        public float crosshairFocusTime = 0.25f;
        public float projectileVelocity = 10f;

        // charge
        public TriggerType chargeType = TriggerType.DisableCharge;
        [Min(0f)] public float minChargeTime = 0.25f;
        public NumberProperty chargeTime = new NumberProperty(1f);
        [VariableFilter(Variable.DataType.Number)]
        public VariableProperty chargeValue = new VariableProperty();

        // shooting mode
        public ShootType shootType = ShootType.Raycast;
        public float distance = 50f;
        public float radius = 0.5f;
        public LayerMask layerMask = -1;
        public QueryTriggerInteraction triggersMask = QueryTriggerInteraction.Ignore;
        public float pushForce = 0f;
        public ShootingTrailRenderer.ShootingTrail shootTrail = new ShootingTrailRenderer.ShootingTrail();
        public GameObject prefabProjectile;
		public GameObject prefabImpactEffect;
        public GameObject prefabMuzzleFlash;

        [Range(0f, 1f)] public float recoil = 0.2f;
        public float delay = 0f;

        public NumberProperty minSpread = new NumberProperty(0.01f);
        public NumberProperty maxSpread = new NumberProperty(0.15f);

        // prefab model
        public GameObject prefabAmmo;
        public Weapon.AttachmentBone prefabAmmoBone = Weapon.AttachmentBone.RightHand;
        public Vector3 prefabAmmoPosition;
        public Vector3 prefabAmmoRotation;

        private RaycastHit[] bufferCastHits = new RaycastHit[100];

        // SOUNDS: --------------------------------------------------------------------------------

        public AudioClip audioShoot;
        public AudioClip audioEmpty;
        public AudioClip audioReload;

        // ANIMATIONS: ----------------------------------------------------------------------------

        public AnimationClip animationShoot;
        public AvatarMask maskShoot;

        public AnimationClip animationReload;
        public AvatarMask maskReload;

        // ACTIONS: -------------------------------------------------------------------------------

        [Header("Actions")]
        public IActionsList actionsOnStartCharge;
        public IActionsList actionsOnEndCharge;
        public IActionsList actionsOnShoot;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public bool StartChargedShot(CharacterShooter shooter)
        {
            if (!this.CanStartChargedShoot(shooter)) return false;

            shooter.chargeShotStartTime = Time.time;
            shooter.isChargingShot = true;

            if (this.actionsOnStartCharge)
            {
                ShootData shootData = this.GetShootData(shooter);

                GameObject actionsInstance = Instantiate(
                    this.actionsOnStartCharge.gameObject,
                    shootData.originWeapon,
                    Quaternion.FromToRotation(
                        Vector3.up,
                        shootData.destination - shootData.originWeapon
                    )
                );

                actionsInstance.hideFlags = HideFlags.HideInHierarchy;
                Actions actions = actionsInstance.GetComponent<Actions>();
                if (actions) actions.Execute(shooter.gameObject, null);
            }

            if (this.aimingMode == AimType.Trajectory)
            {
                if (shooter.trajectoryRenderer) Destroy(shooter.trajectoryRenderer.gameObject);

                this.trajectory.layerMask = this.layerMask;
                this.trajectory.maxDistance = this.distance;
                this.trajectory.shooter = shooter;

                Transform parent = (shooter.muzzle != null
                    ? shooter.muzzle.transform
                    : shooter.transform
                );

                shooter.trajectoryRenderer = TrajectoryRenderer.Create(this.trajectory, parent);
            }

            shooter.eventChargedShotStart.Invoke(shooter.currentWeapon, this);

            return true;
        }

        public bool ExecuteChargedShot(CharacterShooter shooter, float deviation)
        {
            if (!this.CanExecuteChargedShoot(shooter)) return false;

            if (Time.time - shooter.chargeShotStartTime < this.minChargeTime)
            {
                this.StopCharge(shooter);
                return false;
            }

            if (!this.ClipRequirements(shooter, false))
            {
                this.StopCharge(shooter);
                return false;
            }

            float maxCharge = this.chargeTime.GetValue(shooter.gameObject);
            float charge = (Time.time - shooter.chargeShotStartTime) / maxCharge;
            this.chargeValue.Set(Mathf.Clamp01(charge), shooter.gameObject);

            this.Shoot(shooter, deviation, CharacterShooter.ShotType.Charge);
            this.StopCharge(shooter);

            return true;
        }

        public void StopCharge(CharacterShooter shooter)
        {
            if (!this.actionsOnEndCharge) return;
            if (!shooter.isChargingShot) return;
            if (this.chargeType == TriggerType.DisableCharge) return;

            shooter.isChargingShot = false;
            ShootData shootData = this.GetShootData(shooter);

            GameObject actionsInstance = Instantiate(
                this.actionsOnEndCharge.gameObject,
                shootData.originWeapon,
                Quaternion.FromToRotation(Vector3.up, shootData.destination - shootData.originWeapon)
            );

            actionsInstance.hideFlags = HideFlags.HideInHierarchy;
            Actions actions = actionsInstance.GetComponent<Actions>();

            if (actions) actions.Execute(shooter.gameObject, null);
            if (shooter.trajectoryRenderer) Destroy(shooter.trajectoryRenderer.gameObject);

            shooter.eventChargedShotStart.Invoke(shooter.currentWeapon, this);
        }

        public bool Shoot(CharacterShooter shooter, float deviation, CharacterShooter.ShotType shotType)
        {
            if (!this.CanShoot(shooter, shotType, true)) return false;
            this.ShootGeneric(shooter, deviation, shotType);

            shooter.eventShoot.Invoke(shooter.currentWeapon, this);

            if (shooter.GetAmmoInClip(this.ammoID) <= 0)
            {
                this.RemoveAmmoPrefab(shooter);
            }

            return true;
        }

        public IEnumerator Reload(CharacterShooter shooter)
        {
            WaitForSeconds wait = new WaitForSeconds(this.reloadDuration / 2f);
            if (shooter.animator && this.animationReload)
            {
                float speed = this.animationReload.length / this.reloadDuration;
                shooter.animator.CrossFadeGesture(
                    this.animationReload, speed, this.maskReload,
                    0.2f, 0.2f
                );
            }

            if (shooter.aimIK)
            {
                shooter.aimIK.SetStability(
                    shooter.currentWeapon.aiming.stabilizeBody,
                    0.1f
                );
            }

            shooter.PlayAudio(this.audioReload);

            yield return wait;

            this.AddAmmoPrefab(shooter);

            if (shooter.aimIK) shooter.aimIK.SetStability(false);
            shooter.eventReload.Invoke(shooter.currentWeapon, this);

            yield return wait;
        }

        public bool CanShoot(CharacterShooter shooter, CharacterShooter.ShotType shotType, bool subtract)
        {
            if (!this.ShootingRequirements(shooter)) return false;
            if (((int)this.chargeType & (int)shotType) == 0) return false;
            if (!this.ClipRequirements(shooter, subtract)) return false;

            return true;
        }

        public bool CanStartChargedShoot(CharacterShooter shooter)
        {
            if (!this.ShootingRequirements(shooter)) return false;
            if (this.chargeType == TriggerType.DisableCharge) return false;
            if (!this.ClipRequirements(shooter, false)) return false;

            return true;
        }

        public bool CanExecuteChargedShoot(CharacterShooter shooter)
        {
            if (!this.ShootingRequirements(shooter)) return false;
            if (!shooter.isChargingShot) return false;
            if (this.chargeType == TriggerType.DisableCharge) return false;

            return true;
        }

        public void RemoveAmmoPrefab(CharacterShooter shooter)
        {
            if (!this.prefabAmmo) return;
            CharacterAttachments attachments = this.GetCharacterAttachments(shooter);

            if (!attachments) return;
            attachments.Remove((HumanBodyBones)this.prefabAmmoBone);
        }

        public void AddAmmoPrefab(CharacterShooter shooter)
        {
            if (!this.prefabAmmo) return;

            CharacterAttachments attachments = this.GetCharacterAttachments(shooter);
            if (!attachments) return;

            GameObject instance = Instantiate<GameObject>(
                this.prefabAmmo,
                Vector3.zero,
                Quaternion.identity
            );

            instance.transform.localScale = Vector3.one;
            attachments.Attach(
                (HumanBodyBones)this.prefabAmmoBone,
                instance,
                this.prefabAmmoPosition,
                Quaternion.Euler(this.prefabAmmoRotation)
            );
        }

        // PRIVATE METHODS: ----------------------------------------------------

        private void ShootGeneric(CharacterShooter shooter, float deviation,
            CharacterShooter.ShotType shotType)
        {
            shooter.UpdateShootFireRate(this.ammoID);
            shooter.PlayAudio(this.audioShoot);

            if (this.animationShoot && shooter.animator != null)
            {
                shooter.animator.CrossFadeGesture(
                    this.animationShoot, 1f,
                    this.maskShoot,
                    0.05f, 0.2f
                );
            }

            if (this.prefabMuzzleFlash)
            {
                GameObject muzzleInstance = PoolManager.Instance.Pick(this.prefabMuzzleFlash);
                muzzleInstance.transform.SetPositionAndRotation(
                    shooter.muzzle.GetPosition(),
                    shooter.muzzle.GetRotation()
                );
            }

            if (this.delay < 0.01f) this.ShootSelection(shooter, deviation, shotType);
            else
            {
                CoroutinesManager.Instance.StartCoroutine(
                   this.ShootSelectionDelayed(shooter, deviation, shotType)
                );
            }
        }

        private IEnumerator ShootSelectionDelayed(CharacterShooter shooter, float deviation,
            CharacterShooter.ShotType shotType)
        {
            WaitForSeconds wait = new WaitForSeconds(this.delay);
            yield return wait;

            if (shooter) this.ShootSelection(shooter, deviation, shotType);
        }

        private void ShootSelection(CharacterShooter shooter, float deviation,
            CharacterShooter.ShotType shotType)
        {
            switch (this.shootType)
            {
                case ShootType.Projectile: this.ShootProjectile(shooter, deviation, shotType); break;
                case ShootType.Raycast: this.ShootGenericCast(shooter, deviation, shotType, CastType.RaycastOne); break;
                case ShootType.RaycastAll: this.ShootGenericCast(shooter, deviation, shotType, CastType.RaycastAll); break;
                case ShootType.TrajectoryCast: this.ShootTrajectoryCast(shooter, deviation, shotType); break;
                case ShootType.SphereCast: this.ShootGenericCast(shooter, deviation, shotType, CastType.SphereCastOne); break;
                case ShootType.SphereCastAll: this.ShootGenericCast(shooter, deviation, shotType, CastType.SphereCastAll); break;
            }
        }

        private void ShootProjectile(CharacterShooter shooter, float deviation,
            CharacterShooter.ShotType shotType)
        {
            ShootData shootData = this.GetShootData(shooter);
            GameObject bullet = null;
            float velocity = 1f;

            if (this.aimingMode == AimType.Crosshair || this.aimingMode == AimType.None)
            {
                Vector3 shootPositionA = shootData.originWeapon;
                Vector3 shootPositionB = shootData.destination;

                shootPositionB += this.CalculateError(
                    shooter.gameObject,
                    shootPositionA,
                    shootPositionB,
                    deviation
                );

                velocity = this.projectileVelocity;

                bullet = PoolManager.Instance.Pick(this.prefabProjectile);
                bullet.transform.SetPositionAndRotation(
                    shootPositionA,
                    Quaternion.LookRotation(shootPositionB - shootPositionA)
                );
            }
            else if (this.aimingMode == AimType.Trajectory)
            {
                Vector3 shootPositionA = shootData.originWeapon;

                velocity = Mathf.Lerp(
                    this.trajectory.minVelocity,
                    this.trajectory.maxVelocity,
                    shooter.GetCharge()
                );

                Vector3 shootDirection = shooter.muzzle.GetDirection();

                bullet = PoolManager.Instance.Pick(this.prefabProjectile);
                bullet.transform.SetPositionAndRotation(
                    shootPositionA,
                    Quaternion.LookRotation(shootDirection)
                );
            }

            if (bullet)
            {
                Rigidbody bulletRB = bullet.GetComponent<Rigidbody>();
                if (bulletRB)
                {
                    Vector3 direction = bullet.transform.TransformDirection(Vector3.forward);
                    bulletRB.velocity = Vector3.zero;
                    bulletRB.angularVelocity = Vector3.zero;

                    bulletRB.AddForce(direction * velocity, ForceMode.VelocityChange);
                }

                this.ExecuteShootActions(
                    shooter.gameObject,
                    shooter.muzzle.GetPosition(),
                    shooter.muzzle.GetRotation()
                );
            }
        }

        private void ShootGenericCast(CharacterShooter shooter, float deviation,
            CharacterShooter.ShotType shotType, CastType castType)
        {
            ShootData shootData = this.GetShootData(shooter);

            Vector3 shootPositionRaycast = shootData.originRaycast;
            Vector3 shootPositionWeapon = shootData.originWeapon;
            Vector3 shootPositionTarget = shootData.destination;

            shootPositionTarget += this.CalculateError(
                shooter.gameObject,
                shootPositionRaycast,
                shootPositionTarget,
                deviation
            );

            Vector3 direction = (shootPositionTarget - shootPositionRaycast).normalized;

            int hitCounter = 0;
            switch ((((int)castType) & 2) >> 1)
            {
                case 0: // Raycast
                    hitCounter = Physics.RaycastNonAlloc(
                        shootPositionRaycast, direction, this.bufferCastHits,
                        this.distance, this.layerMask, this.triggersMask
                    );
                    break;

                case 1: // SphereCast
                    hitCounter = Physics.SphereCastNonAlloc(
                        shootPositionRaycast, this.radius, direction, this.bufferCastHits,
                        this.distance, this.layerMask, this.triggersMask
                    );
                    break;
            }

            int maxCount = Mathf.Min(hitCounter, this.bufferCastHits.Length);
            Array.Sort(this.bufferCastHits, 0, maxCount, SHOT_COMPARE);

            for (int i = 0; hitCounter > 0 && i < maxCount; ++i)
            {
                Vector3 point = this.bufferCastHits[i].point;
                if (this.bufferCastHits[i].distance <= 0.01f && point == Vector3.zero)
                {
                    // special case in which sphere sweep overlaps the initial value:
                    point = shootPositionWeapon + (direction.normalized * 0.1f);
                }

                if (this.bufferCastHits[i].collider.gameObject.Equals(shooter.gameObject)) continue;

                Vector3 rotationDirection = (shootPositionWeapon - point).normalized;
                Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, rotationDirection);

                GameObject other = this.bufferCastHits[i].collider.gameObject;
                this.ExecuteShootActions(other, point, rotation);

                IgniterOnReceiveShot[] igniters = other.GetComponentsInChildren<IgniterOnReceiveShot>();
                foreach (IgniterOnReceiveShot igniter in igniters)
                {
                    if (igniter) igniter.OnReceiveShot(shooter, shotType, point);
                }

                if (!Mathf.Approximately(this.pushForce, 0f) && this.bufferCastHits[i].rigidbody)
                {
                    this.bufferCastHits[i].rigidbody.AddForceAtPosition(
                        -rotationDirection * this.pushForce,
                        point,
                        ForceMode.Impulse
                    );
                }

                shootPositionTarget = point;

                if (this.prefabImpactEffect)
                {
                    GameObject impact = PoolManager.Instance.Pick(this.prefabImpactEffect);
                    impact.transform.SetPositionAndRotation(shootPositionTarget, rotation);
                }

                if ((((int)castType) & 1) == 0) break;
            }

            if (this.shootTrail.useShootingTrail)
            {
                this.shootTrail.position1 = shootPositionWeapon;
                this.shootTrail.position2 = shootPositionTarget;
                ShootingTrailRenderer.Create(this.shootTrail);
            }
        }

        private void ShootTrajectoryCast(CharacterShooter shooter, float deviation, CharacterShooter.ShotType shotType)
        {
            TrajectoryRenderer.TrajectoryResult result = this.GetTrajectoryResult(shooter);

            if (result.hit.collider)
            {
                Vector3 pointA = result.hit.point;
                Vector3 pointB = result.hit.point;

                if (result.points.Length > 2)
                {
                    pointA = result.points[result.count - 2];
                    pointB = result.points[result.count - 1];
                }

                Vector3 direction = (pointA - pointB).normalized;
                Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, direction);
                GameObject other = result.hit.collider.gameObject;


                this.ExecuteShootActions(
                    other,
                    result.hit.point,
                    rotation
                );

                IgniterOnReceiveShot[] igniters = other.GetComponentsInChildren<IgniterOnReceiveShot>();
                foreach (IgniterOnReceiveShot igniter in igniters)
                {
                    if (igniter) igniter.OnReceiveShot(shooter, shotType, result.hit.point);
                }

                if (!Mathf.Approximately(this.pushForce, 0f) && result.hit.rigidbody && direction != Vector3.zero)
                {
                    result.hit.rigidbody.AddForceAtPosition(
                        -direction * this.pushForce,
                        result.hit.point,
                        ForceMode.Impulse
                    );
                }

                if (this.prefabImpactEffect)
                {
                    GameObject impact = PoolManager.Instance.Pick(this.prefabImpactEffect);
                    impact.transform.SetPositionAndRotation(result.hit.point, rotation);
                }

                this.shootTrail.position1 = shooter.muzzle.GetPosition();
                this.shootTrail.position2 = result.hit.point;
            }

            if (this.shootTrail.useShootingTrail)
            {
                ShootingTrailRenderer.Create(this.shootTrail, result);
            }
        }

        // AUXILIARY PRIVATE METHODS: -------------------------------------------------------

        private bool ShootingRequirements(CharacterShooter shooter)
        {
            if (!shooter.isAiming) return false;
            if (!shooter.currentWeapon) return false;
            if (!shooter.currentAmmo) return false;

            float fireRateValue = this.fireRate.GetValue(shooter.gameObject);
            if (!shooter.CanShootFireRate(this.ammoID, fireRateValue)) return false;

            return true;
        }

        private bool ClipRequirements(CharacterShooter shooter, bool subtract)
        {
            if (shooter.GetAmmoInClip(this.ammoID) > 0)
            {
                if (subtract) shooter.AddAmmoToClip(this.ammoID, -1);
                return true;
            }

            if (shooter.GetAmmoInStorage(this.ammoID) > 0 && this.autoReload)
            {
                shooter.StartCoroutine(shooter.Reload());
            }
            else
            {
                shooter.PlayAudio(this.audioEmpty);
            }

            return false;
        }

        private void ExecuteShootActions(GameObject target, Vector3 position, Quaternion rotation)
        {
            if (this.actionsOnShoot)
            {
                GameObject actionsInstance = Instantiate<GameObject>(
                    this.actionsOnShoot.gameObject,
                    position,
                    rotation
                );

                actionsInstance.hideFlags = HideFlags.HideInHierarchy;
                Actions actions = actionsInstance.GetComponent<Actions>();

                if (!actions) return;
                actions.Execute(target, null);
            }
        }

        private ShootData GetShootData(CharacterShooter shooter)
        {
            Vector3 originTrail = shooter.transform.position;
            if (shooter.muzzle) originTrail = shooter.muzzle.transform.position;
            else if (shooter.modelWeapon) originTrail = shooter.modelWeapon.transform.position;

            Vector3 originRay = shooter.aiming.pointShootingRaycast;
            Vector3 originWeapon = shooter.aiming.pointShootingWeapon;
            Vector3 destination = shooter.aiming.GetAimingPosition();

            return new ShootData(originRay, originWeapon, destination);
        }

        private TrajectoryRenderer.TrajectoryResult GetTrajectoryResult(CharacterShooter shooter)
        {
            TrajectoryRenderer.Trajectory data = this.trajectory;
            data.layerMask = this.layerMask;
            data.maxDistance = this.distance;
            data.shooter = shooter;

            Transform origin = (shooter.muzzle != null
                ? shooter.muzzle.transform
                : shooter.transform
            );

            return TrajectoryRenderer.GetTrajectory(
                origin,
                data,
                shooter.GetCharge()
            );
        }

        private Vector3 CalculateError(GameObject invoker, Vector3 origin, Vector3 destination, float deviation)
        {
            float distanceAB = Vector3.Distance(origin, destination);
            float errorMargin = distanceAB * Mathf.Lerp(
                this.minSpread.GetValue(invoker),
                this.maxSpread.GetValue(invoker),
                deviation
            );

            return new Vector3(
                this.RandomNormalDistribution(errorMargin),
                this.RandomNormalDistribution(errorMargin),
                this.RandomNormalDistribution(errorMargin)
            );
        }

        private float RandomNormalDistribution(float errorMargin)
        {
            float u1 = 1f - UnityEngine.Random.value;
            float u2 = 1f - UnityEngine.Random.value;

            float gauss = (
                Mathf.Sqrt(-2f * Mathf.Log(u1)) *
                Mathf.Sin(2f * Mathf.PI * u2)
            );

            return errorMargin * gauss;
        }

        private CharacterAttachments GetCharacterAttachments(CharacterShooter shooter)
        {
            CharacterAnimator characterAnimator = shooter.character.GetCharacterAnimator();

            if (!characterAnimator) return null;
            return characterAnimator.GetCharacterAttachments();
        }
    }
}