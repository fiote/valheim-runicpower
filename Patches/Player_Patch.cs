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
			__instance.GetExtendedData().Load();
		}
	}

	[HarmonyPatch(typeof(Player), "Save")]
	public static class Player_Save_Patch {
		public static void Prefix(Player __instance) {
			__instance.GetExtendedData().Save();
		}
	}

	[HarmonyPatch(typeof(Player), "Awake")]
	public static class Player_Awake_Patch {
		public static void Postfix(Player __instance) {
			__instance.UpdateSpellBars();
		}
	}

	[HarmonyPatch(typeof(Player), "UseHotbarItem")]
	public static class Player_UseHotbarItem_Patch {
		public static bool Prefix(Player __instance, int index) {
			var item = SpellsBar.GetSpellHotKeyItem(__instance, index, true);
			return (item == null);
		}
	}
}
