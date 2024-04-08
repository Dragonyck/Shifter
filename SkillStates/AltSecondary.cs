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
    class AltSecondary : BaseShifterState
    {
        private float duration;
        private float baseDuration = 0.45f;
        public override void OnEnter()
        {
            base.OnEnter();
            base.StartAimMode(1);
            duration = baseDuration / base.attackSpeedStat;
            base.PlayAnimation("RightArm, Override", "FireMegaBlaster", "MegaBlaster.playbackRate", duration);

            float recoilAmplitude = 5;
            base.AddRecoil(-1f * recoilAmplitude, -1.5f * recoilAmplitude, -0.25f * recoilAmplitude, 0.25f * recoilAmplitude);

            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    crit = base.RollCrit(),
                    damage = 0,
                    damageTypeOverride = DamageType.VoidDeath | DamageType.BypassArmor | DamageType.BypassBlock | DamageType.BypassOneShotProtection,
                    damageColorIndex = DamageColorIndex.Default,
                    force = 500,
                    owner = base.gameObject,
                    position = aimRay.origin,
                    procChainMask = default(RoR2.ProcChainMask),
                    projectilePrefab = Prefabs.insteaDeathProjectile,
                    rotation = Util.QuaternionSafeLookRotation(aimRay.direction),
                    useFuseOverride = false,
                    useSpeedOverride = true,
                    speedOverride = 120,
                    target = null
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }

            AkSoundEngine.PostEvent("Play_voidman_m2_shoot_fullCharge", base.gameObject);

            EffectManager.SimpleMuzzleFlash(Prefabs.itemPanelMuzzleEffect, base.gameObject, "MuzzleMegaBlaster", false);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority && base.fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return  InterruptPriority.PrioritySkill;
        }
    }
}
