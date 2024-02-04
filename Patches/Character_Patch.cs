using HarmonyLib;

namespace RunicPower {

	[HarmonyPatch(typeof(Character), "RPC_Damage")]
	public static class Character_RPC_Damage_Patch {
		static void Prefix(Character __instance, long sender, ref HitData hit) {
			// applying the attacker power buffs, if any
			var attackPower = new Core.DamageTypeValues();
			var attacker = hit.GetAttacker();
			if (hit.HaveAttacker()) attackPower = attacker.ExtendedCharacter(true).runicPowerModifier;
			Core.Rune.ApplyModifierToHitData(attackPower, ref hit, +1);
			// applying the target resist buffs, if any
			var targetResist = __instance.ExtendedCharacter(true).runicResistModifier;
			Core.Rune.ApplyModifierToHitData(targetResist, ref hit, -1);
			// if this hit came from a rune
			if (hit.m_statusEffect == "runicDamage") {
				// stores and remove the damages that would be parsed into debuffs
				var fire = hit.m_damage.m_fire;
				var frost = hit.m_damage.m_frost;
				var poison = hit.m_damage.m_poison;
				hit.m_damage.m_fire = 0;
				hit.m_damage.m_frost = 0;
				hit.m_damage.m_poison = 0;
				// if any of those are valued
				if (fire > 0 || frost > 0 || poison > 0) {
					// create a new hit
					var newhit = new HitData();
					newhit.SetAttacker(attacker);
					newhit.m_damage.m_fire = fire;
					newhit.m_damage.m_frost = frost;
					newhit.m_damage.m_poison = poison;
					// get the target resistances so we can adjust the new hit damage
					HitData.DamageModifiers damageModifiers = __instance.GetDamageModifiers();
					newhit.ApplyResistance(damageModifiers, out var significantModifier);
					// and apply it directly to the target
					__instance.ApplyDamage(newhit, showDamageText: true, triggerEffects: true, significantModifier);
				}
			}
		}
	}
}