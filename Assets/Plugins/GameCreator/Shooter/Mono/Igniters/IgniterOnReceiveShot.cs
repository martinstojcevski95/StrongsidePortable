namespace GameCreator.Shooter
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Variables;
    using UnityEngine.Serialization;

    [AddComponentMenu("")]
    public class IgniterOnReceiveShot : Igniter 
	{
		#if UNITY_EDITOR
        public new static string NAME = "Shooter/On Receive Shot";
        public new static bool REQUIRES_COLLIDER = true;
        #endif

        [FormerlySerializedAs("filterType")]
        public CharacterShooter.ShotType filterByShotType = CharacterShooter.ShotType.Any;

        [TagSelector]
        public string filterByTag = "";
        public Weapon filterByWeapon;
        public Ammo filterByAmmo;

        [Space]
        [VariableFilter(Variable.DataType.GameObject)]
        public VariableProperty storeShooter = new VariableProperty();
        
        [Space]
        [VariableFilter(Variable.DataType.Vector3)]
        public VariableProperty storeImpactPosition = new VariableProperty();

        public void OnReceiveShot(CharacterShooter shooter, CharacterShooter.ShotType type, Vector3 impact)
        {
            if (((int)type & (int)this.filterByShotType) == 0) return;
            bool tagFilter = (
                string.IsNullOrEmpty(this.filterByTag) ||
                this.filterByTag == "Untagged" ||
                shooter.CompareTag(this.filterByTag)
            );

            if (!tagFilter) return;

            if (this.filterByWeapon && shooter.currentWeapon != this.filterByWeapon) return;
            if (this.filterByAmmo && shooter.currentAmmo != this.filterByAmmo) return;

            this.storeShooter.Set(shooter.gameObject, gameObject);
            this.storeImpactPosition.Set(impact, gameObject);

            this.ExecuteTrigger(shooter.gameObject);
        }
	}
}