using HarmonyLib;

namespace RunicPower {

	[HarmonyPatch(typeof(FejdStartup), "OnCharacterStart")]
	public static class FejdStartup_OnCharacterStart_Patch {
		static void Prefix(FejdStartup __instance) {
			RunicPower.UnsetMostThings();
		}
	}
}