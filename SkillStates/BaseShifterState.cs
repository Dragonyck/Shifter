﻿using System;
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
    class BaseShifterState : BaseSkillState
    {
        public ShifterBehaviour behaviour;
        public ShifterTracker tracker { get; set; }

        public override void OnEnter()
        {
            base.OnEnter();
            behaviour = base.GetComponent<ShifterBehaviour>();
            tracker = base.GetComponent<ShifterTracker>();
        }
    }
}
