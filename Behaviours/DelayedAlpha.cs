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
    class DelayedAlpha : MonoBehaviour
    {
        public Transform startTransform;
        public HurtBox target;
        public AnimateShaderAlpha alpha;
        public BezierCurveLine curve;
        private Vector3 lastPos;
        private void Update()
        {
            if (!startTransform)
            {
                startTransform = base.transform.parent;
                base.transform.parent = null;
            }
            else
            {
                base.transform.position = startTransform.position;
            }
            if (target)
            {
                lastPos = target.transform.position;
                if (!alpha.enabled && !target.healthComponent.alive)
                {
                    alpha.enabled = true;
                }
            }
            else
            {
                alpha.enabled = true;
            }
            if (!startTransform)
            {
                alpha.enabled = true;
            }
            curve.endTransform.position = lastPos;
        }
    }
}
