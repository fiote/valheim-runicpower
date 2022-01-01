using HarmonyLib;
using System;

namespace RunicPower {

	public class Destructible_Extra {
		public static void ApplyChopPickaxe(ref HitData hit) {
			try {
				Character attacker = hit.GetAttacker();
				var powers = attacker.ExtendedCharacter(true).runicPowerModifier;
				hit.m_damage.m_chop *= (100f + powers.GetByType(HitData.DamageType.Chop)) / 100f;
				hit.m_damage.m_pickaxe *= (100f + powers.GetByType(HitData.DamageType.Pickaxe)) / 100f;
			} catch (Exception) {
				return;
			}
		}
	}


	[HarmonyPatch(typeof(TreeBase), "RPC_Damage")]
	public static class TreeBase_RPC_Damage_Patch {
		static void Prefix(TreeBase __instance, long sender, ref HitData hit) {
			Destructible_Extra.ApplyChopPickaxe(ref hit);
		}
	}

	[HarmonyPatch(typeof(TreeLog), "RPC_Damage")]
	public static class TreeLog_RPC_Damage_Patch {
		static void Prefix(TreeLog __instance, long sender, ref HitData hit) {
			Destructible_Extra.ApplyChopPickaxe(ref hit);
		}
	}

	[HarmonyPatch(typeof(Destructible), "RPC_Damage")]
	public static class Destructible_RPC_Damage_Patch {
		static void Prefix(Destructible __instance, long sender, ref HitData hit) {
			Destructible_Extra.ApplyChopPickaxe(ref hit);
		}
	}

	[HarmonyPatch(typeof(MineRock), "RPC_Hit")]
	public static class MineRock_RPC_Hit_Patch {
		static void Prefix(MineRock __instance, long sender, ref HitData hit, int hitAreaIndex) {
			Destructible_Extra.ApplyChopPickaxe(ref hit);
		}
	}

	[HarmonyPatch(typeof(MineRock5), "RPC_Damage")]
	public static class MineRock5_RPC_Damage_Patch {
		static void Prefix(MineRock __instance, long sender, ref HitData hit, int hitAreaIndex) {
			Destructible_Extra.ApplyChopPickaxe(ref hit);
		}
	}
}