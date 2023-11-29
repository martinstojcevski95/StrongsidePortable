namespace GameCreator.Shooter
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	 #if UNITY_EDITOR 
 using UnityEditor;
 #endif 
	using GameCreator.Core;
    using System;
    using System.IO;

    [CustomEditor(typeof(Ammo))]
	public class AmmoEditor : IShooterEditor
	{
        private static readonly GUIContent GC_AMID = new GUIContent("Ammo ID");
        private static readonly GUIContent GC_NAME = new GUIContent("Name");
		private static readonly GUIContent GC_DESC = new GUIContent("Description");
        private static readonly GUIContent GC_TRAIL = new GUIContent("Trail");
        private static readonly GUIContent GC_BONE = new GUIContent("Bone");
        private static readonly GUIContent GC_POS = new GUIContent("Position");
        private static readonly GUIContent GC_ROT = new GUIContent("Rotation");
        private static readonly GUIContent GC_FOCUS = new GUIContent("Focus Time");

        private const string ERR_MISS_CROSS = "Crosshair aiming requires a Crosshair prefab";
        private const string ERR_MISS_PROJ = "Shooting Projectile mode requires Projectile prefab";

        private const float SPACING = 2f;

        private const string PREFAB_NAME = "{0}.prefab";
        private const string PATH_ONSTART = "Assets/Plugins/GameCreatorData/Shooter/Actions/OnStart";
        private const string PATH_ONEND = "Assets/Plugins/GameCreatorData/Shooter/Actions/OnEnd";
        private const string PATH_ONSHOOT = "Assets/Plugins/GameCreatorData/Shooter/Actions/OnShoot";

        private static readonly GUIContent[] OPTIONS_ACTIONS = new GUIContent[]
        {
            new GUIContent("On Start Charging"),
            new GUIContent("On Shoot"),
            new GUIContent("On End Charging"),
        };

        // PROPERTIES: ---------------------------------------------------------

        private Ammo ammo;
        private SerializedProperty spID;
		private SerializedProperty spName;
		private SerializedProperty spDesc;

		private Section sectionGeneral;
		private Section sectionCharge;
		private Section sectionAiming;
		private Section sectionShooting;
        private Section sectionAmmoRepr;
        private Section sectionAudio;
        private Section sectionAnimations;

        private SerializedProperty spFireRate;
		private SerializedProperty spInfiniteAmmo;
		private SerializedProperty spClipSize;
		private SerializedProperty spAutoReload;
        private SerializedProperty spReloadDuration;

        private SerializedProperty spChargeType;
        private SerializedProperty spMinChargeTime;
        private SerializedProperty spChargeTime;
        private SerializedProperty spChargeValue;

        private SerializedProperty spAimingMode;
        private SerializedProperty spTrajectory;
        private SerializedProperty spCrosshair;
        private SerializedProperty spCrosshairFocusTime;
        private SerializedProperty spProjectileVelocity;

        private SerializedProperty spShootType;
        private SerializedProperty spDistance;
        private SerializedProperty spRadius;
        private SerializedProperty spLayerMask;
        private SerializedProperty spTriggersMask;
        private SerializedProperty spPrefabImpactEffect;
        private SerializedProperty spPrefabMuzzleFlash;
        private SerializedProperty spShootTrail;
        private SerializedProperty spPrefabProjectile;
        private SerializedProperty spRecoil;
        private SerializedProperty spDelay;
        private SerializedProperty spPushForce;
        private SerializedProperty spMinSpread;
        private SerializedProperty spMaxSpread;

        private SerializedProperty spPrefabAmmo;
        private SerializedProperty spPrefabAmmoBone;
        private SerializedProperty spPrefabAmmoPosition;
        private SerializedProperty spPrefabAmmoRotation;

        private SerializedProperty spAudioShoot;
        private SerializedProperty spAudioEmpty;
        private SerializedProperty spAudioReload;

        private SerializedProperty spAnimationShoot;
        private SerializedProperty spMaskShoot;
        private SerializedProperty spAnimationReload;
        private SerializedProperty spMaskReload;

        private IActionsListEditor actionsOnStartCharge;
        private IActionsListEditor actionsOnEndCharge;
        private IActionsListEditor actionsOnShoot;

        private SerializedProperty spActionsOnStartCharge;
        private SerializedProperty spActionsOnEndCharge;
        private SerializedProperty spActionsOnShoot;

        private int toolbarIndex = 1;

        // INITIALIZERS: -------------------------------------------------------

        private void OnEnable()
		{
			this.ammo = this.target as Ammo;
			if (this.ammo == null) return;

            this.spID = this.serializedObject.FindProperty("ammoID");
            if (string.IsNullOrEmpty(this.spID.stringValue))
            {
                this.spID.stringValue = Guid.NewGuid().ToString("N");
                this.serializedObject.ApplyModifiedProperties();
                this.serializedObject.Update();
            }

            this.spName = this.serializedObject.FindProperty("ammoName");
			this.spDesc = this.serializedObject.FindProperty("ammoDesc");

			this.sectionGeneral = new Section("General", this.LoadIcon("General"), this.Repaint);
			this.sectionCharge = new Section("Charging", this.LoadIcon("Charging"), this.Repaint);
			this.sectionAiming = new Section("Aiming", this.LoadIcon("Aiming"), this.Repaint);
			this.sectionShooting = new Section("Shooting", this.LoadIcon("Shooting"), this.Repaint);
            this.sectionAmmoRepr = new Section("Ammo Model", this.LoadIcon("Ammo-Model"), this.Repaint);
            this.sectionAudio = new Section("Audio", this.LoadIcon("Audio"), this.Repaint);
            this.sectionAnimations = new Section("Animations", this.LoadIcon("Animations"), this.Repaint);

            this.spFireRate = this.serializedObject.FindProperty("fireRate");
            this.spInfiniteAmmo = this.serializedObject.FindProperty("infiniteAmmo");
            this.spClipSize = this.serializedObject.FindProperty("clipSize");
            this.spAutoReload = this.serializedObject.FindProperty("autoReload");
            this.spReloadDuration = this.serializedObject.FindProperty("reloadDuration");

            this.spChargeType = this.serializedObject.FindProperty("chargeType");
            this.spMinChargeTime = this.serializedObject.FindProperty("minChargeTime");
            this.spChargeTime = this.serializedObject.FindProperty("chargeTime");
            this.spChargeValue = this.serializedObject.FindProperty("chargeValue");

            this.spAimingMode = this.serializedObject.FindProperty("aimingMode");
            this.spTrajectory = this.serializedObject.FindProperty("trajectory");
            this.spCrosshair = this.serializedObject.FindProperty("crosshair");
            this.spCrosshairFocusTime = this.serializedObject.FindProperty("crosshairFocusTime");
            this.spProjectileVelocity = this.serializedObject.FindProperty("projectileVelocity");

            this.spShootType = this.serializedObject.FindProperty("shootType");
            this.spDistance = this.serializedObject.FindProperty("distance");
            this.spRadius = this.serializedObject.FindProperty("radius");
            this.spLayerMask = this.serializedObject.FindProperty("layerMask");
            this.spTriggersMask = this.serializedObject.FindProperty("triggersMask");
            this.spPrefabImpactEffect = this.serializedObject.FindProperty("prefabImpactEffect");
            this.spPrefabMuzzleFlash = this.serializedObject.FindProperty("prefabMuzzleFlash");
            this.spShootTrail = this.serializedObject.FindProperty("shootTrail");
            this.spPrefabProjectile = this.serializedObject.FindProperty("prefabProjectile");
            this.spRecoil = this.serializedObject.FindProperty("recoil");
            this.spDelay = this.serializedObject.FindProperty("delay");
            this.spPushForce = this.serializedObject.FindProperty("pushForce");
            this.spMinSpread = this.serializedObject.FindProperty("minSpread");
            this.spMaxSpread = this.serializedObject.FindProperty("maxSpread");

            this.spPrefabAmmo = this.serializedObject.FindProperty("prefabAmmo");
            this.spPrefabAmmoBone = this.serializedObject.FindProperty("prefabAmmoBone");
            this.spPrefabAmmoPosition = this.serializedObject.FindProperty("prefabAmmoPosition");
            this.spPrefabAmmoRotation = this.serializedObject.FindProperty("prefabAmmoRotation");

            this.spAudioShoot = this.serializedObject.FindProperty("audioShoot");
            this.spAudioEmpty = this.serializedObject.FindProperty("audioEmpty");
            this.spAudioReload = this.serializedObject.FindProperty("audioReload");

            this.spAnimationShoot = this.serializedObject.FindProperty("animationShoot");
            this.spMaskShoot = this.serializedObject.FindProperty("maskShoot");
            this.spAnimationReload = this.serializedObject.FindProperty("animationReload");
            this.spMaskReload = this.serializedObject.FindProperty("maskReload");

            this.spActionsOnStartCharge = this.serializedObject.FindProperty("actionsOnStartCharge");
            this.spActionsOnEndCharge = this.serializedObject.FindProperty("actionsOnEndCharge");
            this.spActionsOnShoot = this.serializedObject.FindProperty("actionsOnShoot");

            this.InitActions(this.spActionsOnStartCharge, PATH_ONSTART);
            this.InitActions(this.spActionsOnEndCharge, PATH_ONEND);
            this.InitActions(this.spActionsOnShoot, PATH_ONSHOOT);
        }

        private void InitActions(SerializedProperty property, string actionsPath)
        {
            if (property.objectReferenceValue == null)
            {
                string actionsName = Guid.NewGuid().ToString("N");
                GameCreatorUtilities.CreateFolderStructure(actionsPath);
                string path = Path.Combine(actionsPath, string.Format(PREFAB_NAME, actionsName));
                path = AssetDatabase.GenerateUniqueAssetPath(path);

                GameObject sceneInstance = new GameObject(actionsName);
                sceneInstance.AddComponent<Actions>();

                GameObject prefabInstance = PrefabUtility.SaveAsPrefabAsset(sceneInstance, path);
                DestroyImmediate(sceneInstance);

                Actions prefabActions = prefabInstance.GetComponent<Actions>();
                prefabActions.destroyAfterFinishing = true;
                property.objectReferenceValue = prefabActions.actionsList;

                this.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                this.serializedObject.Update();
            }
        }

		// PAINT METHODS: ------------------------------------------------------

		public override void OnInspectorGUI()
		{
            if (this.ammo == null || this.serializedObject == null) return;
			this.serializedObject.Update();

            EditorGUILayout.PropertyField(this.spID, GC_AMID);
			EditorGUILayout.PropertyField(this.spName, GC_NAME);
			EditorGUILayout.PropertyField(this.spDesc, GC_DESC);

			EditorGUILayout.Space();
			this.PaintSectionGeneral();

            GUILayout.Space(SPACING);
            this.PaintSectionAiming();

            GUILayout.Space(SPACING);
			this.PaintSectionCharge();

			GUILayout.Space(SPACING);
			this.PaintSectionShooting();

            GUILayout.Space(SPACING);
            this.PaintSectionAmmo();

            GUILayout.Space(SPACING);
            this.PaintSectionAudio();

            GUILayout.Space(SPACING);
            this.PaintSectionAnimations();

            GUILayout.Space(SPACING);
            this.PaintSectionWarnings();
            GUILayout.Space(SPACING);

            this.PaintActions();

            this.serializedObject.ApplyModifiedProperties();
		}

		private void PaintSectionGeneral()
		{
			this.sectionGeneral.PaintSection();
			using (var group = new EditorGUILayout.FadeGroupScope(this.sectionGeneral.state.faded))
			{
				if (group.visible)
				{
					EditorGUILayout.BeginVertical(CoreGUIStyles.GetBoxExpanded());

					EditorGUILayout.PropertyField(this.spFireRate);

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(this.spInfiniteAmmo);
					EditorGUILayout.PropertyField(this.spClipSize);

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(this.spAutoReload);
                    EditorGUILayout.PropertyField(this.spReloadDuration);

                    EditorGUILayout.EndVertical();
				}
			}
		}

		private void PaintSectionCharge()
		{
			this.sectionCharge.PaintSection();
			using (var group = new EditorGUILayout.FadeGroupScope(this.sectionCharge.state.faded))
			{
				if (group.visible)
				{
					EditorGUILayout.BeginVertical(CoreGUIStyles.GetBoxExpanded());

                    EditorGUILayout.PropertyField(this.spChargeType);

                    EditorGUI.BeginDisabledGroup((this.spChargeType.intValue & 2) == 0);
                    EditorGUILayout.PropertyField(this.spMinChargeTime);
                    EditorGUILayout.PropertyField(this.spChargeTime);

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(this.spChargeValue);
                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.EndVertical();
				}
			}
		}

		private void PaintSectionAiming()
		{
			this.sectionAiming.PaintSection();
			using (var group = new EditorGUILayout.FadeGroupScope(this.sectionAiming.state.faded))
			{
				if (group.visible)
				{
					EditorGUILayout.BeginVertical(CoreGUIStyles.GetBoxExpanded());

                    EditorGUILayout.PropertyField(this.spAimingMode);

                    EditorGUI.indentLevel++;
                    switch (this.spAimingMode.enumValueIndex)
                    {
                        case (int)Ammo.AimType.Crosshair:
                            EditorGUILayout.PropertyField(this.spCrosshair);
                            EditorGUILayout.PropertyField(this.spCrosshairFocusTime, GC_FOCUS);
                            EditorGUILayout.PropertyField(this.spProjectileVelocity);
                            break;

                        case (int)Ammo.AimType.Trajectory:
                            EditorGUILayout.PropertyField(this.spTrajectory, true);
                            break;
                    }
                    EditorGUI.indentLevel--;

                    EditorGUILayout.EndVertical();
				}
			}
		}

		private void PaintSectionShooting()
		{
			this.sectionShooting.PaintSection();
			using (var group = new EditorGUILayout.FadeGroupScope(this.sectionShooting.state.faded))
			{
				if (group.visible)
				{
					EditorGUILayout.BeginVertical(CoreGUIStyles.GetBoxExpanded());

                    EditorGUILayout.PropertyField(this.spShootType);

                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(this.spDistance);

                    switch (this.spShootType.enumValueIndex)
                    {
                        case (int)Ammo.ShootType.Projectile:
                            EditorGUILayout.PropertyField(this.spPrefabMuzzleFlash);
                            EditorGUILayout.PropertyField(this.spPrefabProjectile);
                            break;

                        case (int)Ammo.ShootType.Raycast:
                        case (int)Ammo.ShootType.RaycastAll:
                        case (int)Ammo.ShootType.TrajectoryCast:
                            EditorGUILayout.PropertyField(this.spPrefabMuzzleFlash);
                            EditorGUILayout.PropertyField(this.spPrefabImpactEffect);
                            EditorGUILayout.PropertyField(this.spLayerMask);
                            EditorGUILayout.PropertyField(this.spTriggersMask);
                            EditorGUILayout.PropertyField(this.spPushForce);
                            EditorGUILayout.PropertyField(this.spShootTrail, GC_TRAIL);
                            break;

                        case (int)Ammo.ShootType.SphereCast:
                        case (int)Ammo.ShootType.SphereCastAll:
                            EditorGUILayout.PropertyField(this.spRadius);
                            EditorGUILayout.PropertyField(this.spPrefabMuzzleFlash);
                            EditorGUILayout.PropertyField(this.spPrefabImpactEffect);
                            EditorGUILayout.PropertyField(this.spLayerMask);
                            EditorGUILayout.PropertyField(this.spTriggersMask);
                            EditorGUILayout.PropertyField(this.spPushForce);
                            EditorGUILayout.PropertyField(this.spShootTrail, GC_TRAIL);
                            break;
                    }

                    EditorGUI.indentLevel--;

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(this.spRecoil);
                    EditorGUILayout.PropertyField(this.spDelay);

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(this.spMinSpread);

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(this.spMaxSpread);

                    EditorGUILayout.EndVertical();
				}
			}
		}

        private void PaintSectionAmmo()
        {
            this.sectionAmmoRepr.PaintSection();
            using (var group = new EditorGUILayout.FadeGroupScope(this.sectionAmmoRepr.state.faded))
            {
                if (group.visible)
                {
                    EditorGUILayout.BeginVertical(CoreGUIStyles.GetBoxExpanded());

                    EditorGUILayout.PropertyField(this.spPrefabAmmo);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(this.spPrefabAmmoBone, GC_BONE);
                    EditorGUI.indentLevel--;

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(this.spPrefabAmmoPosition, GC_POS);
                    EditorGUILayout.PropertyField(this.spPrefabAmmoRotation, GC_ROT);
                    
                    EditorGUILayout.EndVertical();
                }
            }
        }

        private void PaintSectionAudio()
        {
            this.sectionAudio.PaintSection();
            using (var group = new EditorGUILayout.FadeGroupScope(this.sectionAudio.state.faded))
            {
                if (group.visible)
                {
                    EditorGUILayout.BeginVertical(CoreGUIStyles.GetBoxExpanded());

                    EditorGUILayout.PropertyField(this.spAudioShoot);
                    EditorGUILayout.PropertyField(this.spAudioEmpty);
                    EditorGUILayout.PropertyField(this.spAudioReload);

                    EditorGUILayout.EndVertical();
                }
            }
        }

        private void PaintSectionAnimations()
        {
            this.sectionAnimations.PaintSection();
            using (var group = new EditorGUILayout.FadeGroupScope(this.sectionAnimations.state.faded))
            {
                if (group.visible)
                {
                    EditorGUILayout.BeginVertical(CoreGUIStyles.GetBoxExpanded());

                    EditorGUILayout.PropertyField(this.spAnimationShoot);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(this.spMaskShoot);
                    EditorGUI.indentLevel--;

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(this.spAnimationReload);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(this.spMaskReload);
                    EditorGUI.indentLevel--;

                    EditorGUILayout.EndVertical();
                }
            }
        }

        private void PaintSectionWarnings()
        {
            if (this.spAimingMode.enumValueIndex == (int)Ammo.AimType.Crosshair &&
                this.spCrosshair.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox(ERR_MISS_CROSS, MessageType.Error);
            }

            if (this.spShootType.enumValueIndex == (int)Ammo.ShootType.Projectile &&
                this.spPrefabProjectile.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox(ERR_MISS_PROJ, MessageType.Error);
            }
        }

        private void PaintActions()
        {
            if (this.actionsOnStartCharge == null)
            {
                this.actionsOnStartCharge = Editor.CreateEditor(
                    this.spActionsOnStartCharge.objectReferenceValue
                ) as IActionsListEditor;
            }

            if (this.actionsOnEndCharge == null)
            {
                this.actionsOnEndCharge = Editor.CreateEditor(
                    this.spActionsOnEndCharge.objectReferenceValue
                ) as IActionsListEditor;
            }

            if (this.actionsOnShoot == null)
            {
                this.actionsOnShoot = Editor.CreateEditor(
                    this.spActionsOnShoot.objectReferenceValue
                ) as IActionsListEditor;
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            this.toolbarIndex = GUILayout.Toolbar(this.toolbarIndex, OPTIONS_ACTIONS);
            GUILayout.Space(SPACING);

            switch (this.toolbarIndex)
            {
                case 0: this.actionsOnStartCharge.OnInspectorGUI(); break;
                case 1: this.actionsOnShoot.OnInspectorGUI(); break;
                case 2: this.actionsOnEndCharge.OnInspectorGUI(); break;
            }
        }
    }
}
