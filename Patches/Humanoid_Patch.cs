using Common;
using HarmonyLib;
using RunicPower.Core;
using RunicPower.Patches;
using System.Collections.Generic;
using UnityEngine;

namespace RunicPower {

	[HarmonyPatch(typeof(Humanoid), "UseItem")]
	public static class Humanoid_UseItem_Patch {
		static bool Prefix(Humanoid __instance, Inventory inventory, ItemDrop.ItemData item, bool fromInventoryGui) {
			var rune = item.GetRune();
			if (rune == null) return true;
			if (inventory == null) inventory = __instance.m_inventory;
			if (!__instance.ConsumeItem(inventory, item)) return true;
			rune.Cast(__instance);
			return false;
		}
	}

	[HarmonyPatch(typeof(Humanoid), "Pickup")]
	public static class Humanoid_Pickup_Patch {
		static void Prefix(Humanoid __instance, GameObject go) {
			if (!__instance.IsPlayer()) return;
			
			var itemDrop = go.GetComponent<ItemDrop>();
			if (itemDrop == null) return;

			var rune = itemDrop.m_itemData.GetRune();
			if (rune == null) return;

			Player.m_localPlayer.ExtendedPlayer().lootingRuneItem = itemDrop.m_itemData;
		}
		static void Postfix(Humanoid __instance, GameObject go) {
			if (!__instance.IsPlayer()) return;
			Player.m_localPlayer.ExtendedPlayer().lootingRuneItem = null;
		}
	}

	[HarmonyPatch(typeof(Humanoid), "GetInventory")]
	public static class Humanoid_GetInventory_Patch {
		public static bool Prefix(Humanoid __instance, ref Inventory __result) {
			if (!__instance.IsPlayer()) return true;

			var ext = Player.m_localPlayer.ExtendedPlayer();
			if (!ext.isSelectingItemSpellsBar) return true;

			__result = ext.spellsBarInventory;
			return false;
		}
	}
}