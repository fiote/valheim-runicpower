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
	public static class StatusEffect_Prototype {

		public static Dictionary<string, StatusEffect_Extended> mapping = new Dictionary<string, StatusEffect_Extended>();

		public static StatusEffect_Extended ExtendedStatusEffect(this StatusEffect __instance) {

			var key = __instance.GetInstanceID().ToString();
			if (key == null) return null;
			var ext = mapping.ContainsKey(key) ? mapping[key] : null;
			if (ext == null) {
				mapping[key] = ext = new StatusEffect_Extended();
				RunicPower.Debug("ExtendedStatusEffect: " + mapping.Count);
			}
			return ext;
		}

		public static void SetRune(this StatusEffect __instance, Rune rune) {
			if (rune == null) {
				var key = __instance.GetInstanceID().ToString();
				mapping.Remove(key);
				RunicPower.Debug("ExtendedStatusEffect: " + mapping.Count);
			} else {
				var ext = __instance.ExtendedStatusEffect();
				ext.rune = rune;
			}
		}

		public static Rune GetRune(this StatusEffect __instance) {
			var ext = __instance.ExtendedStatusEffect();
			return ext?.rune;
		}

		public static string GetEffectString(this StatusEffect __instance) {
			return __instance.GetRune().GetEffectString();
		}
	}
}
