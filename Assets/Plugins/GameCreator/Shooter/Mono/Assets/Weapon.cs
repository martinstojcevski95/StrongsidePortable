namespace GameCreator.Shooter
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Core.Hooks;
    using GameCreator.Variables;
    using GameCreator.Characters;
    using GameCreator.Localization;
    using UnityEngine.Serialization;

    [CreateAssetMenu(fileName = "New Weapon", menuName = "Game Creator/Shooter/Weapon")]
    public class Weapon : ScriptableObject
    {
        public const CharacterAnimation.Layer STATE_LAYER_IDLE = CharacterAnimation.Layer.Layer1;
        public const CharacterAnimation.Layer STATE_LAYER_AIM = CharacterAnimation.Layer.Layer2;

        [Serializable]
        public class Bending
        {
            public HumanBodyBones bone;
            public float weight;
        }

        [Serializable]
        public class State
        {
            public CharacterState state;
            public AvatarMask mask;
            public Vector3 upperBodyRotation;
            public float lowerBodyRotation;
            public bool stabilizeBody;
        }

        public static readonly Bending[] BENDING = {
            new Bending() { bone = HumanBodyBones.Spine, weight = 0.1f },
            new Bending() { bone = HumanBodyBones.Chest, weight = 0.2f },
            new Bending() { bone = HumanBodyBones.UpperChest, weight = 0.2f }
        };

        public enum AttachmentBone
        {
            Root = -1,
            LeftArm = HumanBodyBones.LeftLowerArm,
            LeftHand = HumanBodyBones.LeftHand,
            RightArm = HumanBodyBones.RightLowerArm,
            RightHand = HumanBodyBones.RightHand,
            Head = HumanBodyBones.Head,
            Camera = 100,
        }

        // PROPERTIES: ---------------------------------------------------------

        [LocStringNoPostProcess] public LocString weaponName = new LocString("Weapon Name");
        [LocStringNoPostProcess] public LocString weaponDesc = new LocString("Weapon Description");

        // General
        [FormerlySerializedAs("defaulAmmo")]
        public Ammo defaultAmmo;

        public AudioClip audioDraw;
        public AudioClip audioHolster;

        // Animation Rest
        public State ease = new State();

        // Animation Aiming
        public float pitchOffset;
        public State aiming = new State();

        // Model
        public GameObject prefab;
        public AttachmentBone attachment = AttachmentBone.RightHand;
        public Vector3 prefabPosition = Vector3.zero;
        public Vector3 prefabRotation = Vector3.zero;

        // PUBLIC METHODS: -----------------------------------------------------

        public GameObject EquipWeapon(Transform source, CharacterAnimator animator)
        {
            if (this.prefab == null) return null;

            Transform bone = null;
            switch (this.attachment)
            {
                case AttachmentBone.Root:
                    bone = source;
                    break;

                case AttachmentBone.Camera:
                    bone = HookCamera.Instance.transform;
                    break;

                default:
                    bone = animator.animator.GetBoneTransform((HumanBodyBones)this.attachment);
                    break;
            }

            if (!bone) return null;

            GameObject instance = GameObject.Instantiate<GameObject>(this.prefab);
            instance.transform.localScale = this.prefab.transform.localScale;

            instance.transform.SetParent(bone);

            instance.transform.localPosition = this.prefabPosition;
            instance.transform.localRotation = Quaternion.Euler(this.prefabRotation);

            return instance;
        }
    }
}
