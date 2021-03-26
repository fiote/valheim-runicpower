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
			Debug.Log("Player_Load_Postfix");
			__instance.GetExtendedData().Load();
		}
	}

	[HarmonyPatch(typeof(Player), "Save")]
	public static class Player_Save_Patch {
		public static void Prefix(Player __instance) {
			Debug.Log("Player_Save_Prefix");
			__instance.GetExtendedData().Save();
		}
	}

	[HarmonyPatch(typeof(Player), "Awake")]
	public static class Player_Awake_Patch {
		public static void Postfix(Player __instance) {
			Debug.Log("Player_Awake_Patch");
			__instance.UpdateSpellBars();
		}
	}
}
