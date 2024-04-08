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

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: HG.Reflection.SearchableAttribute.OptIn]
namespace Shifter
{
    [BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID)]
    [BepInDependency(R2API.LanguageAPI.PluginGUID)]
    [BepInDependency(R2API.LoadoutAPI.PluginGUID)]
    [BepInDependency(R2API.Networking.NetworkingAPI.PluginGUID)]
    [BepInDependency(R2API.PrefabAPI.PluginGUID)]
    [BepInDependency(R2API.SoundAPI.PluginGUID)]
    [BepInDependency(R2API.RecalculateStatsAPI.PluginGUID)]
    [BepInDependency(R2API.DamageAPI.PluginGUID)]
    [BepInDependency(R2API.ItemAPI.PluginGUID)]
    [BepInPlugin(MODUID, MODNAME, VERSION)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class MainPlugin : BaseUnityPlugin
    {
        public const string MODUID = "com.Dragonyck.RealityShifter";
        public const string MODNAME = "RealityShifter";
        public const string VERSION = "1.0.0";
        public const string SURVIVORNAME = "Reality Shifter";
        public const string SURVIVORNAMEKEY = "SHIFTER";
        public static GameObject characterPrefab;
        public static GameObject displayPrefab;
        public static GameObject masterPrefab;
        private static readonly Color characterColor = new Color(0.3773585f, 0, 0);
        public static ConfigEntry<bool> giveItem;
        public static ConfigEntry<bool> enablePP;
        public static ConfigEntry<float> utilSpeed;
        public static ConfigEntry<int> chain;
        public static ConfigEntry<Vector3> freezeProjectileHitboxSize;
        public static ConfigEntry<Vector3> freezeProjectileEffectSize;
        public static ConfigEntry<float> secondaryAltRadius;
        public static ConfigEntry<float> primarySpeed;
        public static ConfigEntry<float> secondarySpeed;
        public static ConfigEntry<float> secondaryAltSpeed;

        private void Awake()
        {
            //On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (self, user, t) => { };
            giveItem = base.Config.Bind<bool>(new ConfigDefinition("Give Items", "Enable"), false, new ConfigDescription("Items received from Reality Warp will be added directly to the inventory instead.", null, Array.Empty<object>()));
            enablePP = base.Config.Bind<bool>(new ConfigDefinition("Enable Shift Post Processing For Alt Utility", "Enable"), false, new ConfigDescription("", null, Array.Empty<object>()));
            utilSpeed = base.Config.Bind<float>(new ConfigDefinition("Utilities Speed Mult", "Value"), 1, new ConfigDescription("", null, Array.Empty<object>()));
            chain = base.Config.Bind<int>(new ConfigDefinition("Chain Lv", "Value"), 10, new ConfigDescription("", null, Array.Empty<object>()));
            freezeProjectileHitboxSize = base.Config.Bind<Vector3>(new ConfigDefinition("Secondary Hitbox Size", "Value"), new Vector3(48, 20, 24), new ConfigDescription("", null, Array.Empty<object>()));
            freezeProjectileEffectSize = base.Config.Bind<Vector3>(new ConfigDefinition("Secondary Effect Size", "Value"), new Vector3(6, 50, 20), new ConfigDescription("", null, Array.Empty<object>()));
            secondaryAltRadius = base.Config.Bind<float>(new ConfigDefinition("Secondary Alt Explosion Radius", "Value"), 10, new ConfigDescription("", null, Array.Empty<object>()));
            primarySpeed = base.Config.Bind<float>(new ConfigDefinition("Primary Projectile Speed", "Value"), 50, new ConfigDescription("", null, Array.Empty<object>()));
            secondarySpeed = base.Config.Bind<float>(new ConfigDefinition("Secondary Projectile Speed", "Value"), 140, new ConfigDescription("", null, Array.Empty<object>()));
            secondaryAltSpeed = base.Config.Bind<float>(new ConfigDefinition("Secondary Alt Projectile Speed", "Value"), 70, new ConfigDescription("", null, Array.Empty<object>()));

            Assets.PopulateAssets();
            Achievements.RegisterUnlockables();
            Prefabs.CreatePrefabs();
            CreatePrefab();
            RegisterStates();
            RegisterCharacter();
            Hook.Hooks();
        }
        internal static void CreatePrefab()
        {
            var bodyObject = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorBody.prefab").WaitForCompletion();

            characterPrefab = PrefabAPI.InstantiateClone(bodyObject, "ShifterBody", true);
            Destroy(characterPrefab.GetComponent<VoidSurvivorController>());
            characterPrefab.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
            characterPrefab.GetComponent<SetStateOnHurt>().canBeFrozen = false;
            characterPrefab.AddComponent<ShifterBehaviour>(); 
            characterPrefab.AddComponent<ShifterTracker>();

            CharacterBody bodyComponent = characterPrefab.GetComponent<CharacterBody>();
            bodyComponent.name = "ShifterBody";
            bodyComponent.baseNameToken = SURVIVORNAMEKEY + "_NAME";
            bodyComponent.subtitleNameToken = SURVIVORNAMEKEY + "_SUBTITLE";
            bodyComponent._defaultCrosshairPrefab = Prefabs.crosshair;
            bodyComponent.portraitIcon = Assets.MainAssetBundle.LoadAsset<Sprite>("portrait").texture;
            bodyComponent.bodyColor = characterColor;

            var modelLocator = characterPrefab.GetComponent<ModelLocator>();
            var characterModel = modelLocator.modelTransform.GetComponent<CharacterModel>();
            characterModel.GetComponentsInChildren<SkinnedMeshRenderer>()[3].gameObject.SetActive(false);
            var renderers = new Renderer[3];
            for (int i = 0; i < characterModel.baseRendererInfos.Length; i++)
            {
                renderers[i] = characterModel.baseRendererInfos[i].renderer;
            }
            characterModel.baseRendererInfos = new CharacterModel.RendererInfo[3];
            for (int i = 0; i < renderers.Length; i++)
            {
                characterModel.baseRendererInfos[i] = new CharacterModel.RendererInfo()
                {
                    renderer = renderers[i],
                    defaultMaterial = Prefabs.redOverlayMat,
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On
                };
            }
            characterModel.GetComponent<ModelSkinController>().skins = new SkinDef[] { LoadoutAPI.CreateNewSkinDef(Utils.CreateNewSkinDefInfo(characterModel.baseRendererInfos, characterModel.gameObject, "Default")) };

            EntityStateMachine mainStateMachine = bodyComponent.GetComponent<EntityStateMachine>();
            mainStateMachine.mainStateType = new SerializableEntityStateType(typeof(GenericCharacterMain));

            Utils.NewStateMachine<Idle>(characterPrefab, "Pick");
            Utils.NewStateMachine<Idle>(characterPrefab, "NoClip");
            Utils.NewStateMachine<Idle>(characterPrefab, "Succ");
            Utils.NewStateMachine<Traverse>(characterPrefab, "Traverse");

            NetworkStateMachine networkStateMachine = bodyComponent.GetComponent<NetworkStateMachine>();
            networkStateMachine.stateMachines = bodyComponent.GetComponents<EntityStateMachine>();

            ContentAddition.AddBody(characterPrefab);
        }
        private void RegisterCharacter()
        {
            string desc = "" +
                "<style=cSub>\r\n\r\n< ! > "
                + Environment.NewLine +
                "<style=cSub>\r\n\r\n< ! > "
                + Environment.NewLine +
                "<style=cSub>\r\n\r\n< ! > "
                + Environment.NewLine +
                "<style=cSub>\r\n\r\n< ! > ";

            string outro = "..and so it left.";
            string fail = "..and so it vanished.";

            LanguageAPI.Add(SURVIVORNAMEKEY + "_NAME", SURVIVORNAME);
            LanguageAPI.Add(SURVIVORNAMEKEY + "_DESCRIPTION", desc);
            LanguageAPI.Add(SURVIVORNAMEKEY + "_SUBTITLE", "");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_OUTRO", outro);
            LanguageAPI.Add(SURVIVORNAMEKEY + "_FAIL", fail);

            displayPrefab = PrefabAPI.InstantiateClone(Prefabs.Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorDisplay.prefab"), "ShifterDisplay", false);
            foreach (AnimateShaderAlpha a in displayPrefab.GetComponentsInChildren<AnimateShaderAlpha>())
            {
                Destroy(a);
            }
            foreach (ParticleSystemRenderer r in displayPrefab.GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                r.material = new Material(r.material);
                r.material.DisableKeyword("VERTEXCOLOR");
                r.material.SetColor("_TintColor", Color.red);
            }
            foreach (Light l in displayPrefab.GetComponentsInChildren<Light>(true))
            {
                float tH, tS, tV;
                Color.RGBToHSV(Color.red, out tH, out tS, out tV);

                float H, S, V;
                Color.RGBToHSV(l.color, out H, out S, out V);

                Color newColor = Color.HSVToRGB(tH, S, V);
                newColor.a = l.color.a;

                l.color = newColor;
            }

            SurvivorDef survivorDef = ScriptableObject.CreateInstance<SurvivorDef>();
            {
                survivorDef.cachedName = SURVIVORNAMEKEY + "_NAME";
                survivorDef.unlockableDef = null;
                survivorDef.descriptionToken = SURVIVORNAMEKEY + "_DESCRIPTION";
                survivorDef.primaryColor = characterColor;
                survivorDef.bodyPrefab = characterPrefab;
                survivorDef.displayPrefab = displayPrefab;
                survivorDef.outroFlavorToken = SURVIVORNAMEKEY + "_OUTRO";
                survivorDef.desiredSortPosition = 0.2f;
                survivorDef.mainEndingEscapeFailureFlavorToken = SURVIVORNAMEKEY + "_FAIL";
            };
            ContentAddition.AddSurvivorDef(survivorDef);

            SkillSetup();

            var characterMaster = PrefabAPI.InstantiateClone(Prefabs.Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorMonsterMaster.prefab"), "ShifterMaster", true);
            Destroy(Array.Find(characterMaster.GetComponents<RoR2.CharacterAI.AISkillDriver>(), x => x.skillSlot == SkillSlot.Special));
            ContentAddition.AddMaster(characterMaster);

            CharacterMaster component = characterMaster.GetComponent<CharacterMaster>();
            component.bodyPrefab = characterPrefab;
        }
        void RegisterStates()
        {
            bool hmm;
            ContentAddition.AddEntityState<BaseShifterState>(out hmm);
            ContentAddition.AddEntityState<AltPrimary>(out hmm);
            ContentAddition.AddEntityState<Primary>(out hmm);
            ContentAddition.AddEntityState<PrimaryEnd>(out hmm);
            ContentAddition.AddEntityState<Secondary>(out hmm);
            ContentAddition.AddEntityState<AltSecondary>(out hmm);
            ContentAddition.AddEntityState<Utility>(out hmm);
            ContentAddition.AddEntityState<Special>(out hmm);
            ContentAddition.AddEntityState<AltSpecial>(out hmm);
            ContentAddition.AddEntityState<Traverse>(out hmm);
            ContentAddition.AddEntityState<FlyState>(out hmm);
        }
        void SkillSetup()
        {
            foreach (GenericSkill obj in characterPrefab.GetComponentsInChildren<GenericSkill>())
            {
                BaseUnityPlugin.DestroyImmediate(obj);
            }
            PassiveSetup();
            PrimarySetup();
            SecondarySetup();
            UtilitySetup();
            SpecialSetup();
        }
        void PassiveSetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();

            LanguageAPI.Add(SURVIVORNAMEKEY + "_PASSIVE_NAME", "Inevitable");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_PASSIVE_DESCRIPTION", "<style=cIsDamage>Immune</style> to <style=cIsUtility>fall damage</style>. All damage dealt applies a random <style=cIsDamage>debuff</style>." + Environment.NewLine +
                "Reality Shifter is <style=cIsDamage>immune</style> to debuffs." + Environment.NewLine +
                "Gain 5% <style=cIsHealth>Max HP</style>, <style=cIsHealing>health regen</style>, <style=cIsHealing>shield</style>, <style=cIsDamage>movement speed</style>, <style=cIsDamage>damage</style>, <style=cIsDamage>attack speed</style>, " +
                "<style=cIsDamage>critical chance</style>, <style=cIsUtility>cooldown reduction</style> and <style=cIsHealing>0.05 armor</style> per level.");

            component.passiveSkill.enabled = true;
            component.passiveSkill.skillNameToken = SURVIVORNAMEKEY + "_PASSIVE_NAME";
            component.passiveSkill.skillDescriptionToken = "<style=cKeywordName>Inevitable</style><style=cSub>";
            component.passiveSkill.icon = Assets.MainAssetBundle.LoadAsset<Sprite>("texVoidSurvivorSkillIcons_1");
            component.passiveSkill.keywordToken = SURVIVORNAMEKEY + "_PASSIVE_DESCRIPTION";
        }
        void PrimarySetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();
            LanguageAPI.Add(SURVIVORNAMEKEY + "_M1", "Cosmic Shrapnel");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_M1_DESCRIPTION", "Fire tracking orbs that deal <style=cIsDamage>250% damage</style>.");

            var SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SkillDef.activationState = new SerializableEntityStateType(typeof(Primary));
            SkillDef.activationStateMachineName = "Weapon";
            SkillDef.baseMaxStock = 0;
            SkillDef.baseRechargeInterval = 0f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = true;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = false;
            SkillDef.cancelSprintingOnActivation = true;
            SkillDef.rechargeStock = 0;
            SkillDef.requiredStock = 0;
            SkillDef.stockToConsume = 0;
            SkillDef.icon = Prefabs.Load<SkillDef>("RoR2/DLC1/VoidSurvivor/FireCorruptDisk.asset").icon;
            SkillDef.skillDescriptionToken = SURVIVORNAMEKEY + "_M1_DESCRIPTION";
            SkillDef.skillName = SURVIVORNAMEKEY + "_M1";
            SkillDef.skillNameToken = SURVIVORNAMEKEY + "_M1";
            ContentAddition.AddSkillDef(SkillDef);
            component.primary = Utils.NewGenericSkill(characterPrefab, SkillDef);

            LanguageAPI.Add(SURVIVORNAMEKEY + "_M1_ALT", "Assimilate");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_M1_ALT_DESCRIPTION", "Absorb an enemy, killing it <style=cIsDamage>instantly</style> and gaining its stats. This effect chains to an additional enemy for every <style=cIsUtility>10 levels</style>.");
            SkillDef = ScriptableObject.CreateInstance<HuntressTrackingSkillDef>();
            SkillDef.activationState = new SerializableEntityStateType(typeof(AltPrimary));
            SkillDef.activationStateMachineName = "Succ";
            SkillDef.baseMaxStock = 0;
            SkillDef.baseRechargeInterval = 0f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = true;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = false;
            SkillDef.cancelSprintingOnActivation = true;
            SkillDef.rechargeStock = 0;
            SkillDef.requiredStock = 0;
            SkillDef.stockToConsume = 0;
            SkillDef.icon = Prefabs.Load<SkillDef>("RoR2/DLC1/VoidSurvivor/FireCorruptBeam.asset").icon;
            SkillDef.skillDescriptionToken = SURVIVORNAMEKEY + "_M1_ALT_DESCRIPTION";
            SkillDef.skillName = SURVIVORNAMEKEY + "_M1_ALT";
            SkillDef.skillNameToken = SURVIVORNAMEKEY + "_M1_ALT";
            ContentAddition.AddSkillDef(SkillDef);
            Utils.AddAlt(component.primary.skillFamily, SkillDef, Achievements.bossKillUnlock);

        }
        void SecondarySetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();
            LanguageAPI.Add(SURVIVORNAMEKEY + "_M2", "Spacetime Blast");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_M2_DESCRIPTION", "Fire a wave attack that deals <style=cIsDamage>400% damage</style> and <style=cIsDamage>freezes</style>.");

            var SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SkillDef.activationState = new SerializableEntityStateType(typeof(Secondary));
            SkillDef.activationStateMachineName = "Weapon";
            SkillDef.baseMaxStock = 1;
            SkillDef.baseRechargeInterval = 5f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = false;
            SkillDef.interruptPriority = InterruptPriority.Skill;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = false;
            SkillDef.cancelSprintingOnActivation = false;
            SkillDef.rechargeStock = 1;
            SkillDef.requiredStock = 1;
            SkillDef.stockToConsume = 1;
            SkillDef.icon = Assets.MainAssetBundle.LoadAsset<Sprite>("texVoidSurvivorSkillIcons_4");
            SkillDef.skillDescriptionToken = SURVIVORNAMEKEY + "_M2_DESCRIPTION";
            SkillDef.skillName = SURVIVORNAMEKEY + "_M2";
            SkillDef.skillNameToken = SURVIVORNAMEKEY + "_M2";
            ContentAddition.AddSkillDef(SkillDef);
            component.secondary = Utils.NewGenericSkill(characterPrefab, SkillDef);

            LanguageAPI.Add(SURVIVORNAMEKEY + "_M2_ALT", "End of Fate");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_M2_ALT_DESCRIPTION", "Shoot out a void implosion that <style=cIsDamage>instantly kills</style> anything in its radius.");
            SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SkillDef.activationState = new SerializableEntityStateType(typeof(AltSecondary));
            SkillDef.activationStateMachineName = "Weapon";
            SkillDef.baseMaxStock = 1;
            SkillDef.baseRechargeInterval = 5f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = false;
            SkillDef.interruptPriority = InterruptPriority.Skill;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = false;
            SkillDef.cancelSprintingOnActivation = false;
            SkillDef.rechargeStock = 1;
            SkillDef.requiredStock = 1;
            SkillDef.stockToConsume = 1;
            SkillDef.icon = Assets.MainAssetBundle.LoadAsset<Sprite>("texVoidSurvivorSkillIcons_8");
            SkillDef.skillDescriptionToken = SURVIVORNAMEKEY + "_M2_ALT_DESCRIPTION";
            SkillDef.skillName = SURVIVORNAMEKEY + "_M2_ALT";
            SkillDef.skillNameToken = SURVIVORNAMEKEY + "_M2_ALT";
            ContentAddition.AddSkillDef(SkillDef);
            Utils.AddAlt(component.secondary.skillFamily, SkillDef, Achievements.billionDamageUnlock);

        }
        void UtilitySetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();
            LanguageAPI.Add(SURVIVORNAMEKEY + "_UTIL", "Dimensional Ascension");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_UTIL_DESCRIPTION", "Gain <style=cIsDamage>1000% movement speed</style> for <style=cIsUtility>10s</style>. During this period, you may fly and travel through walls. You cannot be hit for the duration.");

            var SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SkillDef.activationState = new SerializableEntityStateType(typeof(Utility));
            SkillDef.activationStateMachineName = "NoClip";
            SkillDef.baseMaxStock = 1;
            SkillDef.baseRechargeInterval = 8f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = false;
            SkillDef.interruptPriority = InterruptPriority.Skill;
            SkillDef.isCombatSkill = false;
            SkillDef.mustKeyPress = true;
            SkillDef.cancelSprintingOnActivation = false;
            SkillDef.rechargeStock = 1;
            SkillDef.requiredStock = 1;
            SkillDef.stockToConsume = 1;
            SkillDef.icon = Prefabs.Load<SkillDef>("RoR2/DLC1/VoidSurvivor/VoidBlinkDown.asset").icon;
            SkillDef.skillDescriptionToken = SURVIVORNAMEKEY + "_UTIL_DESCRIPTION";
            SkillDef.skillName = SURVIVORNAMEKEY + "_UTIL";
            SkillDef.skillNameToken = SURVIVORNAMEKEY + "_UTIL";
            ContentAddition.AddSkillDef(SkillDef);
            component.utility = Utils.NewGenericSkill(characterPrefab, SkillDef);

            LanguageAPI.Add(SURVIVORNAMEKEY + "_UTIL_ALT", "Dimensional Transcendance");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_UTIL_ALT_DESCRIPTION", "<style=cIsDamage>Passive effect:</style> You may fly and travel though walls. You cannot be hit.");
            SkillDef = ScriptableObject.CreateInstance<MasterSpawnSlotSkillDef>();
            SkillDef.activationState = new SerializableEntityStateType(typeof(Idle));
            SkillDef.activationStateMachineName = "Traverse";
            SkillDef.baseMaxStock = 0;
            SkillDef.baseRechargeInterval = 0;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = false;
            SkillDef.interruptPriority = InterruptPriority.Skill;
            SkillDef.isCombatSkill = false;
            SkillDef.mustKeyPress = true;
            SkillDef.cancelSprintingOnActivation = false;
            SkillDef.rechargeStock = 0;
            SkillDef.requiredStock = 0;
            SkillDef.stockToConsume = 0;
            SkillDef.icon = Assets.MainAssetBundle.LoadAsset<Sprite>("texVoidSurvivorSkillIcons_2");
            SkillDef.skillDescriptionToken = SURVIVORNAMEKEY + "_UTIL_ALT_DESCRIPTION";
            SkillDef.skillName = SURVIVORNAMEKEY + "_UTIL_ALT";
            SkillDef.skillNameToken = SURVIVORNAMEKEY + "_UTIL_ALT";
            ContentAddition.AddSkillDef(SkillDef);
            Utils.AddAlt(component.utility.skillFamily, SkillDef, Achievements.endingUnlock);
        }
        void SpecialSetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();
            LanguageAPI.Add(SURVIVORNAMEKEY + "_SPEC", "Reality Warp");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_SPEC_DESCRIPTION", "Choose any item or equipment to find.");

            var SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SkillDef.activationState = new SerializableEntityStateType(typeof(Special));
            SkillDef.activationStateMachineName = "Pick";
            SkillDef.baseMaxStock = 1;
            SkillDef.baseRechargeInterval = 60f;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = false;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = true;
            SkillDef.cancelSprintingOnActivation = false;
            SkillDef.rechargeStock = 1;
            SkillDef.requiredStock = 1;
            SkillDef.stockToConsume = 1;
            SkillDef.icon = Prefabs.Load<SkillDef>("RoR2/DLC1/VoidSurvivor/CrushHealth.asset").icon;
            SkillDef.skillDescriptionToken = SURVIVORNAMEKEY + "_SPEC_DESCRIPTION";
            SkillDef.skillName = SURVIVORNAMEKEY + "_SPEC";
            SkillDef.skillNameToken = SURVIVORNAMEKEY + "_SPEC";
            ContentAddition.AddSkillDef(SkillDef);
            component.special = Utils.NewGenericSkill(characterPrefab, SkillDef);

            LanguageAPI.Add(SURVIVORNAMEKEY + "_SPEC_ALT", "Singularity");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_SPEC_ALT_DESCRIPTION", "<style=cIsDamage>Instantly kills</style> all enemies. Enemies that spawn in the next <style=cIsUtility>10s</style> instantly die.");
            SkillDef = ScriptableObject.CreateInstance<SkillDef>();
            SkillDef.activationState = new SerializableEntityStateType(typeof(AltSpecial));
            SkillDef.activationStateMachineName = "Pick";
            SkillDef.baseMaxStock = 0;
            SkillDef.baseRechargeInterval = 0;
            SkillDef.beginSkillCooldownOnSkillEnd = true;
            SkillDef.canceledFromSprinting = false;
            SkillDef.fullRestockOnAssign = false;
            SkillDef.interruptPriority = InterruptPriority.Any;
            SkillDef.isCombatSkill = true;
            SkillDef.mustKeyPress = true;
            SkillDef.cancelSprintingOnActivation = false;
            SkillDef.rechargeStock = 0;
            SkillDef.requiredStock = 0;
            SkillDef.stockToConsume = 0;
            SkillDef.icon = Assets.MainAssetBundle.LoadAsset<Sprite>("texVoidSurvivorSkillIcons_3");
            SkillDef.skillDescriptionToken = SURVIVORNAMEKEY + "_SPEC_ALT_DESCRIPTION";
            SkillDef.skillName = SURVIVORNAMEKEY + "_SPEC_ALT";
            SkillDef.skillNameToken = SURVIVORNAMEKEY + "_SPEC_ALT";
            ContentAddition.AddSkillDef(SkillDef);
            Utils.AddAlt(component.special.skillFamily, SkillDef, Achievements.unlockAll);
        }
    }
}
