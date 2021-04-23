using HarmonyLib;
using UnityEngine;

namespace RunicPower.Patches {

	[HarmonyPatch(typeof(Projectile), "OnHit")]
	public static class Projectile_OnHit_Patch {
		static void Prefix(Projectile __instance, Collider collider, Vector3 hitPoint, bool water) {
			var rune = __instance.GetRuneConfig();
			rune?.ApplyProjectile(collider, hitPoint);
		}
	}
}
