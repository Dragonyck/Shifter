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

namespace Shifter
{
    class Hook
    {
        internal static void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            On.RoR2.CharacterBody.SetBuffCount += CharacterBody_SetBuffCount;
            On.RoR2.CharacterBody.CallCmdAddTimedBuff += CharacterBody_CallCmdAddTimedBuff;
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float_int += CharacterBody_AddTimedBuff_BuffDef_float_int;
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += CharacterBody_AddTimedBuff_BuffDef_float;
            On.RoR2.CharacterBody.AddBuff_BuffIndex += CharacterBody_AddBuff_BuffIndex;
            On.RoR2.DotController.InflictDot_refInflictDotInfo += DotController_InflictDot_refInflictDotInfo;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            //On.RoR2.JumpVolume.OnTriggerStay += JumpVolume_OnTriggerStay;
        }

        /*private static void JumpVolume_OnTriggerStay(On.RoR2.JumpVolume.orig_OnTriggerStay orig, JumpVolume self, Collider other)
        {
            CharacterBody body = other ? other.GetComponent<CharacterBody>() : null;
            if (body && body.HasBuff(Prefabs.gigaSpeedBuff) || body.HasBuff(Prefabs.gigaSpeedBuffAlt))
            {
                return;
            }
            orig(self, other);
        }*/

        private static void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (self.GetComponent<ShifterBehaviour>())
            {
                if (self.skillLocator.secondary)
                {
                    float scale = self.skillLocator.secondary.cooldownScale;
                    scale *= Mathf.Clamp((float)Math.Pow(0.75f, self.level - 1), 0, 1);
                    self.skillLocator.secondary.cooldownScale = scale;
                }
                if (self.skillLocator.utility)
                {
                    float scale = self.skillLocator.utility.cooldownScale;
                    scale *= Mathf.Clamp((float)Math.Pow(0.75f, self.level - 1), 0, 1);
                    self.skillLocator.utility.cooldownScale = scale;
                }
                if (self.skillLocator.special)
                {
                    float scale = self.skillLocator.special.cooldownScale;
                    scale *= Mathf.Clamp((float)Math.Pow(0.75f, self.level - 1), 0, 1);
                    self.skillLocator.special.cooldownScale = scale;
                }
            }
        }
        private static void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            var cost = self.costType;
            var body = activator.GetComponent<CharacterBody>();
            if (self.costType == CostTypeIndex.PercentHealth && activator.GetComponent<ShifterBehaviour>() && body && body.HasBuff(Prefabs.gigaSpeedBuff) || body.HasBuff(Prefabs.gigaSpeedBuffAlt))
            {
                self.costType = CostTypeIndex.None;
            }
            orig(self, activator);
            self.costType = cost;
        }

        private static void DotController_InflictDot_refInflictDotInfo(On.RoR2.DotController.orig_InflictDot_refInflictDotInfo orig, ref InflictDotInfo inflictDotInfo)
        {
            if (inflictDotInfo.victimObject.GetComponent<ShifterBehaviour>())
            {
                return;
            }
            orig(ref inflictDotInfo);
        }
        private static void CharacterBody_AddBuff_BuffIndex(On.RoR2.CharacterBody.orig_AddBuff_BuffIndex orig, CharacterBody self, BuffIndex buffType)
        {
            if (self.GetComponent<ShifterBehaviour>() && BuffCatalog.GetBuffDef(buffType).isDebuff)
            {
                return;
            }
            orig(self, buffType);
        }
        private static void CharacterBody_AddTimedBuff_BuffDef_float(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float orig, CharacterBody self, BuffDef buffDef, float duration)
        {
            if (self.GetComponent<ShifterBehaviour>() && buffDef.isDebuff || buffDef == DLC1Content.Buffs.VoidRaidCrabWardWipeFog)
            {
                return;
            }
            orig(self, buffDef, duration);
        }
        private static void CharacterBody_AddTimedBuff_BuffDef_float_int(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float_int orig, CharacterBody self, BuffDef buffDef, float duration, int maxStacks)
        {
            if (self.GetComponent<ShifterBehaviour>() && buffDef.isDebuff)
            {
                return;
            }
            orig(self, buffDef, duration, maxStacks);
        }
        private static void CharacterBody_CallCmdAddTimedBuff(On.RoR2.CharacterBody.orig_CallCmdAddTimedBuff orig, CharacterBody self, BuffIndex buffType, float duration)
        {
            if (self.GetComponent<ShifterBehaviour>() && BuffCatalog.GetBuffDef(buffType).isDebuff)
            {
                return;
            }
            orig(self, buffType, duration);
        }
        private static void CharacterBody_SetBuffCount(On.RoR2.CharacterBody.orig_SetBuffCount orig, CharacterBody self, BuffIndex buffType, int newCount)
        {
            orig(self, buffType, newCount);
        }
        private static void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            SetStateOnHurt state = null;
            bool shouldDestroy = false;
            if (damageInfo.attacker.GetComponent<ShifterBehaviour>())
            {
                if (damageInfo.damageType == DamageType.Freeze2s)
                {
                    damageInfo.damageType = DamageType.Generic;
                    state = self.GetComponent<SetStateOnHurt>();
                    if (!state)
                    {
                        shouldDestroy = true;
                        state = self.gameObject.AddComponent<SetStateOnHurt>();
                        state.canBeFrozen = true;
                        var machines = self.GetComponents<EntityStateMachine>();
                        state.targetStateMachine = machines.First((x) => x.customName == "Body");
                        state.idleStateMachine = machines.Where((x) => x.customName != "Body").ToArray();
                    }
                    state.SetStun(1);
                    state.SetFrozen(5);
                }
            }
            if (damageInfo.force != Vector3.zero && self.GetComponent<ShifterBehaviour>())
            {
                damageInfo.force = Vector3.zero;
            }
            if (damageInfo.inflictor == null && damageInfo.damageColorIndex == DamageColorIndex.Void && self.body && self.GetComponent<ShifterBehaviour>())
            {
                damageInfo.rejected = true;
            }
            orig(self, damageInfo);
            ShifterBehaviour behaviour = damageInfo.attacker ? damageInfo.attacker.GetComponent<ShifterBehaviour>() : null;
            if (behaviour)
            {
                if (shouldDestroy)
                {
                    UnityEngine.Object.Destroy(self.GetComponent<SetStateOnHurt>());
                }
                if (self.alive && self.body && damageInfo.dotIndex == DotController.DotIndex.None && damageInfo.procCoefficient > 0)
                {
                    DotController.InflictDot(self.gameObject, damageInfo.attacker, DotController.DotIndex.SuperBleed, 10f * damageInfo.procCoefficient, 1f);
                    DotController.InflictDot(self.gameObject, damageInfo.attacker, DotController.DotIndex.Poison, 10f * damageInfo.procCoefficient, 1f);
                    self.body.AddTimedBuff(RoR2Content.Buffs.HealingDisabled, 10f * damageInfo.procCoefficient);
                    self.body.AddTimedBuff(RoR2Content.Buffs.LunarSecondaryRoot, 10f * damageInfo.procCoefficient);
                    if (RoR2Application.rng.nextBool)
                    {
                        //self.body.AddTimedBuff(BuffCatalog.nonHiddenBuffIndices[RoR2Application.rng.RangeInt(0, BuffCatalog.nonHiddenBuffIndices.Length - 1)], 10f * damageInfo.procCoefficient);
                    }
                    else
                    {
                        //DotController.InflictDot(self.gameObject, damageInfo.attacker, (DotController.DotIndex)RoR2Application.rng.RangeInt(0, 9), 10f * damageInfo.procCoefficient, 1f);
                    }
                }
                if (damageInfo.inflictor && damageInfo.inflictor.GetComponent<InstaKill>() && damageInfo.rejected)
                {
                    self.Suicide(damageInfo.attacker);
                }
            }
        }
        private static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            var behaviour = sender.GetComponent<ShifterBehaviour>();
            if (behaviour)
            {
                if (behaviour.statHolder)
                {
                    args.baseHealthAdd += behaviour.statHolder.statGains[0];
                    args.baseDamageAdd += behaviour.statHolder.statGains[1];
                    args.baseShieldAdd += behaviour.statHolder.statGains[2];
                    args.armorAdd += behaviour.statHolder.statGains[3];
                }

                float level = sender.level;
                args.healthMultAdd += 0.05f * level;
                args.regenMultAdd += 0.05f * level;
                args.shieldMultAdd += 0.05f * level;
                args.moveSpeedMultAdd += 0.05f * level;
                args.damageMultAdd += 0.05f * level;
                args.attackSpeedMultAdd += 0.05f * level;
                args.critAdd += 0.05f * level;
                args.armorAdd += 0.05f * level;
                //args.cooldownReductionAdd += 0.05f * level;
            }
            if (sender.HasBuff(Prefabs.gigaSpeedBuff))
            {
                args.moveSpeedMultAdd += 10;
            }
            if (sender.HasBuff(Prefabs.gigaSpeedBuffAlt))
            {
                args.moveSpeedMultAdd += MainPlugin.utilSpeed.Value;
            }
        }
    }
}
