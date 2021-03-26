using Common;
using HarmonyLib;
using RunicPower.Core;
using RunicPower.Patches;
using System.Collections.Generic;
using UnityEngine;

namespace RunicPower {

	[HarmonyPatch(typeof(BaseAI), "CanHearTarget")]
	public static class BaseAI_CanHearTarget_Patch {
		static bool Prefix(BaseAI __instance, Character target, ref bool __result) {
			var invisible = target.IsInvisibleTo(__instance);
			if (invisible) { __result = false; return false; }
			return true;
		}
	}

	[HarmonyPatch(typeof(BaseAI), "CanSeeTarget", typeof(Character))]
	public static class BaseAI_CanSeeTarget_Patch {
		static bool Prefix(BaseAI __instance, Character target, ref bool __result) {
			var invisible = target.IsInvisibleTo(__instance);
			if (invisible) { __result = false; return false; }
			return true;
		}
	}
}