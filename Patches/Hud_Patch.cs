using HarmonyLib;
using RunicPower.Core;

namespace RunicPower.Patches {

	[HarmonyPatch(typeof(Hud), "Awake")]
	public static class Hud_Awake_Patch {
		public static void Postfix(Hud __instance) {
			SpellsBar.CreateHotkeysBar(__instance);
		}
	}
}