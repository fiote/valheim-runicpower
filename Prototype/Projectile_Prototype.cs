using RunicPower.Core;
using System;

namespace RunicPower.Patches {

	public static class Projectile_Prototype {
		public static Projectile_Extended ExtendedProjectile(this Projectile __instance, Boolean create) {
			var ext = __instance.gameObject.GetComponent<Projectile_Extended>();
			if (ext == null && create) ext = __instance.gameObject.AddComponent<Projectile_Extended>();
			return ext;
		}

		public static Rune GetRuneConfig(this Projectile self) {
			return self.ExtendedProjectile(false)?.rune;
		}

		public static void SetRune(this Projectile self, Rune rune) {
			self.ExtendedProjectile(true)?.SetRune(rune);
		}
	}
}
