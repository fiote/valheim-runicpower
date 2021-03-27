using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Skill = Skills.Skill;

namespace RunicPower {

	[HarmonyPatch(typeof(Skills), "GetSkillList")]
	public static class Skills_GetSkillList_Patch {
		public static void Postfix(Skills __instance, ref List<Skill> __result) {

			var ids = new List<Skills.SkillType>();
			foreach (var cskill in RunicPower.cskills) ids.Add(cskill.GetSkillType());
			__result = __result.OrderBy(o => ids.Contains(o.m_info.m_skill)).ThenBy(o => o.m_info.m_description).ToList();

		}
	}
}
