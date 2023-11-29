namespace GameCreator.Shooter
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using GameCreator.Core;
    using GameCreator.Characters;

    [AddComponentMenu("Game Creator/Shooter/UI/Ammo UI")]
    public class AmmoUI : MonoBehaviour
    {
        public TargetGameObject character = new TargetGameObject(TargetGameObject.Target.Player);
        private string currentAmmoID = "";

        [Header("Information")]
        public Text ammoName;
        public Text ammoDescription;

        [Header("Ammo values")]
        public Text ammoInClip;
        public Text ammoMaxClip;
        public Text ammoInStorage;

        private void Start()
        {
            CharacterShooter shooter = this.character.GetComponent<CharacterShooter>(gameObject);
            if (shooter == null) return;

            shooter.AddListenerClipChange(this.OnChangeInClip);
            shooter.AddListenerStorageChange(this.OnChangeInStorage);
            shooter.eventChangeAmmo.AddListener(this.OnChangeAmmo);

            if (shooter.currentAmmo != null)
            {
                this.currentAmmoID = shooter.currentAmmo.ammoID;
                this.OnChangeAmmo(shooter.currentAmmo);
            }
        }

        private void OnDestroy()
        {
            CharacterShooter shooter = this.character.GetComponent<CharacterShooter>(gameObject);
            if (shooter == null) return;
        }

        protected virtual void OnChangeInClip(string ammoID, int amount)
        {
            if (this.ammoInClip == null) return;
            if (this.currentAmmoID != ammoID) return;

            this.ammoInClip.text = amount.ToString();
        }

        protected virtual void OnChangeInStorage(string ammoID, int amount)
        {
            if (this.ammoInStorage == null) return;
            if (this.currentAmmoID != ammoID) return;

            this.ammoInStorage.text = amount.ToString();
        }

        protected virtual void OnChangeAmmo(Ammo ammo)
        {
            if (ammo == null) return;
            this.currentAmmoID = ammo.ammoID;

            if (this.ammoMaxClip != null) this.ammoMaxClip.text = ammo.clipSize.ToString();
            if (this.ammoName != null) this.ammoName.text = ammo.ammoName.GetText();
            if (this.ammoDescription != null) this.ammoDescription.text = ammo.ammoDesc.GetText();

            CharacterShooter shooter = this.character.GetComponent<CharacterShooter>(gameObject);
            if (shooter != null)
            {
                this.OnChangeInClip(this.currentAmmoID, shooter.GetAmmoInClip(this.currentAmmoID));
                this.OnChangeInStorage(this.currentAmmoID, shooter.GetAmmoInStorage(this.currentAmmoID));
            }
        }
    }
}