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
    class DelayedProjectile : MonoBehaviour
    {
        public ProjectileSimple simple;
        public ProjectileDirectionalTargetFinder finder;
        public ProjectileTargetComponent target;
        public ProjectileController controller;
        public GameObject owner;
        public CharacterBody body;
        private float duration = 0.2f;
        private float stopwatch;
        private void Awake()
        {
            simple = base.GetComponent<ProjectileSimple>();
            finder = base.GetComponent<ProjectileDirectionalTargetFinder>();
            target = base.GetComponent<ProjectileTargetComponent>();
            controller = base.GetComponent<ProjectileController>();
        }
        private void FixedUpdate()
        {
            if (!owner)
            {
                owner = controller.owner;
                if (owner)
                {
                    body = owner.GetComponent<CharacterBody>();
                    if (body)
                    {
                        duration /= body.attackSpeed;
                    }
                    base.transform.parent = owner.transform;
                }
            }
            stopwatch += Time.fixedDeltaTime;
            if (stopwatch >= duration)
            {
                base.transform.parent = null;
                target.target = null;
                finder.SearchForTarget();
                simple.enabled = true;
            }
        }
    }
}
