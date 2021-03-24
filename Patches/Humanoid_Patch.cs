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
			// no rune? stop here
			if (rune == null) return true;
			// cant consume item? stop here
			if (inventory == null) inventory = __instance.m_inventory;
			if (!__instance.ConsumeItem(inventory, item)) return true;
			// casting it
			rune.Cast(__instance);
			// returning false. no need to native code to run
			return false;
		}
	}
}