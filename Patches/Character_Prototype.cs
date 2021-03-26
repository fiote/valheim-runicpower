using HarmonyLib;
using RunicPower.Core;
using RunicPower.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RunicPower {
	public static class Character_Prototype {

		public static Dictionary<string, Character_Extended> mapping = new Dictionary<string, Character_Extended>();

		public static Character_Extended GetExtendedData(this Character __instance) {
			var key = __instance.GetInstanceID().ToString();
			if (key == null) return null;
			var ext = mapping.ContainsKey(key) ? mapping[key] : null;
			if (ext == null) mapping[key] = ext = new Character_Extended();
			return ext;
		}

		public static bool IsInvisibleTo(this Character __instance, BaseAI monster) {
			var runes = __instance.m_seman.GetRunes();
			var range = 0f;
			foreach (var rune in runes) if (rune.effect?.stealthiness != 0) range += rune.GetStealhiness();

			if (range != 0) {
				var dist = Vector3.Distance(__instance.transform.position, monster.transform.position);
				if (dist < range) return true;
			}
			return false;
		}

		public static List<Rune> GetRunes(this Character __instance) {
			var seman = __instance.m_seman;
			var runes = seman?.GetRunes();
			return (runes != null) ? runes : new List<Rune>();
		}
	}
}