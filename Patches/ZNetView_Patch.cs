using HarmonyLib;
using RunicPower.Patches;
using UnityEngine;

namespace RunicPower {

	[HarmonyPatch(typeof(ZNetView), "Awake")]
	public static class ZNetView_Awake_Patch {
		public static void Postfix(ZNetView __instance) {
			if (!__instance.IsOwner()) return;
			
			__instance.Register("SetHashCode", (long sender, int vint, string vstring) => {
				__instance.SetHashCode(vint, vstring);
			});

			__instance.Register("RPC_SetHashCode", (long sender, int vint, string vstring) => {
				__instance.SetHashCode(vint, vstring);
			});
		}
	}
}
