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

namespace Shifter
{
    class MeleeSkillState : BaseSkillState
    {
        internal float hitPauseDuration;
        internal float hopVelocity = EntityStates.Merc.Assaulter.smallHopVelocity;
        internal string animParameter = "";
        internal float hitPauseTimer;
        internal bool isInHitPause;
        internal BaseState.HitStopCachedState hitStopCachedState;
        internal Animator animator;
        internal float stopwatch;
        internal OverlapAttack attack;
        internal bool hitCallback;
        internal float damage;
        internal GameObject hitEffectPrefab;
        internal HitBoxGroup hitBoxGroup;
        internal DamageType damageType = DamageType.Generic;
        internal DamageColorIndex damageColor = DamageColorIndex.Default;
        internal Vector3 forceVector = Vector3.back * 100;
        internal float attackStopwatch;

        public override void OnEnter()
        {
            base.OnEnter();
            animator = base.GetModelAnimator();
            isInHitPause = false;
            hitPauseDuration = EntityStates.Merc.GroundLight.hitPauseDuration / base.attackSpeedStat;
        }
        internal OverlapAttack NewOverlapAttack()
        {
            var attack = new OverlapAttack();
            attack.procChainMask = default(ProcChainMask);
            attack.procCoefficient = 1f;
            attack.attacker = base.gameObject;
            attack.inflictor = base.gameObject;
            attack.teamIndex = base.characterBody.teamComponent.teamIndex;
            attack.damage = base.characterBody.damage * damage;
            attack.forceVector = forceVector;
            attack.hitEffectPrefab = hitEffectPrefab;
            attack.isCrit = base.characterBody.RollCrit();
            attack.damageColorIndex = damageColor;
            attack.damageType = damageType;
            attack.maximumOverlapTargets = 100;
            attack.hitBoxGroup = hitBoxGroup;
            return attack;
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            hitPauseTimer -= Time.fixedDeltaTime;
            if (hitCallback)
            {
                if (!isInHitPause)
                {
                    if (!base.isGrounded)
                    {
                        base.SmallHop(base.characterMotor, hopVelocity);
                    }
                    if (!animParameter.IsNullOrWhiteSpace())
                    {
                        hitStopCachedState = base.CreateHitStopCachedState(base.characterMotor, animator, animParameter);
                    }
                    hitPauseTimer = hitPauseDuration / attackSpeedStat;
                    isInHitPause = true;
                }
            }
            if (!animParameter.IsNullOrWhiteSpace() && hitPauseTimer <= 0f && isInHitPause)
            {
                base.ConsumeHitStopCachedState(hitStopCachedState, base.characterMotor, animator);
                isInHitPause = false;
            }
            if (!isInHitPause)
            {
                attackStopwatch += Time.fixedDeltaTime;
                stopwatch += Time.fixedDeltaTime;
                if (animator)
                {
                    animator.speed = 1;
                }
            }
            else
            {
                if (animator)
                {
                    animator.speed = 0;
                }
            }
        }
    }
}
