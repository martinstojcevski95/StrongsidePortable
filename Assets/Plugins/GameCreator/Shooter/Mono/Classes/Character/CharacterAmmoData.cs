namespace GameCreator.Shooter
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using GameCreator.Core;
    using GameCreator.Characters;

    [Serializable]
    public class CharacterAmmoData : SerializableDictionaryBase<string, CharacterAmmoData.Data>
    {
        public class EventAmmo : UnityEvent<string, int> { }

        public readonly EventAmmo eventStorage = new EventAmmo();
        public readonly EventAmmo eventClip = new EventAmmo();

        public void AddListenerStorageChange(UnityAction<string, int> callback)
        {
            this.eventStorage.AddListener(callback);
        }

        public void AddListenerClipChange(UnityAction<string, int> callback)
        {
            this.eventClip.AddListener(callback);
        }

        public void RmvListenerStorageChange(UnityAction<string, int> callback)
        {
            this.eventStorage.RemoveListener(callback);
        }

        public void RmvListenerClipChange(UnityAction<string, int> callback)
        {
            this.eventClip.RemoveListener(callback);
        }

        public void Reload(string ammoID, int clipSize, bool infiniteAmmo)
        {
            Data data = this.RequestData(ammoID);

            int available = Mathf.Max(clipSize - data.inClip, 0);
            int amount = (infiniteAmmo
                ? available
                : Mathf.Min(data.inStorage, available)
            );

            if (!infiniteAmmo) data.inStorage -= amount;
            data.inClip += amount;

            this.eventStorage.Invoke(ammoID, data.inStorage);
            this.eventClip.Invoke(ammoID, data.inClip);
        }

        public int GetInStorage(string ammoID)
        {
            Data data = this.RequestData(ammoID);
            return data.inStorage;
        }

        public int GetInClip(string ammoID)
        {
            Data data = this.RequestData(ammoID);
            return data.inClip;
        }

        public void AddStorage(string ammoID, int amount)
        {
            Data data = this.RequestData(ammoID);
            data.inStorage += amount;
            this.eventStorage.Invoke(ammoID, data.inStorage);
        }

        public void SetStorage(string ammoID, int amount)
        {
            Data data = this.RequestData(ammoID);
            data.inStorage = amount;
            this.eventStorage.Invoke(ammoID, data.inStorage);
        }

        public void AddClip(string ammoID, int amount)
        {
            Data data = this.RequestData(ammoID);
            data.inClip += amount;
            this.eventClip.Invoke(ammoID, data.inClip);
        }

        public void SetClip(string ammoID, int amount)
        {
            Data data = this.RequestData(ammoID);
            data.inClip = amount;
            this.eventClip.Invoke(ammoID, data.inClip);
        }

        public void UpdateShotTime(string ammoID)
        {
            Data data = this.RequestData(ammoID);
            data.shotTime = Time.time;
        }

        public bool CanShoot(string ammoID, float fireRate)
        {
            Data data =this.RequestData(ammoID);
            float offsetTime = 1f / fireRate;
            return data.shotTime + offsetTime < Time.time;
        }

        private Data RequestData(string ammoID)
        {
            Data data = null;
            this.dictionary.TryGetValue(ammoID, out data);
            if (data == null)
            {
                data = new Data();
                this.dictionary.Add(ammoID, data);
            }

            return data;
        }

        [Serializable]
        public class Data
        {
            public int inStorage;
            public int inClip;
            public float shotTime;

            public Data()
            {
                this.inStorage = 0;
                this.inClip = 0;
                this.shotTime = -1000f;
            }
        }
    }
}