using Common;
using HarmonyLib;
using RunicPower.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RunicPower.Patches {

	[HarmonyPatch(typeof(Player), "Load")]
	public static class Player_Load_Patch {
		public static void Postfix(Player __instance) {
			__instance.ExtendedPlayer().Load();
		}
	}

	[HarmonyPatch(typeof(Player), "Save")]
	public static class Player_Save_Patch {
		public static void Prefix(Player __instance) {
			try { 
				__instance.ExtendedPlayer().Save();
				RunicPower.Log("Spellsbar saved!");
			} catch (Exception) {
				RunicPower.Log("Failed to save Spellsbar!");
			};
		}
	}

	[HarmonyPatch(typeof(Player), "Awake")]
	public static class Player_Awake_Patch {
		public static void Postfix(Player __instance) {
			SpellsBar.UpdateInventory();
		}
	}

	[HarmonyPatch(typeof(Player), "UseHotbarItem")]
	public static class Player_UseHotbarItem_Patch {
		public static bool Prefix(Player __instance, int index) {
			RunicPower.Debug("UseHotbarItem Prefix");
			var item = SpellsBar.GetSpellHotKeyItem(__instance, index-1, true);
			return (item == null);
		}
	}

	[HarmonyPatch(typeof(Player), "UpdateMovementModifier")]
	public static class Player_UpdateMovementModifier_Patch {
		public static void Postfix(Player __instance) {
			var bonus = __instance.ExtendedCharacter()?.runicMoveBonus ?? 0;
			__instance.m_equipmentMovementModifier += bonus;
		}
	}
}
