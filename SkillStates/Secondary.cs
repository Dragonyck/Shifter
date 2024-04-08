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
    class Secondary : BaseShifterState
    {
        private float duration;
        private float baseDuration = 0.45f;
        private float damageCoefficient = 2.5f;

        public override void OnEnter()
        {
            base.OnEnter();
            base.StartAimMode(1);
            if (!base.isGrounded)
            {
                base.SmallHop(base.characterMotor, 8);
            }

            duration = baseDuration / base.attackSpeedStat;
            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();
                ProjectileManager.instance.FireProjectile(Prefabs.freezingWaveProjectile, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, base.damageStat * 4, 0, false, DamageColorIndex.Default);
            }
            base.PlayAnimation("LeftArm, Override", "SwingMelee2", "Melee.playbackRate", duration);

            var child = base.FindModelChild("MuzzleMelee");
            UnityEngine.Object.Instantiate(Prefabs.swingEffect, child.position, Quaternion.identity, child).transform.localRotation = Quaternion.identity;

            AkSoundEngine.PostEvent("Play_bandit2_m2_slash", base.gameObject);

            float recoilAmplitude = 5;
            base.AddRecoil(-1f * recoilAmplitude, -1.5f * recoilAmplitude, -0.25f * recoilAmplitude, 0.25f * recoilAmplitude);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= duration && base.isAuthority)
            {
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
}
