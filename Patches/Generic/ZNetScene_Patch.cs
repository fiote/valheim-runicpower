using HarmonyLib;

namespace RunicPower {
	[HarmonyPatch(typeof(ZNetScene), "Awake")]
	public static class ZNetScene_Awake_Patch {
		public static bool Prefix(ZNetScene __instance) {
			RunicPower.TryRegisterPrefabs(__instance);
			return true;
		}
	}
}
