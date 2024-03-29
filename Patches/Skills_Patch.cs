﻿using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Skill = Skills.Skill;

namespace RunicPower {

	[HarmonyPatch(typeof(Skills), "GetSkillList")]
	public static class Skills_GetSkillList_Patch {
		public static void Postfix(Skills __instance, ref List<Skill> __result) {
			var ids = new List<Skills.SkillType>();
			RunicPower.listofCSkills.ForEach(cskill => ids.Add(cskill.GetSkillType()));
			__result = __result.OrderBy(o => o.m_info != null && ids.Contains(o.m_info.m_skill)).ThenBy(o => o.m_info != null ? o.m_info.m_description : "").ToList().FindAll(o => o.m_info != null);
		}
	}

	[HarmonyPatch(typeof(Skills), "Save")]
	public static class Skills_Save_Patch {
		public static void Prefix(Skills __instance) {
			var toRemove = new List<Skills.SkillType>();
			foreach (KeyValuePair<Skills.SkillType, Skill> skillDatum in __instance.m_skillData) {
				var sk = skillDatum.Value;
				if (sk.m_info == null) toRemove.Add(skillDatum.Key);
			}
			foreach (var key in toRemove) __instance.m_skillData.Remove(key);
		}
	}
}
