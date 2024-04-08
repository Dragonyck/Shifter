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
    class Primary : BaseShifterState
    {
        private float duration;
        private float baseDuration = 0.15f;
        private float damageCoefficient = 2.5f;
        public override void OnEnter()
        {
            base.OnEnter();
            base.StartAimMode(1);
            duration = baseDuration / base.attackSpeedStat;

            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();
                var corePos = base.characterBody.corePosition;
                Vector3 forward = base.characterDirection.forward;
                Vector3 direction = Quaternion.AngleAxis(RoR2Application.rng.RangeFloat(-90, 91), forward) * Vector3.up;
                float pos = RoR2Application.rng.RangeFloat(behaviour.UpPositions[RoR2Application.rng.RangeInt(0, behaviour.UpPositions.Length)], behaviour.UpPositions[RoR2Application.rng.RangeInt(0, behaviour.UpPositions.Length)]);
                ProjectileManager.instance.FireProjectile(Prefabs.trackingOrbProjectile, corePos + Vector3.up * pos + direction * behaviour.LRPositions[RoR2Application.rng.RangeInt(0, behaviour.LRPositions.Length)], 
                    Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, base.characterBody.damage * damageCoefficient, 240, base.RollCrit(), DamageColorIndex.Default, tracker.trackingTarget ? tracker.trackingTarget.gameObject : null);
            }
            AkSoundEngine.PostEvent("Play_voidman_m2_shoot", base.gameObject);

            if (!behaviour.castingOrbs)
            {
                behaviour.castingOrbs = true;
                base.PlayAnimation("LeftArm, Override", "FireCorruptHandBeam");
            }
            float recoilAmplitude = 0.5f;
            base.AddRecoil(-1f * recoilAmplitude, -1.5f * recoilAmplitude, -0.25f * recoilAmplitude, 0.25f * recoilAmplitude);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= duration && base.isAuthority)
            {
                if (!base.IsKeyDownAuthority())
                {
                    outer.SetNextState(new PrimaryEnd());
                    return;
                }
                outer.SetNextStateToMain();
            }
        }
        public override void OnExit()
        {
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
    class PrimaryEnd : BaseShifterState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            behaviour.castingOrbs = false;
            base.PlayAnimation("LeftArm, Override", "ExitHandBeam");
            if (base.isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }
    }
}
