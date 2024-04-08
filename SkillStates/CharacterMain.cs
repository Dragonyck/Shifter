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
	class FlyState : GenericCharacterMain
	{
		private bool _providingAntiGravity;
		private bool _providingFlight;
		private ICharacterGravityParameterProvider targetCharacterGravityParameterProvider;
		private ICharacterFlightParameterProvider targetCharacterFlightParameterProvider;

		private bool providingAntiGravity
		{
			get
			{
				return _providingAntiGravity;
			}
			set
			{
				if (_providingAntiGravity == value)
				{
					return;
				}
				_providingAntiGravity = value;
				if (targetCharacterGravityParameterProvider != null)
				{
					CharacterGravityParameters gravityParameters = targetCharacterGravityParameterProvider.gravityParameters;
					gravityParameters.channeledAntiGravityGranterCount += (_providingAntiGravity ? 1 : -1);
					targetCharacterGravityParameterProvider.gravityParameters = gravityParameters;
				}
			}
		}
		private bool providingFlight
		{
			get
			{
				return _providingFlight;
			}
			set
			{
				if (_providingFlight == value)
				{
					return;
				}
				_providingFlight = value;
				if (targetCharacterFlightParameterProvider != null)
				{
					CharacterFlightParameters flightParameters = targetCharacterFlightParameterProvider.flightParameters;
					flightParameters.channeledFlightGranterCount += (_providingFlight ? 1 : -1);
					targetCharacterFlightParameterProvider.flightParameters = flightParameters;
				}
			}
		}
		private void StartFlight()
		{
			providingAntiGravity = true;
			providingFlight = true;
			if (base.characterBody.hasEffectiveAuthority && base.characterBody.characterMotor && base.characterBody.characterMotor.isGrounded)
			{
				Vector3 velocity = base.characterBody.characterMotor.velocity;
				velocity.y = 15f;
				base.characterBody.characterMotor.velocity = velocity;
				base.characterBody.characterMotor.Motor.ForceUnground();
			}
		}
		public override void OnEnter()
		{
			base.OnEnter();

			if (base.characterBody)
			{
				targetCharacterGravityParameterProvider = base.characterBody.GetComponent<ICharacterGravityParameterProvider>();
				targetCharacterFlightParameterProvider = base.characterBody.GetComponent<ICharacterFlightParameterProvider>();
				StartFlight();
			}
		}
        public override void HandleMovements()
        {
            base.HandleMovements();
			if (base.isAuthority)
            {
				base.characterMotor.velocity = base.moveVector * base.characterBody.moveSpeed;
			}
			base.characterBody.isSprinting = true;
        }
        public override void OnExit()
        {
            base.OnExit();
			providingAntiGravity = false;
			providingFlight = false;
		}
        public override void Update()
		{
			base.Update();
			if (base.characterBody.characterMotor.disableAirControlUntilCollision)
			{
				providingAntiGravity = false;
				providingFlight = false;
			}
			else
			{
				providingAntiGravity = true;
				providingFlight = true;
			}
		}
	}
	class Traverse : Idle
    {
		private EntityStateMachine bodyMachine;
		private bool traverse;
		private HurtBoxGroup hurtBoxGroup;
		private int collidableLayers;
		private CharacterModel characterModel;
		private GameObject pp;
		private bool stoppedMoving;
		public override void OnEnter()
        {
            base.OnEnter();
			bodyMachine = base.GetComponent<EntityStateMachine>();
			var skill = base.skillLocator.utility;
			traverse = Array.IndexOf(skill.skillFamily.variants, Array.Find<SkillFamily.Variant>(skill.skillFamily.variants, (x) => x.skillDef == skill.skillDef)) == 1;
			if (traverse)
			{
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
				if (NetworkServer.active && !base.characterBody.HasBuff(Prefabs.gigaSpeedBuffAlt))
				{
					base.characterBody.AddBuff(Prefabs.gigaSpeedBuffAlt);
				}
			}
		}
		void NoForce()
		{
			if (base.inputBank.moveVector == Vector3.zero && !stoppedMoving)
			{
				stoppedMoving = true;
				base.characterMotor.velocity = Vector3.zero;
				base.characterMotor.rootMotion = Vector3.zero;
			}
			else
			{
				stoppedMoving = false;
			}
		}
        public override void FixedUpdate()
        {
            base.FixedUpdate();
			if (base.characterBody.HasBuff(Prefabs.gigaSpeedBuff) || base.characterBody.HasBuff(Prefabs.gigaSpeedBuffAlt))
            {
				NoForce();
			}
			if (!traverse || !base.isAuthority)
            {
				return;
			}
			if (!pp && MainPlugin.enablePP.Value)
			{
				pp = UnityEngine.Object.Instantiate(Prefabs.traversePPPermanent);
			}
			if (bodyMachine.state.GetType() != typeof(FlyState))
            {
				bodyMachine.SetNextState(new FlyState());
			}
		}
		public override void OnExit()
		{
			if (traverse)
			{
				if (base.isAuthority)
				{
					if (pp)
					{
						Destroy(pp);
					}
				}
				for (int i = 0; i < characterModel.baseRendererInfos.Length; i++)
				{
					characterModel.baseRendererInfos[i].defaultMaterial = Prefabs.redOverlayMat;
				}
				base.characterMotor.Motor.CollidableLayers = collidableLayers;
				int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter - 1;
				hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
			}
			base.OnExit();
		}
	}
}
