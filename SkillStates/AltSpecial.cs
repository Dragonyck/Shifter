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
    class AltSpecial : BaseShifterState
    {
        private float duration = 10;
        private TeamMask mask;
        private List<CharacterBody> killList = new List<CharacterBody>();
        private GameObject pp;
        public override void OnEnter()
        {
            base.OnEnter();
            mask = TeamMask.GetEnemyTeams(base.teamComponent.teamIndex);

            base.PlayAnimation("LeftArm, Override", "CrushCorruption", "CrushCorruption.playbackRate", 0.45f);

            AkSoundEngine.PostEvent("Play_voidman_R_pop", base.gameObject);
            EffectManager.SimpleMuzzleFlash(Prefabs.itemPanelMuzzleEffect, base.gameObject, "MuzzleHandBeam", false);

            pp = UnityEngine.Object.Instantiate(Prefabs.instaKillPP);
        }
        void Execute()
        {
            if (!NetworkServer.active)
            {
                return;
            }
            foreach (CharacterBody body in CharacterBody.instancesList)
            {
                if (body.healthComponent.alive && mask.HasTeam(body.teamComponent.teamIndex))
                {
                    body.healthComponent.Suicide(base.gameObject);
                }
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority && base.fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
            Execute();
        }
        public override void OnExit()
        {
            if (pp)
            {
                Destroy(pp);
            }
            base.OnExit();
        }
    }
}
