using HarmonyLib;

namespace RunicPower.Patches {

	[HarmonyPatch(typeof(SEMan), "OnDamaged")]
	public static class Character_OnDamaged_Patch {
		static void Prefix(SEMan __instance, ref HitData hit, Character attacker) {
			if (hit == null) return;
			if (attacker == null) return;

			var prSteal = attacker.ExtendedCharacter(false)?.runicLifeSteal ?? 0;
			if (prSteal <= 0) return;

			var totalf = hit.GetTotalDamage();
			var back = totalf * prSteal / 100f;
			attacker.Heal(back);
		}
	}

	[HarmonyPatch(typeof(SEMan), "Internal_AddStatusEffect")]
	public static class Character_Internal_AddStatusEffect_Patch {
		static bool Prefix(SEMan __instance, string name, bool resetTime) {
			var parts = name.Split('|');
			if (parts[0] != "RUNICPOWER") return true;

			var effectName = parts[1];
			var effectCaster = Player.GetAllPlayers().Find(p => p.GetZDOID().ToString() == parts[2]);
			var effectBuffs = parts[3];
			__instance.AddRunicEffect(effectName, effectCaster, effectBuffs, true);

			return false;
		}
	}

	[HarmonyPatch(typeof(SEMan), "AddStatusEffect", typeof(StatusEffect), typeof(bool))]
	public static class SEMan_AddStatusEffect_Patch {
		static void Postfix(SEMan __instance, StatusEffect statusEffect, bool resetTime, ref StatusEffect __result) {
			var rune = SEMan_Prototype.GetTempRune(statusEffect);
			if (rune == null) return;

			__result.SetRune(rune);
			__instance.m_character?.ExtendedCharacter(true)?.AddRune(rune);

			SEMan_Prototype.UnsetTemp();
		}
	}
}
