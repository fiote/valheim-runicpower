using RunicPower.Core;
using System.Collections.Generic;

namespace RunicPower.Patches {
	public static class StatusEffect_Prototype {

		public static Dictionary<string, StatusEffect_Extended> mapping = new Dictionary<string, StatusEffect_Extended>();

		public static StatusEffect_Extended ExtendedStatusEffect(this StatusEffect __instance, bool create = false) {
			var key = __instance.GetInstanceID().ToString();
			if (key == null) return null;
			var ext = mapping.ContainsKey(key) ? mapping[key] : null;
			if (ext == null && create) {
				mapping[key] = ext = new StatusEffect_Extended();
			}
			return ext;
		}

		public static void SetRune(this StatusEffect __instance, Rune rune) {
			var key = __instance.GetInstanceID().ToString();
			if (rune == null) {
				mapping.Remove(key);
			} else {
				__instance.ExtendedStatusEffect(true).rune = rune;
			}
		}

		public static Rune GetRune(this StatusEffect __instance) {
			return __instance.ExtendedStatusEffect()?.rune;
		}
	}
}
