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
		static bool Prefix(Character __instance, float dt) {
			__instance.UpdateGroundContact_RP(dt);
			return false;
		}
	}

	[HarmonyPatch(typeof(Character), "ApplyDamage")]
	public static class Character_ApplyDamage_Patch {
		static void Prefix(Character __instance, ref HitData hit, bool showDamageText, bool triggerEffects, HitData.DamageModifier mod = HitData.DamageModifier.Normal) {
			RunicPower.Debug("Character_ApplyDamage_Patch " + __instance.name + " " + hit.GetTotalDamage());
			hit.GetAttacker()?.ExtendedCharacter()?.ApplyPowerModifiersToHit(ref hit);
			RunicPower.Debug("ApplyPowerModifiersToHit -> "+hit.GetTotalDamage());
			__instance?.ExtendedCharacter()?.ApplyResistModifiersToHit(ref hit);
			RunicPower.Debug("ApplyResistModifiersToHit -> " + hit.GetTotalDamage());
		}
	}
}