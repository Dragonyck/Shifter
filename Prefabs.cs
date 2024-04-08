using System;
using System.Reflection;
using System.Collections.Generic;
using BepInEx;
using R2API;
using R2API.Utils;
using EntityStates;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;
using KinematicCharacterController;
using BepInEx.Configuration;
using RoR2.UI;
using UnityEngine.UI;
using System.Security;
using System.Security.Permissions;
using System.Linq;
using R2API.ContentManagement;
using UnityEngine.AddressableAssets;
using Rewired.ComponentControls.Effects;
using UnityEngine.Rendering.PostProcessing;
using RoR2.Audio;

namespace Shifter
{
    class Prefabs
    {
        internal static GameObject crosshair;
        internal static GameObject trackingOrbProjectile;
        internal static GameObject trackingOrbProjectileGhost;
        internal static GameObject trackingOrbImpactEffect;
        internal static GameObject freezingWaveProjectile;
        internal static GameObject freezingWaveProjectileGhost;
        internal static GameObject itemPanelChargeEffect;
        internal static GameObject itemPanelMuzzleEffect;
        internal static GameObject executeLineEffect;
        internal static GameObject executeLineImpactEffect;
        internal static GameObject traversePP;
        internal static GameObject traversePPPermanent;
        internal static GameObject instaKillPP;
        internal static GameObject swingEffect;
        internal static GameObject itemSelectionPanel;
        internal static GameObject aimIndicator;
        internal static GameObject insteaDeathProjectile;
        internal static GameObject insteaDeathProjectileGhost;
        internal static GameObject insteaDeathProjectileImpactEffect;
        internal static BuffDef gigaSpeedBuff;
        internal static BuffDef gigaSpeedBuffAlt;
        internal static Material ghostEffectMat;
        internal static Material redOverlayMat;
        internal static Material instaKillMat;
        internal static Sprite texUnlockIcon;
        internal static List<PickupIndex> allItems = new List<PickupIndex>();
        internal static PickupPickerController.Option[] allOptions;
        internal static T Load<T>(string path)
        {
            return Addressables.LoadAssetAsync<T>(path).WaitForCompletion();
        }
        [SystemInitializer(typeof(ItemCatalog))]
        public static void SetAllAvailableItems()
        {
            List<PickupIndex> tier1 = new List<PickupIndex>();
            List<PickupIndex> voidtier1 = new List<PickupIndex>();
            List<PickupIndex> tier2 = new List<PickupIndex>();
            List<PickupIndex> voidtier2 = new List<PickupIndex>();
            List<PickupIndex> tier3 = new List<PickupIndex>();
            List<PickupIndex> voidtier3 = new List<PickupIndex>();
            List<PickupIndex> lunar = new List<PickupIndex>();
            List<PickupIndex> boss = new List<PickupIndex>();
            List<PickupIndex> voidboss = new List<PickupIndex>();
            List<PickupIndex> modded = new List<PickupIndex>();
            List<PickupIndex> equip = new List<PickupIndex>();
            List<PickupIndex> equiplunar = new List<PickupIndex>();
            List<PickupIndex> equipboss = new List<PickupIndex>();

            Sprite nullIcon = Load<Sprite>("RoR2/Base/Core/texNullIcon.png");

            for (int i = 0; i < ItemCatalog.allItemDefs.Length; i++)
            {
                var itemDef = ItemCatalog.allItemDefs[i];
                if (!itemDef.hidden)// && itemDef.DoesNotContainTag(ItemTag.WorldUnique)
                {
                    List<PickupIndex> list = null;
                    switch (itemDef.tier)
                    {
                        case ItemTier.Tier1:
                            list = tier1;
                            break;
                        case ItemTier.Tier2:
                            list = tier2;
                            break;
                        case ItemTier.Tier3:
                            list = tier3;
                            break;
                        case ItemTier.VoidTier1:
                            list = voidtier1;
                            break;
                        case ItemTier.VoidTier2:
                            list = voidtier2;
                            break;
                        case ItemTier.VoidTier3:
                            list = voidtier3;
                            break;
                        case ItemTier.Lunar:
                            list = lunar;
                            break;
                        case ItemTier.Boss:
                            list = boss;
                            break;
                        case ItemTier.VoidBoss:
                            list = voidboss;
                            break;
                        case ItemTier.AssignedAtRuntime:
                            list = modded;
                            break;
                    }
                    switch (itemDef.deprecatedTier)
                    {
                        case ItemTier.Tier1:
                            list = tier1;
                            break;
                        case ItemTier.Tier2:
                            list = tier2;
                            break;
                        case ItemTier.Tier3:
                            list = tier3;
                            break;
                        case ItemTier.VoidTier1:
                            list = voidtier1;
                            break;
                        case ItemTier.VoidTier2:
                            list = voidtier2;
                            break;
                        case ItemTier.VoidTier3:
                            list = voidtier3;
                            break;
                        case ItemTier.Lunar:
                            list = lunar;
                            break;
                        case ItemTier.Boss:
                            list = boss;
                            break;
                        case ItemTier.VoidBoss:
                            list = voidboss;
                            break;
                        case ItemTier.AssignedAtRuntime:
                            list = modded;
                            break;
                    }
                    var name = Language.GetString(itemDef.nameToken);
                    if (!name.IsNullOrWhiteSpace() && name != itemDef.nameToken && itemDef.pickupIconSprite != nullIcon && itemDef.pickupIconSprite != null)
                    {
                        var pickup = PickupCatalog.FindPickupIndex(itemDef.itemIndex);
                        if (list != null && !list.Contains(pickup))
                        {
                            list.Add(pickup);
                        }
                    }
                }
            }
            for (int i = 0; i < EquipmentCatalog.equipmentDefs.Length; i++)
            {
                var equipDef = EquipmentCatalog.equipmentDefs[i];
                if (equipDef.canDrop)
                {
                }
                List<PickupIndex> list = null;
                if (equipDef.isLunar)
                {
                    list = equiplunar;
                }
                else if (equipDef.isBoss)
                {
                    list = equipboss;
                }
                else
                {
                    list = equip;
                }
                var name = Language.GetString(equipDef.nameToken);
                if (!name.IsNullOrWhiteSpace() && name != equipDef.nameToken && equipDef.pickupIconSprite != nullIcon && equipDef.pickupIconSprite != null)
                {
                    var pickup = PickupCatalog.FindPickupIndex(equipDef.equipmentIndex);
                    if (list != null && !list.Contains(pickup))
                    {
                        list.Add(pickup);
                    }
                }
            }
            allItems.AddRange(tier1);
            allItems.AddRange(tier2);
            allItems.AddRange(tier3);
            allItems.AddRange(lunar);
            allItems.AddRange(boss);
            allItems.AddRange(voidtier1);
            allItems.AddRange(voidtier2);
            allItems.AddRange(voidtier3);
            allItems.AddRange(voidboss);
            allItems.AddRange(modded);
            allItems.AddRange(equip);
            allItems.AddRange(equiplunar);
            allItems.AddRange(equipboss);

            allOptions = PickupPickerController.GenerateOptionsFromArray(allItems.ToArray());

            var gridRect = itemSelectionPanel.GetComponentInChildren<GridLayoutGroup>().GetComponent<RectTransform>();
            gridRect.sizeDelta += Vector2.up * allItems.Count * 14.5f;
            gridRect.localPosition = Vector2.up * allItems.Count * -25.7f;

            UnityEngine.Object.Destroy(freezingWaveProjectileGhost.GetComponentInChildren<RotateAroundAxis>(true));
        }
        internal static void CreatePrefabs()
        {
            /*foreach (HG.GeneralSerializer.SerializedField f in Load<EntityStateConfiguration>("RoR2/DLC1/VoidSurvivor/EntityStates.VoidSurvivor.Weapon.FireMegaBlasterBig.asset").serializedFieldsCollection.serializedFields)
            {
                Debug.LogWarning(f.fieldName + ": " + f.fieldValue.stringValue);
            }*/

            texUnlockIcon = Load<Sprite>("RoR2/Base/Common/MiscIcons/texUnlockIcon.png");

            redOverlayMat = Load<Material>("RoR2/DLC1/VoidSurvivor/matVoidSurvivorCorruptOverlay.mat");

            ghostEffectMat = new Material(Load<Material>("RoR2/Base/Common/VFX/matGhostEffect.mat"));
            ghostEffectMat.SetColor("_TintColor", new Color(0.31372f, 0, 0));
            ghostEffectMat.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampParentFire.png"));

            instaKillMat = new Material(Load<Material>("RoR2/Base/Parent/matParentBlink.mat"));
            instaKillMat.SetColor("_TintColor", Color.red);
            instaKillMat.SetTexture("_MainTex", Load<Texture2D>("RoR2/Base/Common/texCloudWaterFoam1.jpg"));
            instaKillMat.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampGolem.png"));

            gigaSpeedBuff = Utils.NewBuffDef("Dimensional Ascension", false, true, null, Color.white);//Load<Sprite>("RoR2/DLC1/VoidSurvivor/texBuffVoidSurvivorCorruptionIcon.tif")
            gigaSpeedBuffAlt = Utils.NewBuffDef("Dimensional Ascension", false, true, null, Color.white);//Load<Sprite>("RoR2/DLC1/VoidSurvivor/texBuffVoidSurvivorCorruptionIcon.tif")

            aimIndicator = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Huntress/HuntressTrackingIndicator.prefab"), "ShifterAimIndicator", false);
            foreach (SpriteRenderer s in aimIndicator.GetComponentsInChildren<SpriteRenderer>())
            {
                float tH, tS, tV;
                Color.RGBToHSV(Color.red, out tH, out tS, out tV);

                float H, S, V;
                Color.RGBToHSV(s.color, out H, out S, out V);

                Color newColor = Color.HSVToRGB(tH, S, V);
                newColor.a = s.color.a;

                s.color = newColor;
            }

            swingEffect = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorMeleeSlash2.prefab"), "ShifterSwingEffect", false);
            var swingParticle = swingEffect.GetComponentInChildren<ParticleSystemRenderer>();
            swingParticle.material = Load<Material>("RoR2/DLC1/VoidSurvivor/matVoidSurvivorLightningCorrupted.mat");
            swingEffect.GetComponentInChildren<DestroyOnTimer>().duration = 0.8f;

            crosshair = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorCrosshair.prefab"), "ShifterCrosshair", false);
            crosshair.GetComponentsInChildren<Image>().ForEachTry(x => x.color = new Color(0.87450f, 0, 0));

            traversePP = new GameObject("ShifterPP");
            traversePP.layer = 20;
            traversePP.AddComponent<NetworkIdentity>();
            traversePP.transform.parent = crosshair.transform.parent;
            var tPP = traversePP.AddComponent<PostProcessVolume>();
            tPP.profile = UnityEngine.Object.Instantiate(Load<PostProcessProfile>("RoR2/Base/title/ppLocalDoppelganger.asset"));
            tPP.sharedProfile = tPP.profile;
            tPP.weight = 1;
            tPP.priority = 99;
            tPP.isGlobal = true;
            var colorGrading = (ColorGrading)tPP.profile.settings[1];
            colorGrading.tint.overrideState = false;
            colorGrading.colorFilter.overrideState = true;
            colorGrading.colorFilter.value = new Color(0.38039f, 0.11764f, 0.10980f);
            colorGrading.mixerRedOutRedIn.value = 300;
            var ppDuration = traversePP.AddComponent<PostProcessDuration>();
            ppDuration.ppVolume = tPP;
            ppDuration.ppWeightCurve = AnimationCurve.Linear(0, 1, 1, 0);
            ppDuration.ppWeightCurve.AddKey(0.85f, 1);
            ppDuration.maxDuration = 11;
            ppDuration.destroyOnEnd = true;
            PrefabAPI.RegisterNetworkPrefab(traversePP);

            traversePPPermanent = PrefabAPI.InstantiateClone(traversePP, "ShifterPPPermanent", false);
            UnityEngine.Object.Destroy(traversePPPermanent.GetComponent<PostProcessDuration>());

            instaKillPP = PrefabAPI.InstantiateClone(traversePP, "ShifterPPPInstakill", false);
            UnityEngine.Object.Destroy(instaKillPP.GetComponent<PostProcessDuration>());
            var tPP2 = instaKillPP.AddComponent<PostProcessVolume>();
            tPP2.profile = UnityEngine.Object.Instantiate(Load<PostProcessProfile>("RoR2/Base/title/ppLocalElectricWorm.asset"));
            tPP2.sharedProfile = tPP2.profile;
            tPP2.weight = 1;
            tPP2.priority = 99;
            tPP2.isGlobal = true;
            var colorGrading2 = (ColorGrading)tPP2.profile.settings[1];
            colorGrading2.temperature.value = 130;
            colorGrading2.colorFilter.value = new Color(1, 0.29803f, 0.33333f);

            itemPanelChargeEffect = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorChargeMegaBlaster.prefab"), "ShifterItemPanelChargeEffect", false);
            var panelChargeMesh = itemPanelChargeEffect.GetComponentInChildren<MeshRenderer>();
            panelChargeMesh.sharedMaterials[1] = new Material(panelChargeMesh.sharedMaterials[1]);
            panelChargeMesh.sharedMaterials[1].SetColor("_TintColor", new Color(0.78431f, 0, 0));
            foreach (ParticleSystemRenderer r in itemPanelChargeEffect.GetComponentsInChildren<ParticleSystemRenderer>())
            {
                r.material = new Material(r.material);
                r.material.DisableKeyword("VERTEXCOLOR");
                r.material.SetColor("_TintColor", new Color(0.78431f, 0, 0));
            }

            itemPanelMuzzleEffect = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorCrushHealthMuzzleflash.prefab"), "ShifterItemPanelMuzzleEffect", false);
            foreach (ParticleSystemRenderer r in itemPanelMuzzleEffect.GetComponentsInChildren<ParticleSystemRenderer>())
            {
                r.material = new Material(r.material);
                r.material.DisableKeyword("VERTEXCOLOR");
                r.material.SetColor("_TintColor", new Color(0.78431f, 0, 0));
            }
            Utils.RegisterEffect(itemPanelMuzzleEffect, -1);

            itemSelectionPanel = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Command/CommandPickerPanel.prefab"), "ShifterPickerPanel", false);
            itemSelectionPanel.GetComponentInChildren<LanguageTextMeshController>().gameObject.SetActive(false);
            var grid = itemSelectionPanel.GetComponentInChildren<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 6;
            grid.childAlignment = TextAnchor.UpperCenter;
            var gridRect = grid.GetComponent<RectTransform>();
            gridRect.GetComponent<RawImage>().enabled = false;
            gridRect.sizeDelta = Vector2.right * 444;
            gridRect.localPosition = Vector2.up * 100;
            gridRect.anchorMin = new Vector2(0.5f, 1);
            gridRect.anchorMax = new Vector2(0.5f, 1);
            var scrollObject = new GameObject("Scroll", typeof(RectTransform));
            scrollObject.gameObject.layer = 5;
            scrollObject.transform.SetParent(grid.transform.parent);
            scrollObject.transform.localPosition = grid.transform.localPosition;
            grid.transform.parent = scrollObject.transform;
            var scrollRect = scrollObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.content = gridRect;
            scrollRect.scrollSensitivity = 30;
            var pickerPanel = itemSelectionPanel.GetComponent<PickupPickerPanel>();
            var panelBehaviour = itemSelectionPanel.AddComponent<ShifterPanelBehaviour>();
            foreach (Image i in itemSelectionPanel.GetComponentsInChildren<Image>())
            {
                var name = i.name;
                if (name == "SpinnyOutlines")
                {
                    if (i.transform.childCount == 0)
                    {
                        i.color = new Color(0.43529f, 0, 0.05098f);
                    }
                    else
                    {
                        i.color = new Color(0.63529f, 0, 0.07450f);
                    }
                }
                if (name == "BG")
                {
                    i.color = Color.red;
                }
                if (name == "ColoredOverlay")
                {
                    i.color = new Color(0.63529f, 0, 0.07450f);
                }
                if (name == "ColoredOverlay, Subtle")
                {
                    i.color = Color.clear;
                }
                if (name == "CancelButton")
                {
                    panelBehaviour.cancelButton = i.GetComponent<MPButton>();
                }
            }
            var image = scrollObject.gameObject.AddComponent<Image>();
            image.color = new Color(0.12549f, 0, 0);
            image.sprite = Load<Sprite>("RoR2/Base/UI/texUIHighlightBoxOutlineThickIcon.png");
            image.type = Image.Type.Tiled;
            scrollObject.gameObject.AddComponent<Mask>();
            var rectT = scrollRect.GetComponent<RectTransform>();
            rectT.localPosition = Vector2.zero;
            rectT.sizeDelta = new Vector2(500, 510);
            panelBehaviour.gridlayoutGroup = pickerPanel.gridlayoutGroup;
            panelBehaviour.buttonContainer = pickerPanel.buttonContainer;
            panelBehaviour.buttonPrefab = pickerPanel.buttonPrefab;
            panelBehaviour.coloredImages = pickerPanel.coloredImages;
            panelBehaviour.darkColoredImages = pickerPanel.darkColoredImages;
            UnityEngine.Object.Destroy(pickerPanel);

            trackingOrbImpactEffect = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/VoidBarnacle/VoidBarnacleImpactExplosion.prefab"), "ShifterTrackingOrbImpactEffect", false);
            trackingOrbImpactEffect.GetComponentInChildren<Light>().color = Color.red;
            foreach (ParticleSystemRenderer r in trackingOrbImpactEffect.GetComponentsInChildren<ParticleSystemRenderer>())
            {
                var name = r.name;
                if (name == "BillboardFire" || name == "Flames")
                {
                    r.enabled = false;
                }
                else
                {
                    r.material = new Material(r.material);
                    r.material.DisableKeyword("VERTEXCOLOR");
                    r.material.SetColor("_TintColor", new Color(0.78431f, 0, 0));
                }
            }
            Utils.RegisterEffect(trackingOrbImpactEffect, -1, "Play_item_void_chainLightning", false, false, true);

            trackingOrbProjectileGhost = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/LunarSkillReplacements/LunarNeedleGhost.prefab"), "ShifterTrackingOrbProjectileGhost", false);
            trackingOrbProjectileGhost.GetComponentInChildren<TrailRenderer>().material = Load<Material>("RoR2/DLC1/VoidSurvivor/matVoidBlinkTrailCorrupted.mat");
            var trackingOrbparticles = trackingOrbProjectileGhost.GetComponentsInChildren<ParticleSystemRenderer>();
            trackingOrbparticles[0].transform.localScale = Vector3.one * 3;
            trackingOrbparticles[0].material = Load<Material>("RoR2/DLC1/VoidSurvivor/matVoidSurvivorBlasterCoreCorrupted.mat");
            trackingOrbparticles[1].material = new Material(trackingOrbparticles[1].material);
            trackingOrbparticles[1].material.DisableKeyword("VERTEXCOLOR");
            trackingOrbparticles[1].material.SetColor("_TintColor", Color.red);

            trackingOrbProjectile = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/LunarSkillReplacements/LunarNeedleProjectile.prefab"), "ShifterTrackingOrbProjectile", true);
            UnityEngine.Object.Destroy(trackingOrbProjectile.GetComponent<ProjectileSingleTargetImpact>());
            UnityEngine.Object.Destroy(trackingOrbProjectile.GetComponent<ProjectileStickOnImpact>());
            var trackingOrbController = trackingOrbProjectile.GetComponent<ProjectileController>();
            trackingOrbController.ghostPrefab = trackingOrbProjectileGhost;
            trackingOrbController.flightSoundLoop = Load<LoopSoundDef>("RoR2/DLC1/VoidSurvivor/lsdVoidMegaBlasterFlight.asset");
            trackingOrbController.canImpactOnTrigger = true;
            trackingOrbProjectile.GetComponent<ProjectileSteerTowardTarget>().rotationSpeed = 315;
            var orbSimple = trackingOrbProjectile.GetComponent<ProjectileSimple>();
            orbSimple.enabled = false;
            orbSimple.desiredForwardSpeed = MainPlugin.primarySpeed.Value;
            orbSimple.lifetime = 12;
            trackingOrbProjectile.AddComponent<DelayedProjectile>();
            var orbImpact = trackingOrbProjectile.GetComponent<ProjectileImpactExplosion>();
            orbImpact.impactEffect = trackingOrbImpactEffect;
            orbImpact.blastRadius = 2f;
            orbImpact.blastDamageCoefficient = 1;
            orbImpact.timerAfterImpact = false;
            orbImpact.destroyOnEnemy = true;
            orbImpact.destroyOnWorld = false;
            orbImpact.lifetime = 12;
            trackingOrbProjectile.GetComponent<SphereCollider>().isTrigger = true;
            ContentAddition.AddProjectile(trackingOrbProjectile);

            freezingWaveProjectileGhost = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/PrimarySkillShuriken/ShurikenGhost.prefab"), "ShifterFreezingWaveGhost", false);
            var child = freezingWaveProjectileGhost.transform.GetChild(0);
            child.localScale = MainPlugin.freezeProjectileEffectSize.Value;
            child.localRotation = Quaternion.Euler(0, -90, 0);
            UnityEngine.Object.Destroy(freezingWaveProjectileGhost.GetComponentInChildren<SetRandomRotation>());
            UnityEngine.Object.Destroy(freezingWaveProjectileGhost.GetComponentInChildren<RotateAroundAxis>(true));
            freezingWaveProjectileGhost.GetComponentsInChildren<MeshRenderer>().ForEachTry(x => x.enabled = false);
            var freezeTrail = freezingWaveProjectileGhost.GetComponentInChildren<TrailRenderer>(true);
            freezeTrail.gameObject.SetActive(true);
            freezeTrail.material = Load<Material>("RoR2/DLC1/VoidSurvivor/matVoidBlinkTrailCorrupted.mat");
            freezeTrail.textureMode = LineTextureMode.Stretch;
            freezeTrail.widthMultiplier = 0;//30
            var freezeTrail2 = UnityEngine.Object.Instantiate(freezeTrail);
            freezeTrail2.transform.parent = freezeTrail.transform.parent;
            freezeTrail2.transform.localRotation = Quaternion.Euler(90, 0, 0);
            freezeTrail2.transform.localPosition = Vector3.up * 2;
            freezeTrail2.widthMultiplier = 14;
            var freezeTrail3 = UnityEngine.Object.Instantiate(freezeTrail);
            freezeTrail3.transform.parent = freezeTrail.transform.parent;
            freezeTrail3.transform.localRotation = Quaternion.Euler(90, 0, 0);
            freezeTrail3.transform.localPosition = Vector3.up * -2;
            freezeTrail3.widthMultiplier = 14;
            var freezeParticles = freezingWaveProjectileGhost.GetComponentsInChildren<ParticleSystemRenderer>();
            freezeParticles[0].material = new Material(Load<Material>("RoR2/DLC1/VoidMegaCrab/matVoidCrabAntiMatterParticleBillboard.mat"));
            freezeParticles[0].material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampImp2.png"));
            freezeParticles[0].material.DisableKeyword("VERTEXCOLOR");
            freezeParticles[0].material.SetColor("_TintColor", Color.red);
            freezeParticles[1].enabled = false;
            var main = freezeParticles[0].GetComponent<ParticleSystem>();
            var bR = main.startRotation3D; bR.Set(0, 0, 0);
            var rot = main.rotationOverLifetime;rot.enabled = false;

            freezingWaveProjectile = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Commando/FMJRamping.prefab"), "ShifterFreezingWaveProjectile", true);
            freezingWaveProjectile.GetComponent<ProjectileDamage>().damageType = DamageType.Freeze2s;
            var dbppSimple = freezingWaveProjectile.GetComponent<ProjectileSimple>();
            dbppSimple.desiredForwardSpeed = MainPlugin.secondarySpeed.Value;
            dbppSimple.lifetime = 15;//1
            var freezingController = freezingWaveProjectile.GetComponent<ProjectileController>();
            freezingController.ghostPrefab = freezingWaveProjectileGhost;
            freezingController.flightSoundLoop = Load<LoopSoundDef>("RoR2/DLC1/VoidJailer/lsdJailerDart.asset");
            var dbpOverlap = freezingWaveProjectile.GetComponent<ProjectileOverlapAttack>();
            dbpOverlap.impactEffect = Load<GameObject>("RoR2/Base/Mage/MageIceExplosion.prefab");
            freezingWaveProjectile.GetComponent<HitBoxGroup>().hitBoxes[0].gameObject.transform.localScale = MainPlugin.freezeProjectileHitboxSize.Value;
            ContentAddition.AddProjectile(freezingWaveProjectile);

            executeLineImpactEffect = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Nullifier/NullifierExplosion.prefab"), "ShifterExecuteLineImpactEffect", false);
            var executeMesh = executeLineImpactEffect.GetComponentInChildren<MeshRenderer>();
            Material matNullifierGemPortal = new Material(Load<Material>("RoR2/Base/Nullifier/matNullifierGemPortal.mat"));
            matNullifierGemPortal.SetColor("_Color", Color.red);
            Material matGravsphereCore = new Material(Load<Material>("RoR2/Base/Common/VFX/matGravsphereCore.mat"));
            matGravsphereCore.SetColor("_TintColor", Color.red);
            executeMesh.sharedMaterials = new Material[] { matNullifierGemPortal, matGravsphereCore };
            foreach (ParticleSystemRenderer r in executeLineImpactEffect.GetComponentsInChildren<ParticleSystemRenderer>())
            {
                r.material = new Material(r.material);
                r.material.DisableKeyword("VERTEXCOLOR");
                r.material.SetColor("_TintColor", Color.red);
            }
            Utils.RegisterEffect(executeLineImpactEffect, -1, "Play_nullifier_death_vortex_explode", false, false, true);

            executeLineEffect = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/SiphonOnLowHealth/SiphonTetherVFX.prefab"), "ShifterExecuteLineEffect", false);
            UnityEngine.Object.Destroy(executeLineEffect.GetComponent<EffectComponent>());
            UnityEngine.Object.Destroy(executeLineEffect.GetComponent<VFXAttributes>());
            UnityEngine.Object.Destroy(executeLineEffect.GetComponent<NetworkIdentity>());
            UnityEngine.Object.Destroy(executeLineEffect.GetComponent<TetherVfx>());
            executeLineEffect.GetComponent<LineRenderer>().material = Load<Material>("RoR2/DLC1/VoidSurvivor/matVoidBlinkTrailCorrupted.mat");
            var executeAlpha = executeLineEffect.AddComponent<DelayedAlpha>();
            executeAlpha.alpha = executeLineEffect.GetComponents<AnimateShaderAlpha>()[1];
            executeAlpha.curve = executeLineEffect.GetComponent<BezierCurveLine>();

            insteaDeathProjectileImpactEffect = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorMegaBlasterExplosionCorrupted.prefab"), "ShifterInsteaDeathProjectileImpactEffect", false);
            foreach (ParticleSystemRenderer r in insteaDeathProjectileImpactEffect.GetComponentsInChildren<ParticleSystemRenderer>())
            {
                var name = r.name;
                if (name == "ExplosionSphere, Stars")
                {
                    r.transform.localScale = Vector3.one * 2;
                }
                if (name == "ScaledHitsparks 2")
                {
                    r.material = new Material(r.material);
                    r.material.DisableKeyword("VERTEXCOLOR");
                    r.material.SetColor("_TintColor", new Color(0.23529f, 0, 0));
                }
            }
            ContentAddition.AddEffect(insteaDeathProjectileImpactEffect);

            insteaDeathProjectileGhost = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorMegaBlasterBigGhostCorrupted.prefab"), "ShifterInsteaDeathProjectileGhost", false);
            var instaTrail = insteaDeathProjectileGhost.GetComponentInChildren<TrailRenderer>();
            instaTrail.material = new Material(instaTrail.material);
            instaTrail.material.DisableKeyword("VERTEXCOLOR");
            instaTrail.material.SetColor("_TintColor", new Color(0.23529f, 0, 0));
            foreach (ParticleSystemRenderer r in insteaDeathProjectileGhost.GetComponentsInChildren<ParticleSystemRenderer>())
            {
                var name = r.name;
                if (name == "SoftGlow")
                {
                    r.material = new Material(instaTrail.material);
                    r.material.SetColor("_TintColor", new Color(0.23529f, 0, 0));
                }
                if (name == "Fire")
                {
                    r.material = Load<Material>("RoR2/DLC1/VoidSurvivor/matVoidSurvivorCrabCannonCore.mat");
                }
            }
            
            insteaDeathProjectile = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorMegaBlasterBigProjectileCorrupted.prefab"), "ShifterInsteaDeathProjectile", true);
            UnityEngine.Object.Destroy(insteaDeathProjectile.GetComponent<AntiGravityForce>());
            insteaDeathProjectile.GetComponent<Rigidbody>().useGravity = false;
            insteaDeathProjectile.GetComponent<ProjectileController>().ghostPrefab = insteaDeathProjectileGhost;
            var idPE = insteaDeathProjectile.GetComponent<ProjectileImpactExplosion>();
            idPE.impactEffect = insteaDeathProjectileImpactEffect;
            idPE.blastRadius = MainPlugin.secondaryAltRadius.Value;
            insteaDeathProjectile.AddComponent<InstaKill>();
            insteaDeathProjectile.GetComponent<ProjectileSimple>().desiredForwardSpeed = MainPlugin.secondaryAltSpeed.Value;
            ContentAddition.AddProjectile(insteaDeathProjectile);
        }
    }
}
