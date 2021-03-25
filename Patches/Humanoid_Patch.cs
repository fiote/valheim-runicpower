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
}