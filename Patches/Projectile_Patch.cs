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

	[HarmonyPatch(typeof(Projectile), "OnHit")]
	public static class Projectile_OnHit_Patch {
		static void Prefix(Projectile __instance, Collider collider, Vector3 hitPoint, bool water) {
			RunicPower.Debug("Projectile_OnHit_Patch Prefix");
			var rune = __instance.GetRuneConfig();
			rune?.ApplyProjectile(collider, hitPoint);
		}
	}
}
