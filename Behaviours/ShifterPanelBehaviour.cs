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
using System.Collections.ObjectModel;

namespace Shifter
{
    class ShifterPanelBehaviour : MonoBehaviour
    {
        public ShifterBehaviour behaviour;
		public GridLayoutGroup gridlayoutGroup;
		public RectTransform buttonContainer;
		public GameObject buttonPrefab;
		public Image[] coloredImages;
		public Image[] darkColoredImages;
		public int maxColumnCount = 6;
		public MPButton cancelButton;
		private UIElementAllocator<MPButton> buttonAllocator;
		private void Awake()
		{
			buttonAllocator = new UIElementAllocator<MPButton>(buttonContainer, buttonPrefab, true, false);
			buttonAllocator.onCreateElement = new UIElementAllocator<MPButton>.ElementOperationDelegate(OnCreateButton);
			cancelButton.onClick.AddListener(delegate ()
			{
				if (behaviour)
				{
					behaviour.canceledPick = true;
					var special = behaviour.GetComponent<SkillLocator>().special;
					special.RunRecharge(special.skillDef.baseRechargeInterval);
				}
			});
		}
		private void OnEnable()
		{
			if (RoR2Application.isInSinglePlayer)
			{
				Time.timeScale = 0.0f;
			}
		}
		private void OnDisable()
		{
			if (RoR2Application.isInSinglePlayer)
			{
				Time.timeScale = 1.0f;
			}
		}
		private void OnCreateButton(int index, MPButton button)
		{
			button.onClick.AddListener(delegate ()
			{
				behaviour.pickedItem = true;
				behaviour.CmdCreatePickup(index);
				Destroy(base.gameObject);
			});
		}
		public void SetPickupOptions(PickupPickerController.Option[] options)
		{
			buttonAllocator.AllocateElements(options.Length);
			ReadOnlyCollection<MPButton> elements = buttonAllocator.elements;
			Sprite sprite = Prefabs.texUnlockIcon;
			if (options.Length != 0)
			{
				PickupDef pickupDef = PickupCatalog.GetPickupDef(options[0].pickupIndex);
				Color baseColor = pickupDef.baseColor;
				Color darkColor = pickupDef.darkColor;
				coloredImages.ForEachTry(x => x.color *= baseColor);
				darkColoredImages.ForEachTry(x => x.color *= darkColor);
			}
			for (int i = 0; i < options.Length; i++)
			{
				PickupDef pickupDef = PickupCatalog.GetPickupDef(options[i].pickupIndex);
				MPButton mpbutton = elements[i];
				int columns = i - i % maxColumnCount;
				int rows = i % maxColumnCount;
				int numUp = rows - maxColumnCount;
				int numLeft = rows - 1;
				int numRight = rows + 1;
				int numDown = rows + maxColumnCount;
				Navigation navigation = mpbutton.navigation;
				navigation.mode = Navigation.Mode.Explicit;
				if (numLeft >= 0)
				{
					MPButton selectOnLeft = elements[columns + numLeft];
					navigation.selectOnLeft = selectOnLeft;
				}
				if (numRight < maxColumnCount && columns + numRight < options.Length)
				{
					MPButton selectOnRight = elements[columns + numRight];
					navigation.selectOnRight = selectOnRight;
				}
				if (columns + numUp >= 0)
				{
					MPButton selectOnUp = elements[columns + numUp];
					navigation.selectOnUp = selectOnUp;
				}
				if (columns + numDown < options.Length)
				{
					MPButton selectOnDown = elements[columns + numDown];
					navigation.selectOnDown = selectOnDown;
				}
				mpbutton.navigation = navigation;
				PickupDef pickup = PickupCatalog.GetPickupDef(options[i].pickupIndex);
				Image image = mpbutton.GetComponent<ChildLocator>().FindChild("Icon").GetComponent<Image>();
				if (options[i].available)
				{
					image.color = Color.white;
					image.sprite = ((pickup != null) ? pickup.iconSprite : null);
					mpbutton.interactable = true;
				}
				else
				{
					image.color = Color.gray;
					image.sprite = sprite;
					mpbutton.interactable = false;
				}

				var itemDef = ItemCatalog.GetItemDef(pickupDef.itemIndex);
				var equipDef = EquipmentCatalog.GetEquipmentDef(pickupDef.equipmentIndex);
				string nameToken = itemDef ? itemDef.nameToken : equipDef.nameToken;
				TooltipProvider tooltipProvider = mpbutton.gameObject.AddComponent<TooltipProvider>();
				tooltipProvider.titleToken = nameToken;
				tooltipProvider.bodyToken = itemDef ? itemDef.descriptionToken : equipDef.descriptionToken;
				tooltipProvider.titleColor = itemDef ? ColorCatalog.GetColor(itemDef.colorIndex == ColorCatalog.ColorIndex.None ? itemDef._itemTierDef.colorIndex : itemDef.colorIndex) : ColorCatalog.GetColor(equipDef.colorIndex);
				tooltipProvider.bodyColor = Color.gray;
			}
		}
	}
}
