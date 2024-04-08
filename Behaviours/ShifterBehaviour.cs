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
    class ShifterBehaviour : NetworkBehaviour
    {
        public ShifterStatsHolder statHolder;
        public CharacterBody body;
        public bool pickedItem;
        public bool canceledPick;
        public bool castingOrbs;
        public float[] LRPositions = new float[]
        {
            0.5f, 1
        };
        public float[] UpPositions = new float[]
        {
            0.1f, 0.35f, 1
        };
        [Command]
        public void CmdCreatePickup(int index)
        {
            if (MainPlugin.giveItem.Value)
            {
                var pickup = Prefabs.allItems[index];
                PickupDef pickupDef = PickupCatalog.GetPickupDef(pickup);
                var itemDef = ItemCatalog.GetItemDef(pickupDef.itemIndex);
                if (itemDef)
                {
                    body.inventory.GiveItem(itemDef);
                }
                var equipDef = EquipmentCatalog.GetEquipmentDef(pickupDef.equipmentIndex);
                if (equipDef)
                {
                    body.inventory.GiveEquipmentString(equipDef.name);
                }
                return;
            }
            PickupDropletController.CreatePickupDroplet(Prefabs.allItems[index], base.transform.position + Vector3.up * 1.5f, Vector3.up * 20 + body.characterDirection.forward * 2);
        }
        private void Awake()
        {
            body = base.GetComponent<CharacterBody>();
        }
        private void Start()
        {
            if (body && body.masterObject)
            {
                statHolder = body.masterObject.GetComponent<ShifterStatsHolder>();
                if (!statHolder)
                {
                    statHolder = body.masterObject.AddComponent<ShifterStatsHolder>();
                }
            }
        }
    }
}
