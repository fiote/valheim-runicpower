using HarmonyLib;
using RunicPower.Core;
using RunicPower.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static HitData;

namespace RunicPower {

	[HarmonyPatch(typeof(Character), "UpdateGroundContact")]
	public static class Character_UpdateGroundContact_Patch {
		static bool Prefix(Character __instance, float dt) {
			__instance.UpdateGroundContact_RP(dt);
			return false;
		}
	}

	[HarmonyPatch(typeof(Character), "RPC_Damage")]
	public static class Character_RPC_Damage_Patch {
		static bool Prefix(Character __instance, long sender, HitData hit) {
			if (hit.m_statusEffect != "applyRaw") return true;
			__instance.RPC_Damage_RP(sender, hit);
			return false;
		}
	}
}