using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Achievements;
using RoR2.Skills;
using System;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Shifter
{
    public static class Achievements
    {
        internal static UnlockableDef bossKillUnlock;
        internal static UnlockableDef billionDamageUnlock;
        internal static UnlockableDef endingUnlock;
        internal static UnlockableDef unlockAll;
		public static void RegisterUnlockables()
        {
            bossKillUnlock = NewUnlockable<Achievements.BossKillUnlockable>("BOSSKILL", Prefabs.Load<SkillDef>("RoR2/DLC1/VoidSurvivor/FireCorruptBeam.asset").icon, MainPlugin.SURVIVORNAMEKEY + ": Carnage", "As " + MainPlugin.SURVIVORNAME + ", kill Mithrix 10 times in a single run.");
			billionDamageUnlock = NewUnlockable<Achievements.BillionDamageUnlockable>("BILLIONDMG", Assets.MainAssetBundle.LoadAsset<Sprite>("texVoidSurvivorSkillIcons_8"), MainPlugin.SURVIVORNAMEKEY + ": Savagery", "As " + MainPlugin.SURVIVORNAME + ", deal 1,000,000,000 damage in a single run.");
			endingUnlock = NewUnlockable<Achievements.EarlyVoidEndingAchievement>("ENDING", Assets.MainAssetBundle.LoadAsset<Sprite>("texVoidSurvivorSkillIcons_2"), MainPlugin.SURVIVORNAMEKEY + ": Void Deep", "As " + MainPlugin.SURVIVORNAME + ", escape the Planetarium before the timer hits 25 minutes.");
			unlockAll = NewUnlockable<Achievements.UnlockAllAchievement>("UNLOCKALL", Assets.MainAssetBundle.LoadAsset<Sprite>("texVoidSurvivorSkillIcons_3"), MainPlugin.SURVIVORNAMEKEY + ": Completionist", "Unlock all other skills.");
		}
        static UnlockableDef NewUnlockable<T>(string AchievementIdentifier, Sprite Icon, string Title, string Description) where T : BaseAchievement
        {
            string IDKey = "ACHIEVEMENT_" + MainPlugin.SURVIVORNAMEKEY + "_";
            var unlock = ScriptableObject.CreateInstance<UnlockableDef>();
            string langName = IDKey + AchievementIdentifier + "_NAME";
            string langDesc = IDKey + AchievementIdentifier + "_DESCRIPTION";
            LanguageAPI.Add(langName, Title);
            LanguageAPI.Add(langDesc, Description);
            var s = new Func<string>(() => Language.GetStringFormatted("UNLOCKED_FORMAT", new object[]
            {
                Language.GetString(langName),
                Language.GetString(langDesc)
            }));
            Type type = typeof(T);

            unlock.cachedName = IDKey + AchievementIdentifier + "_UNLOCKABLE_ID";
            unlock.getHowToUnlockString = s;
            unlock.getUnlockedString = s;
            unlock.achievementIcon = Icon;
            unlock.sortScore = 200;
            unlock.hidden = false;
            ContentAddition.AddUnlockableDef(unlock);
            return unlock;
        }
        [RegisterAchievement(MainPlugin.SURVIVORNAMEKEY + "_BOSSKILL", "ACHIEVEMENT_" + MainPlugin.SURVIVORNAMEKEY + "_BOSSKILL_UNLOCKABLE_ID", "BossKill", typeof(BossKillServerAchievement))]
        public class BossKillUnlockable : BaseAchievement
		{
			private static readonly int requirement = 10;
			public override BodyIndex LookUpRequiredBodyIndex()
			{
				return BodyCatalog.FindBodyIndex("ShifterBody");
			}
			public override void OnBodyRequirementMet()
			{
				base.OnBodyRequirementMet();
				base.SetServerTracked(true);
			}
			public override void OnBodyRequirementBroken()
			{
				base.SetServerTracked(false);
				base.OnBodyRequirementBroken();
			}
			private class BossKillServerAchievement : BaseServerAchievement
			{
				private int killCount;
				private BodyIndex requiredVictimBodyIndex;
				public override void OnInstall()
				{
					base.OnInstall();
					requiredVictimBodyIndex = BodyCatalog.FindBodyIndex("BrotherHurtBody");
					GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal;
				}
				public override void OnUninstall()
				{
					GlobalEventManager.onCharacterDeathGlobal -= OnCharacterDeathGlobal;
					base.OnUninstall();
				}
				private void OnCharacterDeathGlobal(DamageReport damageReport)
				{
					if (damageReport.victimBodyIndex == requiredVictimBodyIndex && serverAchievementTracker.networkUser.master == damageReport.attackerMaster)
					{
						killCount++;
						Debug.LogWarning("killCount: " + killCount);
						if (BossKillUnlockable.requirement <= killCount)
						{
							base.Grant();
						}
					}
				}
			}
		}

		[RegisterAchievement(MainPlugin.SURVIVORNAMEKEY + "_BILLIONDMG", "ACHIEVEMENT_" + MainPlugin.SURVIVORNAMEKEY + "_BILLIONDMG_UNLOCKABLE_ID", null, null)]
		public class BillionDamageUnlockable : BaseAchievement
		{
			private static readonly float damageRequirement = 1000000000;
			private float currentDamageDealt;
			public override BodyIndex LookUpRequiredBodyIndex()
			{
				return BodyCatalog.FindBodyIndex("ShifterBody");
			}
			public override void OnBodyRequirementMet()
			{
				GlobalEventManager.onClientDamageNotified += this.onClientDamageNotified;
			}
			public override void OnBodyRequirementBroken()
			{
				GlobalEventManager.onClientDamageNotified -= this.onClientDamageNotified;
			}
			private void onClientDamageNotified(DamageDealtMessage message)
			{
				if (message.attacker == base.localUser.cachedBodyObject)
				{
					currentDamageDealt += message.damage;
					Debug.LogWarning("currentDamageDealt: " + currentDamageDealt);
					if (damageRequirement <= currentDamageDealt)
					{
						base.Grant();
					}
				}
			}
		}

		[RegisterAchievement(MainPlugin.SURVIVORNAMEKEY + "_ENDING", "ACHIEVEMENT_" + MainPlugin.SURVIVORNAMEKEY + "_ENDING_UNLOCKABLE_ID", null, null)]
		public class EarlyVoidEndingAchievement : BaseAchievement
		{
			private static readonly float timeRequirement = 1500f;
			public override BodyIndex LookUpRequiredBodyIndex()
			{
				return BodyCatalog.FindBodyIndex("ShifterBody");
			}
			public override void OnBodyRequirementMet()
			{
				base.OnBodyRequirementMet();
				Run.onClientGameOverGlobal += this.OnClientGameOverGlobal;
			}
			public override void OnBodyRequirementBroken()
			{
				Run.onClientGameOverGlobal -= this.OnClientGameOverGlobal;
				base.OnBodyRequirementBroken();
			}
			private void OnClientGameOverGlobal(Run run, RunReport runReport)
			{
				if (Run.instance.GetRunStopwatch() < timeRequirement && runReport.gameEnding == DLC1Content.GameEndings.VoidEnding && base.isUserAlive)
				{
					base.Grant();
				}
			}
		}

		[RegisterAchievement(MainPlugin.SURVIVORNAMEKEY + "_UNLOCKALL", "ACHIEVEMENT_" + MainPlugin.SURVIVORNAMEKEY + "_UNLOCKALL_UNLOCKABLE_ID", null, null)]
		public class UnlockAllAchievement : BaseAchievement
		{
			public override BodyIndex LookUpRequiredBodyIndex()
			{
				return BodyCatalog.FindBodyIndex("ShifterBody");
			}
            public override void OnBodyRequirementMet()
            {
                base.OnBodyRequirementMet();
				RoR2Application.onUpdate += Check;
			}
            public override void OnBodyRequirementBroken()
			{
				RoR2Application.onUpdate -= Check;
				base.OnBodyRequirementBroken();
            }
			private void Check()
			{
				if (base.userProfile.HasUnlockable(bossKillUnlock) && base.userProfile.HasUnlockable(billionDamageUnlock) && base.userProfile.HasUnlockable(endingUnlock))
				{
					base.Grant();
				}
			}
		}
	}
}
