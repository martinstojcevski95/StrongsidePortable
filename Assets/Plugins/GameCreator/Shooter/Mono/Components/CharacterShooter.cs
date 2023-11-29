namespace GameCreator.Shooter
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.Audio;
    using GameCreator.Core;
    using GameCreator.Characters;
    using GameCreator.Variables;
    using System;

    [AddComponentMenu("Game Creator/Shooter/Character Shooter")]
    public class CharacterShooter : GlobalID, IGameSave
    {
        private const float TRANSITION = 0.2f;

        private const float MIN_RAND_PITCH = 0.9f;
        private const float MAX_RAND_PITCH = 1.1f;

        public enum ShotType
        {
            Normal = 1, // 01 Mask
            Charge = 2, // 10 Mask
            Any    = 3, // 11 Mask
        }

        public class EventWeapon : UnityEvent<Weapon> { }
        public class EventAmmo : UnityEvent<Ammo> { }
        public class EventShooter : UnityEvent<Weapon, Ammo> { }
        public class EventActivation : UnityEvent<bool> { }

        // PROPERTIES: ---------------------------------------------------------

        public Weapon currentWeapon;
        public Ammo currentAmmo;

        public EventWeapon eventEquipWeapon = new EventWeapon();
        public EventWeapon eventUnequipWeapon = new EventWeapon();
        public EventAmmo eventChangeAmmo = new EventAmmo();
        public EventActivation eventOnAim = new EventActivation();

        public EventShooter eventReload = new EventShooter();
        public EventShooter eventShoot = new EventShooter();
        public EventShooter eventChargedShotStart = new EventShooter();
        public EventShooter eventChargedShotStop = new EventShooter();

        public Character character { get; protected set; }
        public CharacterAnimator animator { get; protected set; }

        public CharacterAimIK aimIK { get; protected set; }

        protected CharacterAmmoData ammoData = new CharacterAmmoData();

        public GameObject modelWeapon { get; protected set; }
        public WeaponMuzzle muzzle { get; protected set; }
        public TrajectoryRenderer trajectoryRenderer { get; set; }

        public bool isAiming { get; protected set; }
        public bool isChargingShot { get; set; }
        public bool isReloading { get; protected set; }
        public bool isDrawing { get; protected set; }
        public bool isHolstering { get; protected set; }

        public AimingBase aiming { get; protected set; }

        public float chargeShotStartTime;

        // INITIALIZERS: -------------------------------------------------------

        protected virtual void Start()
        {
            if (!Application.isPlaying) return;

            this.character = GetComponent<Character>();
            if (this.character != null)
            {
                this.animator = this.character.GetCharacterAnimator();
            }

            SaveLoadManager.Instance.Initialize(this);
        }

        private void OnDestroy()
        {
            base.OnDestroyGID();

            if (!Application.isPlaying) return;
            if (this.exitingApplication) return;

            SaveLoadManager.Instance.OnDestroyIGameSave(this);
        }

        // UPDATE METHOD: ------------------------------------------------------

        protected virtual void Update()
        {
            if (!Application.isPlaying) return;
            if (this.aiming != null)
            {
                this.aiming.Update();
            }
        }

        // PUBLIC METHODS: -----------------------------------------------------

        public IEnumerator ChangeWeapon(Weapon weapon, Ammo ammo = null)
        {
            this.RequireAimIK();
            if (this.isReloading) yield break;
            if (this.isDrawing) yield break;
            if (this.isHolstering) yield break;

            if (this.character != null)
            {
                if (!this.character.IsControllable()) yield break;
                if (this.character.characterLocomotion.isBusy) yield break;
            }

            if (this.character != null)
            {
                this.character.characterLocomotion.isBusy = true;
            }

            if (this.isAiming) this.StopAiming();
            if (this.isChargingShot) this.currentAmmo.StopCharge(this);

            yield return this.UnequipWeapon();
            yield return this.EquipWeapon(weapon, ammo);

            if (this.character != null)
            {
                this.character.characterLocomotion.isBusy = false;
            }
        }

        public void ChangeAmmo(Ammo ammo)
        {
            this.RequireAimIK();
            if (this.isReloading) return;
            if (this.isDrawing) return;
            if (this.isHolstering) return;

            if (this.isChargingShot) this.currentAmmo.StopCharge(this);

            this.currentAmmo = ammo;
            this.eventChangeAmmo.Invoke(this.currentAmmo);
        }

        public IEnumerator Reload()
        {
            if (this.currentWeapon == null) yield break;
            if (this.currentAmmo == null) yield break;
            if (this.isReloading) yield break;
            if (this.isDrawing) yield break;
            if (this.isHolstering) yield break;
            if (this.character != null)
            {
                if (!this.character.IsControllable()) yield break;
                if (this.character.characterLocomotion.isBusy) yield break;
            }

            int currentClip = this.GetAmmoInClip(this.currentAmmo.ammoID);
            int currentStorage = this.GetAmmoInStorage(this.currentAmmo.ammoID);
            if (currentStorage <= 0 || currentClip >= this.currentAmmo.clipSize)
            {
                yield break;
            }

            this.OnBeforeReload();
            this.RequireAimIK();

            if (this.isChargingShot) this.currentAmmo.StopCharge(this);

            this.isReloading = true;
            yield return this.currentAmmo.Reload(this);

            this.isReloading = false;

            int clipSize = this.currentAmmo.clipSize;
            this.ammoData.Reload(
                this.currentAmmo.ammoID,
                clipSize,
                this.currentAmmo.infiniteAmmo
            );

            this.OnAfterReload();
        }

        public void StartAiming(AimingBase aiming)
        {
            if (this.currentWeapon == null) return;
            if (this.currentAmmo == null) return;
            if (this.character != null)
            {
                if (!this.character.IsControllable()) return;
                if (this.character.characterLocomotion.isBusy) return;
            }

            this.RequireAimIK();

            if (this.isDrawing) return;
            if (this.isHolstering) return;

            if (this.isChargingShot) this.currentAmmo.StopCharge(this);

            this.isAiming = true;
            this.aiming = aiming;

            this.ChangeState(this.currentWeapon.aiming, Weapon.STATE_LAYER_AIM);
            if (this.aimIK) this.aimIK.SetState(this.currentWeapon.aiming);

            this.eventOnAim.Invoke(true);
            this.OnStartAimWeapon();
        }

        public void StopAiming()
        {
            if (this.currentWeapon == null) return;
            if (this.currentAmmo == null) return;

            this.RequireAimIK();

            if (this.aiming != null) this.aiming.Stop();
            if (this.isChargingShot) this.currentAmmo.StopCharge(this);

            this.isAiming = false;
            this.aiming = null;

            if (this.animator)
            {
                CharacterState currentState = this.animator.GetState(Weapon.STATE_LAYER_AIM);
                this.ResetState(currentState, Weapon.STATE_LAYER_AIM);
                if (this.aimIK) this.aimIK.SetState(this.currentWeapon.ease);
            }

            this.eventOnAim.Invoke(false);
            this.OnStopAimWeapon();
        }

        public void Shoot()
        {
            if (this.currentWeapon == null) return;
            if (this.currentAmmo == null) return;
            if (this.isReloading) return;
            if (this.isDrawing) return;
            if (this.isHolstering) return;
            if (this.isChargingShot) return;
            if (this.character != null)
            {
                if (!this.character.IsControllable()) return;
                if (this.character.characterLocomotion.isBusy) return;
            }

            if (this.muzzle == null)
            {
                Debug.LogError("No Muzzle Component found! Make sure your weapon prefab has one");
                return;
            }

            this.OnBeforeShoot();
            this.RequireAimIK();

            if (this.currentAmmo.Shoot(this, this.GetShootDeviation(), ShotType.Normal))
            {
                this.OnAfterShoot();
            }
        }

        public void StartChargedShot()
        {
            if (this.currentWeapon == null) return;
            if (this.currentAmmo == null) return;
            if (this.isReloading) return;
            if (this.isDrawing) return;
            if (this.isHolstering) return;
            if (this.character != null)
            {
                if (!this.character.IsControllable()) return;
                if (this.character.characterLocomotion.isBusy) return;
            }
            
            if (this.muzzle == null)
            {
                Debug.LogError("No Muzzle Component found! Make sure your weapon prefab has one");
                return;
            }

            this.RequireAimIK();
            this.currentAmmo.StartChargedShot(this);
        }

        public void ExecuteChargedShot()
        {
            if (this.currentWeapon == null) return;
            if (this.currentAmmo == null) return;
            if (this.isReloading) return;
            if (this.isDrawing) return;
            if (this.isHolstering) return;
            if (this.character != null)
            {
                if (!this.character.IsControllable()) return;
                if (this.character.characterLocomotion.isBusy) return;
            }
            
            if (this.muzzle == null)
            {
                Debug.LogError("No Muzzle Component found! Make sure your weapon prefab has one");
                return;
            }

            this.OnBeforeExecuteChargedShot();
            this.OnBeforeShoot();

            this.RequireAimIK();

            if (this.currentAmmo.ExecuteChargedShot(this, this.GetShootDeviation()))
            {
                this.OnAfterExecuteChargedShot();
                this.OnAfterShoot();
            }
        }

        public float GetCharge()
        {
            if (this.currentWeapon == null) return 0f;
            if (this.currentAmmo == null) return 0f;

            if (this.isChargingShot)
            {
                float duration = this.currentAmmo.chargeTime.GetValue(gameObject);
                return (Time.time - this.chargeShotStartTime) / duration;
            }

            return 0f;
        }

        public int GetAmmoInClip(string ammoID)
        {
            if (this.ammoData.ContainsKey(ammoID))
            {
                return this.ammoData.GetInClip(ammoID);
            }

            return 0;
        }

        public int GetAmmoInStorage(string ammoID)
        {
            if (this.ammoData.ContainsKey(ammoID))
            {
                if (this.currentAmmo.infiniteAmmo) return int.MaxValue;
                return this.ammoData.GetInStorage(ammoID);
            }

            return 0;
        }

        public void SetAmmoToClip(string ammoID, int amount)
        {
            if (!this.ammoData.ContainsKey(ammoID))
            {
                this.ammoData.Add(ammoID, new CharacterAmmoData.Data());
            }

            this.ammoData.SetClip(ammoID, amount);
        }

        public void AddAmmoToClip(string ammoID, int amount)
        {
            if (!this.ammoData.ContainsKey(ammoID))
            {
                this.ammoData.Add(ammoID, new CharacterAmmoData.Data());
            }

            this.ammoData.AddClip(ammoID, amount);
        }

        public void SetAmmoToStorage(string ammoID, int amount)
        {
            if (!this.ammoData.ContainsKey(ammoID))
            {
                this.ammoData.Add(ammoID, new CharacterAmmoData.Data());
            }

            this.ammoData.SetStorage(ammoID, amount);
        }

        public void AddAmmoToStorage(string ammoID, int amount)
        {
            if (!this.ammoData.ContainsKey(ammoID))
            {
                this.ammoData.Add(ammoID, new CharacterAmmoData.Data());
            }

            this.ammoData.AddStorage(ammoID, amount);
        }

        public bool CanShootFireRate(string ammoID, float fireRate)
        {
            if (!this.ammoData.ContainsKey(ammoID))
            {
                this.ammoData.Add(ammoID, new CharacterAmmoData.Data());
            }

            return this.ammoData.CanShoot(ammoID, fireRate);
        }

        public void UpdateShootFireRate(string ammoID)
        {
            if (!this.ammoData.ContainsKey(ammoID))
            {
                this.ammoData.Add(ammoID, new CharacterAmmoData.Data());
            }

            this.ammoData.UpdateShotTime(ammoID);
        }

        public void SetPitch(float angle)
        {
            if (this.currentWeapon == null) return;
            if (this.RequireAimIK())
            {
                this.aimIK.SetAimAngle(angle);
            }
        }

        public void AddPitch(float angle)
        {
            if (this.currentWeapon == null) return;
            if (this.RequireAimIK())
            {
                this.aimIK.AddAimAngle(angle);
            }
        }

        public void PlayAudio(AudioClip audioClip)
        {
            if (audioClip == null) return;

            Vector3 position = transform.position;
            if (this.muzzle != null) position = this.muzzle.transform.position;

            float pitch = UnityEngine.Random.Range(MIN_RAND_PITCH, MAX_RAND_PITCH);
            AudioMixerGroup soundMixer = DatabaseGeneral.Load().soundAudioMixer;

            AudioManager.Instance.PlaySound3D(
                audioClip, 0f, position, 1f, pitch,
                1.0f, soundMixer
            );
        }

        public void AddListenerStorageChange(UnityAction<string, int> callback)
        {
            this.ammoData.AddListenerStorageChange(callback);
        }

        public void AddListenerClipChange(UnityAction<string, int> callback)
        {
            this.ammoData.AddListenerClipChange(callback);
        }

        public void RmvListenerStorageChange(UnityAction<string, int> callback)
        {
            this.ammoData.RmvListenerStorageChange(callback);
        }

        public void RmvListenerClipChange(UnityAction<string, int> callback)
        {
            this.ammoData.RmvListenerClipChange(callback);
        }

        // PRIVATE & PROTECTED METHODS: ----------------------------------------

        private IEnumerator UnequipWeapon()
        {
            this.OnBeforeUnequipWeapon();

            WaitForSeconds wait = new WaitForSeconds(0f);
            if (this.currentWeapon != null)
            {
                if (this.currentWeapon.aiming.state != null)
                {
                    CharacterState currentState = this.animator.GetState(Weapon.STATE_LAYER_IDLE);
                    if (currentState != null && currentState.exitClip != null)
                    {
                        float time = this.ResetState(currentState, Weapon.STATE_LAYER_IDLE);
                        wait = new WaitForSeconds(time);
                    }
                }

                this.PlayAudio(this.currentWeapon.audioHolster);
                if (this.aimIK) this.aimIK.SetWeapon(null, null);
            }

            this.isHolstering = true;
            yield return wait;

            this.eventUnequipWeapon.Invoke(this.currentWeapon);
            if (this.modelWeapon != null) Destroy(this.modelWeapon);
            if (this.currentAmmo != null) this.currentAmmo.RemoveAmmoPrefab(this);

            this.OnAfterUnequipWeapon();

            yield return wait;
            this.isHolstering = false;

            this.currentWeapon = null;
            this.currentAmmo = null;
        }

        private IEnumerator EquipWeapon(Weapon weapon, Ammo ammo)
        {
            if (weapon != null)
            {
                this.currentWeapon = weapon;
                this.ChangeAmmo(ammo ?? this.currentWeapon.defaultAmmo);

                this.OnBeforeEquipWeapon();

                WaitForSeconds wait = new WaitForSeconds(0f);

                if (this.currentWeapon.ease.state != null)
                {
                    CharacterState state = this.currentWeapon.ease.state;
                    float time = this.ChangeState(
                        this.currentWeapon.ease,
                        Weapon.STATE_LAYER_IDLE
                    );

                    if (state.enterClip != null) wait = new WaitForSeconds(time);
                }

                this.PlayAudio(this.currentWeapon.audioDraw);

                if (this.aimIK)
                {
                    this.aimIK.SetWeapon(
                        this.currentWeapon,
                        this.currentAmmo
                    );
                }

                this.isDrawing = true;
                yield return wait;

                this.eventEquipWeapon.Invoke(this.currentWeapon);
                this.modelWeapon = this.currentWeapon.EquipWeapon(this.transform, this.animator);
                this.muzzle = this.modelWeapon.GetComponentInChildren<WeaponMuzzle>();
                if (this.muzzle != null) this.muzzle.Setup(this);

                int ammoClip = this.GetAmmoInClip(this.currentAmmo.ammoID);
                if (this.currentAmmo != null && ammoClip > 0) this.currentAmmo.AddAmmoPrefab(this);

                this.OnAfterEquipWeapon();

                yield return wait;
                this.isDrawing = false;
            }
        }

        protected float ChangeState(Weapon.State state, CharacterAnimation.Layer layer)
        {
            float time = TRANSITION;
            if (state.state != null)
            {
                if (state.state.enterClip != null)
                {
                    time = state.state.enterClip.length;
                }

                time = Mathf.Max(TRANSITION, time) * 0.5f;
                this.animator.SetState(
                    state.state, state.mask,
                    1f, time, 1f, layer
                );
            }

            return time;
        }

        protected float ResetState(CharacterState state, CharacterAnimation.Layer layer)
        {
            float time = TRANSITION;
            if (state != null)
            {
                if (state.enterClip != null)
                {
                    time = state.exitClip.length;
                }

                time = Mathf.Max(TRANSITION, time) * 0.5f;
                this.animator.ResetState(time, layer);
            }

            return time;
        }

        protected bool RequireAimIK()
        {
            if (this.animator == null) return false;
            if (this.aimIK != null) return true;

            GameObject target = this.animator.animator.gameObject;
            this.aimIK = target.AddComponent<CharacterAimIK>();
            this.aimIK.Setup(this);

            return true;
        }

        // VIRTUAL METHODS: ----------------------------------------------------

        protected virtual void OnBeforeEquipWeapon() { }
        protected virtual void OnAfterEquipWeapon() { }

        protected virtual void OnBeforeUnequipWeapon() { }
        protected virtual void OnAfterUnequipWeapon() { }

        protected virtual void OnStartAimWeapon() { }
        protected virtual void OnStopAimWeapon() { }

        protected virtual void OnBeforeShoot() { }
        protected virtual void OnAfterShoot() { }

        protected virtual void OnBeforeReload() { }
        protected virtual void OnAfterReload() { }

        protected virtual void OnBeforeExecuteChargedShot() { }
        protected virtual void OnAfterExecuteChargedShot() { }

        public virtual float GetShootDeviation()
        {
            if (this.character != null)
            {
                Character.State state = this.character.GetCharacterState();
                if (state.forwardSpeed.magnitude <= 0.1f) return 0;
                else if (this.character.IsGrounded()) return 0.5f;
                else return 1f;
            }

            return 0f;
        }

        // SAVE LOAD SYSTEM: ---------------------------------------------------

        public string GetUniqueName()
        {
            return string.Format("shooter:{0}", this.GetUniqueID());
        }

        protected virtual string GetUniqueID()
        {
            return this.GetID();
        }

        public Type GetSaveDataType()
        {
            return typeof(CharacterAmmoData);
        }

        public object GetSaveData()
        {
            return this.ammoData;
        }

        public void ResetData()
        {
            this.ammoData = new CharacterAmmoData();
        }

        public void OnLoad(object generic)
        {
            this.ammoData = generic as CharacterAmmoData;
            if (this.ammoData == null)
            {
                this.ammoData = new CharacterAmmoData();
                foreach (KeyValuePair<string, CharacterAmmoData.Data> entry in this.ammoData)
                {
                    // reset shot time
                    entry.Value.shotTime = -1000f;
                }
            }
        }
    }
}
