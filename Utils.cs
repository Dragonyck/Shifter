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
using HG;
using System.Runtime.InteropServices;
using UnityEngine.Events;
using UnityEngine.AddressableAssets;
using Rewired.ComponentControls.Effects;    
using System.Linq;

namespace Shifter       
{
    class Utils
    {
        public static EntityStateMachine NewStateMachine<T>(GameObject obj, string customName) where T : EntityState
        {
            SerializableEntityStateType s = new SerializableEntityStateType(typeof(T));
            var newStateMachine = obj.AddComponent<EntityStateMachine>();
            newStateMachine.customName = customName;
            newStateMachine.initialStateType = s;
            newStateMachine.mainStateType = s;
            return newStateMachine;
        }
        public static GenericSkill NewGenericSkill(GameObject obj, SkillDef skill)
        {
            GenericSkill generic = obj.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            generic._skillFamily = newFamily;
            SkillFamily skillFamily = generic.skillFamily;
            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = skill,
                viewableNode = new ViewablesCatalog.Node(skill.skillNameToken, false, null)
            };
            ContentAddition.AddSkillFamily(skillFamily);
            return generic;
        }
        public static void AddAlt(SkillFamily skillFamily, SkillDef SkillDef, UnlockableDef unlock = null)
        {
            Array.Resize<SkillFamily.Variant>(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = SkillDef,
                viewableNode = new ViewablesCatalog.Node(SkillDef.skillNameToken, false, null),
                unlockableDef = unlock
            };
        }
        public static BuffDef NewBuffDef(string name, bool stack, bool hidden, Sprite sprite, Color color)
        {
            BuffDef buff = ScriptableObject.CreateInstance<BuffDef>();
            buff.name = name;
            buff.canStack = stack;
            buff.isHidden = hidden;
            buff.iconSprite = sprite;
            buff.buffColor = color;
            ContentAddition.AddBuffDef(buff);
            return buff;
        }
        internal static LoadoutAPI.SkinDefInfo CreateNewSkinDefInfo(CharacterModel.RendererInfo[] rendererInfos, GameObject rootObject, string skinName)
        {
            LoadoutAPI.SkinDefInfo skinDefInfo = default(LoadoutAPI.SkinDefInfo);
            skinDefInfo.BaseSkins = Array.Empty<SkinDef>();
            skinDefInfo.MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0];
            skinDefInfo.ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0];
            skinDefInfo.GameObjectActivations = new SkinDef.GameObjectActivation[0];
            skinDefInfo.Icon = LoadoutAPI.CreateSkinIcon(Color.black, Color.red, Color.black, Color.red);
            skinDefInfo.MeshReplacements = new SkinDef.MeshReplacement[0];
            skinDefInfo.Name = skinName;
            skinDefInfo.NameToken = skinName;
            skinDefInfo.RendererInfos = rendererInfos;
            skinDefInfo.RootObject = rootObject;
            skinDefInfo.UnlockableDef = null;
            return skinDefInfo;
        }
        internal static EffectComponent RegisterEffect(GameObject effect, float duration, string soundName = "", bool parentToReferencedTransform = true, bool positionAtReferencedTransform = true, bool applyScale = false)
        {
            var effectcomponent = effect.GetComponent<EffectComponent>();
            if (!effectcomponent)
            {
                effectcomponent = effect.AddComponent<EffectComponent>();
            }
            if (duration != -1)
            {
                var destroyOnTimer = effect.GetComponent<DestroyOnTimer>();
                if (!destroyOnTimer)
                {
                    effect.AddComponent<DestroyOnTimer>().duration = duration;
                }
                else
                {
                    destroyOnTimer.duration = duration;
                }
            }
            if (!effect.GetComponent<NetworkIdentity>())
            {
                effect.AddComponent<NetworkIdentity>();
            }
            if (!effect.GetComponent<VFXAttributes>())
            {
                effect.AddComponent<VFXAttributes>().vfxPriority = VFXAttributes.VFXPriority.Always;
            }
            effectcomponent.applyScale = applyScale;
            effectcomponent.effectIndex = EffectIndex.Invalid;
            effectcomponent.parentToReferencedTransform = parentToReferencedTransform;
            effectcomponent.positionAtReferencedTransform = positionAtReferencedTransform;
            effectcomponent.soundName = soundName;
            ContentAddition.AddEffect(effect);
            return effectcomponent;
        }
    }
}
