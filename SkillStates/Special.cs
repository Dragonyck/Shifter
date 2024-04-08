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
    class Special : BaseShifterState
    {
        private float duration = 0.65f;
        private float chargeDuration = 0.58f;
        private bool spawnedParticle;
        public CameraRigController camera;
        private GameObject panelInstance;
        private GameObject chargeEffect;

        public override void OnEnter()
        {
            base.OnEnter();
            if (RoR2Application.isInMultiPlayer && NetworkServer.active)
            {
                base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
            }
            base.PlayAnimation("LeftArm, Override", "CrushCorruption", "CrushCorruption.playbackRate", duration);

            var child = base.FindModelChild("MuzzleHandBeam");
            chargeEffect = UnityEngine.Object.Instantiate(Prefabs.itemPanelChargeEffect, child.position, Quaternion.identity, child);
            chargeEffect.GetComponent<ObjectScaleCurve>().timeMax = duration + 0.05f;

            AkSoundEngine.PostEvent("Play_voidman_R_activate", base.gameObject);

        }
        public override void Update()
        {
            base.Update();
            if (base.age >= chargeDuration && !spawnedParticle)
            {
                spawnedParticle = true;
                EffectManager.SimpleMuzzleFlash(Prefabs.itemPanelMuzzleEffect, base.gameObject, "MuzzleHandBeam", false);
            }
            if (base.isAuthority && base.age >= duration && !panelInstance)
            {
                camera = CameraRigController.instancesList.Find(x => x.target = base.gameObject);
                panelInstance = UnityEngine.Object.Instantiate<GameObject>(Prefabs.itemSelectionPanel, camera.hud.mainContainer.transform);
                var panel = panelInstance.GetComponent<ShifterPanelBehaviour>();
                panel.behaviour = behaviour;
                panel.SetPickupOptions(Prefabs.allOptions);
            }
            if (base.isAuthority && behaviour.pickedItem || behaviour.canceledPick)
            {
                outer.SetNextStateToMain();
            }
        }
        public override void OnExit()
        {
            AkSoundEngine.PostEvent("Play_voidman_R_pop", base.gameObject);
            if (chargeEffect)
            {
                Destroy(chargeEffect);
            }
            if (RoR2Application.isInMultiPlayer && NetworkServer.active)
            {
                base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
            }
            behaviour.pickedItem = false;
            behaviour.canceledPick = false;
            if (panelInstance)
            {
                Destroy(panelInstance);
            }
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
