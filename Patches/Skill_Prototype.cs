using Common;
using HarmonyLib;
using RunicPower.Core;
using RunicPower;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Skills;

namespace RunicPower.Patches {
	public static class Skill_Prototype {

		public static bool Lower(this Skill __instance, float factor) {
			if (__instance.m_level <= 0) {
				return false;
			}

			var ccontrol = RunicPower.configClassControl.Value;
			if (__instance.m_level <= ccontrol) {
				return false;
			}
			float num = __instance.m_info.m_increseStep * factor;
			__instance.m_accumulator -= num;
			if (__instance.m_accumulator < 0) {
				__instance.m_level -= 1f;
				__instance.m_level = Mathf.Clamp(__instance.m_level, 0f, 100f);

				float nextLevelRequirement = __instance.GetNextLevelRequirement();
				__instance.m_accumulator += nextLevelRequirement;
				return true;
			}

			return false;
		}

	}


}
