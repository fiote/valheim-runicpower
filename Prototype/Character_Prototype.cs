using System;
using UnityEngine;

namespace RunicPower {
	public static class Character_Prototype {
		public static Character_Extended ExtendedCharacter(this Character __instance, Boolean create) {
			var ext = __instance.gameObject.GetComponent<Character_Extended>();
			if (ext == null && create) {
				ext = __instance.gameObject.AddComponent<Character_Extended>();
				ext.SetCharacter(__instance);
			}
			return ext;
		}

		public static bool IsInvisibleTo(this Character __instance, BaseAI monster) {
			var range = __instance.ExtendedCharacter(false)?.runicStealth ?? 0;
			if (range == 0) return false;

			var dist = Vector3.Distance(__instance.transform.position, monster.transform.position);
			return (dist < range);
		}
	}
}