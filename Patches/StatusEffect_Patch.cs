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
	public class ExtendedStatusEffectData {
		public Rune rune;
	}

	public static class StatusEffectExtensions {

		public static Dictionary<string, ExtendedStatusEffectData> mapping = new Dictionary<string, ExtendedStatusEffectData>();

		public static ExtendedStatusEffectData GetExtendedData(this StatusEffect __instance) {
			var key = __instance.GetInstanceID().ToString();
			if (key == null) return null;
			var ext = mapping.ContainsKey(key) ? mapping[key] : null;
			if (ext == null) mapping[key] = ext = new ExtendedStatusEffectData();
			return ext;
		}

		public static void SetRune(this StatusEffect __instance, Rune rune) {
			var ext = __instance.GetExtendedData();
			rune.statusEffect = __instance;
			ext.rune = rune;
		}

		public static Rune GetRune(this StatusEffect __instance) {
			var ext = __instance.GetExtendedData();
			return ext?.rune;
		}
	}
}
