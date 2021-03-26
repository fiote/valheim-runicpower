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

	[HarmonyPatch(typeof(Character), "UpdateGroundContact")]
	public static class Character_UpdateGroundContact_Patch {
		static void Prefix(Character __instance, float dt) {
			if (!__instance.IsPlayer()) return;
			var player = __instance as Player;
			var ext = player.GetExtendedData();
			var runes = player.GetRunes();
			var prevented = runes?.Find(rune => rune.effect?.ignoreFallDamage == true);
			if (prevented != null) ext.isNotAPlayerRightNow = true;
		}

		static void Postfix(Character __instance, float dt) {
			if (!__instance.IsPlayer()) return;
			var player = __instance as Player;
			var ext = player.GetExtendedData();
			ext.isNotAPlayerRightNow = false;
		}
	}

	[HarmonyPatch(typeof(Character), "ApplyDamage")]
	public static class Character_ApplyDamage_Patch {
		static void Prefix(Character __instance, ref HitData hit, bool showDamageText, bool triggerEffects, HitData.DamageModifier mod = HitData.DamageModifier.Normal) {
			var runes = __instance.GetRunes();
			foreach (var rune in runes) rune.ModifyAppliedDamage(ref hit);
		}
	}
}