using HarmonyLib;

namespace RunicPower {

	[HarmonyPatch(typeof(BaseAI), "CanHearTarget")]
	public static class BaseAI_CanHearTarget_Patch {
		static bool Prefix(BaseAI __instance, Character target, ref bool __result) {
			var invisible = target.IsInvisibleTo(__instance);
			if (!invisible) return true;
			__result = false;
			return false;
		}
	}

	[HarmonyPatch(typeof(BaseAI), "CanSeeTarget", typeof(Character))]
	public static class BaseAI_CanSeeTarget_Patch {
		static bool Prefix(BaseAI __instance, Character target, ref bool __result) {
			var invisible = target.IsInvisibleTo(__instance);
			if (!invisible) return true;
			__result = false;
			return false;
		}
	}
}