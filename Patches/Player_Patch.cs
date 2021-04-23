using HarmonyLib;
using RunicPower.Core;
using System;

namespace RunicPower.Patches {

	[HarmonyPatch(typeof(Player), "Load")]
	public static class Player_Load_Patch {
		public static void Postfix(Player __instance) {
			__instance.ExtendedPlayer(true).Load();
		}
	}

	[HarmonyPatch(typeof(Player), "Save")]
	public static class Player_Save_Patch {
		public static void Prefix(Player __instance) {
			try {
				__instance.ExtendedPlayer(true).Save();
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
			var item = SpellsBar.GetSpellHotKeyItem(__instance, index - 1, true);
			return (item == null);
		}
	}

	[HarmonyPatch(typeof(Player), "SetPlaceMode")]
	public static class Player_SetPlaceMode_Patch {
		public static void Postfix(Player __instance, PieceTable buildPieces) {
			SpellsBar.UpdateVisibility(buildPieces == null);
		}
	}

	[HarmonyPatch(typeof(Player), "UpdateMovementModifier")]
	public static class Player_UpdateMovementModifier_Patch {
		public static void Postfix(Player __instance) {
			var bonus = __instance.ExtendedCharacter(false)?.runicMoveBonus ?? 0;
			__instance.m_equipmentMovementModifier += bonus;
		}
	}
}
