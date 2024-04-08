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
    class AltPrimary : BaseShifterState
    {
        private float duration = 0.25f;
        private List<HurtBox> HurtBoxes = new List<HurtBox>();
        private bool executed;
        private GameObject chargeEffect;
        public override void OnEnter()
        {
            base.OnEnter();
            base.StartAimMode(2);

            var child = base.FindModelChild("MuzzleHandBeam");
            int chainCount = (int)base.characterBody.level/MainPlugin.chain.Value;
            var target = tracker.trackingTarget;
            HurtBoxes.Add(target);
            NewLineEffect(child, target);

            for (int i = 0; i < chainCount; i++)
            {
                SearchNewTarget(HurtBoxes[i].transform);
            }

            base.PlayAnimation("LeftArm, Override", "FireCorruptHandBeam");

            chargeEffect = UnityEngine.Object.Instantiate(Prefabs.itemPanelChargeEffect, child.position, Quaternion.identity, child);
            chargeEffect.GetComponent<ObjectScaleCurve>().timeMax = 0.15f;

            AkSoundEngine.PostEvent("Play_voidman_m1_corrupted_start", base.gameObject);
        }
        void SearchNewTarget(Transform parent)
        {
            float lastDistance = 999;
            HurtBox newTarget = null;
            foreach (CharacterBody c in CharacterBody.instancesList)
            {
                if (c.healthComponent.alive && TeamMask.GetEnemyTeams(base.teamComponent.teamIndex).HasTeam(c.teamComponent.teamIndex) && !HurtBoxes.Contains(c.mainHurtBox))
                {
                    var dist = Vector3.Distance(parent.position, c.mainHurtBox.transform.position);
                    if (dist < lastDistance)
                    {
                        lastDistance = dist;
                        newTarget = c.mainHurtBox;
                        break;
                    }
                }
            }
            if (newTarget)
            {
                HurtBoxes.Add(newTarget);
                NewLineEffect(parent, newTarget);
            }
        }
        void NewLineEffect(Transform parent, HurtBox target)
        {
            var lineEffect = UnityEngine.Object.Instantiate(Prefabs.executeLineEffect, parent.position, Quaternion.identity, parent);
            lineEffect.GetComponent<DelayedAlpha>().target = target;

            if (target.healthComponent && target.healthComponent.body && target.healthComponent.body.modelLocator && target.healthComponent.body.modelLocator.modelTransform)
            {
                var temporaryOverlay = target.healthComponent.body.modelLocator.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                temporaryOverlay.duration = 99999;
                temporaryOverlay.originalMaterial = Prefabs.instaKillMat;
                temporaryOverlay.AddToCharacerModel(target.healthComponent.body.modelLocator.modelTransform.GetComponent<CharacterModel>());
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= duration)
            {
                if (!executed)
                {
                    executed = true;
                    base.PlayAnimation("LeftArm, Override", "ExitHandBeam");
                    for (int i = 0; i < HurtBoxes.Count; i++)
                    {
                        var targetHurtbox = HurtBoxes[i];
                        if (targetHurtbox)
                        {
                            behaviour.statHolder.statGains[0] += targetHurtbox.healthComponent.body.baseMaxHealth;
                            behaviour.statHolder.statGains[1] += targetHurtbox.healthComponent.body.baseDamage;
                            behaviour.statHolder.statGains[2] += targetHurtbox.healthComponent.body.baseMaxShield;
                            behaviour.statHolder.statGains[3] += targetHurtbox.healthComponent.body.baseArmor;

                            EffectManager.SpawnEffect(Prefabs.executeLineImpactEffect, new EffectData() { 
                                origin = targetHurtbox.transform.position,
                                scale = 12
                            }, false);

                            if (NetworkServer.active)
                            {
                                targetHurtbox.healthComponent.Suicide(base.gameObject);
                            }
                        }
                    }
                }
                if (base.isAuthority)
                {
                    outer.SetNextStateToMain();
                }
            }
        }
        public override void OnExit()
        {
            AkSoundEngine.PostEvent("Play_voidman_m1_corrupted_end", base.gameObject);
            if (chargeEffect)
            {
                Destroy(chargeEffect);
            }
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
