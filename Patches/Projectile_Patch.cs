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
	public class ExtendedProjectileData {
		public Rune rune;
	}


	[HarmonyPatch(typeof(Projectile), "OnHit")]
	public static class Projectile_OnHit_Patch {
		static void Prefix(Projectile __instance, Collider collider, Vector3 hitPoint, bool water) {
			Debug.Log("Projectile_OnHit_Patch");
			var rune = __instance.GetRune();
			rune?.ApplyProjectile(collider, hitPoint);
		}
	}

	public static class ProjectileExtensions {

		public static Dictionary<string, ExtendedProjectileData> mapping = new Dictionary<string, ExtendedProjectileData>();
		public static ExtendedProjectileData GetExtendedData(this Projectile self) {
			var key = self.GetInstanceID().ToString();
			if (key == null) return null;
			var ext = mapping.ContainsKey(key) ? mapping[key] : null;
			if (ext == null) mapping[key] = ext = new ExtendedProjectileData();
			return ext;
		}

		public static Rune GetRune(this Projectile self) {
			return self.GetExtendedData().rune;
		}

		public static void SetRune(this Projectile self, Rune rune) {
			self.GetExtendedData().rune = rune;
		}
	}
}
