namespace GameCreator.Shooter
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Core.Hooks;
    using GameCreator.Characters;

    [AddComponentMenu("Game Creator/Shooter/Crosshair")]
    public class WeaponCrosshair : MonoBehaviour
    {
        public static WeaponCrosshair CURRENT;

        private const string DEFAULT_PATH = "GameCreator/Crosshair-Default";
        private static readonly int ANIM_PRECISION = Animator.StringToHash("Precision");

        public const float SMOOTH_TIME_UP = 0.05f;
        public const float SMOOTH_TIME_DN = 0.25f;

        private CharacterShooter shooter;
        private Animator animator;

        private void Awake()
        {
            this.animator = this.GetComponent<Animator>();
        }

        private void Update()
        {
            if (this.shooter == null) return;
            if (this.animator == null) return;

            float deviation = this.shooter.GetShootDeviation();
            this.animator.SetFloat(ANIM_PRECISION, deviation);
        }

        public void Setup()
        {
            this.shooter = HookPlayer.Instance.Get<CharacterShooter>();
            if (CURRENT != null) WeaponCrosshair.Destroy();
            CURRENT = this;
        }

        public static void Create(GameObject prefab = null)
        {
            if (prefab == null) prefab = Resources.Load<GameObject>(DEFAULT_PATH);
            GameObject instance = Instantiate(prefab);

            WeaponCrosshair crosshair = instance.GetComponent<WeaponCrosshair>();
            crosshair.Setup();
        }

        public static void Destroy()
        {
            if (CURRENT == null) return;
            GameObject.Destroy(CURRENT.gameObject);
        }
    }
}