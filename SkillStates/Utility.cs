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
    class Utility : BaseShifterState
    {
        private float duration = 10;
        private HurtBoxGroup hurtBoxGroup;
        private int collidableLayers;
        private CharacterModel characterModel;
        private GameObject pp;
        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active)
            { 
                base.characterBody.AddBuff(Prefabs.gigaSpeedBuff);
            }
            hurtBoxGroup = base.GetModelTransform().GetComponent<HurtBoxGroup>();
            int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter + 1;
            hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;

            collidableLayers = base.characterMotor.Motor.CollidableLayers;
            base.characterMotor.Motor.CollidableLayers = 0;

            characterModel = base.GetModelTransform().GetComponent<CharacterModel>();
            for (int i = 0; i < characterModel.baseRendererInfos.Length; i++)
            {
                characterModel.baseRendererInfos[i].defaultMaterial = Prefabs.ghostEffectMat;
            }

            if (base.isAuthority)
            {
                base.GetComponent<EntityStateMachine>().SetNextState(new FlyState());
                pp = UnityEngine.Object.Instantiate(Prefabs.traversePP);
            }

            EffectManager.SimpleEffect(Prefabs.Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidBlinkMuzzleflashCorrupted.prefab"), base.characterBody.corePosition, Quaternion.identity, false);

            AkSoundEngine.PostEvent("Play_voidman_shift_start", base.gameObject);
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
            EffectManager.SimpleEffect(Prefabs.Load<GameObject>("RoR2/DLC1/VoidSurvivor/VoidBlinkMuzzleflashCorrupted.prefab"), base.characterBody.corePosition, Quaternion.identity, false);
            AkSoundEngine.PostEvent("Play_voidman_shift_end", base.gameObject);
            if (base.isAuthority)
            {
                if (pp)
                {
                    Destroy(pp);
                }
                base.GetComponent<EntityStateMachine>().SetNextStateToMain();
            }
            for (int i = 0; i < characterModel.baseRendererInfos.Length; i++)
            {
                characterModel.baseRendererInfos[i].defaultMaterial = Prefabs.redOverlayMat;
            }
            base.characterMotor.Motor.CollidableLayers = collidableLayers;
            if (NetworkServer.active)
            {
                base.characterBody.RemoveBuff(Prefabs.gigaSpeedBuff);
            }
            int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter - 1;
            hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
