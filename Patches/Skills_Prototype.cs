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
	public static class Skills_Prototype {

		public static void LowerSkill(this Skills __instance, SkillType skillType, float factor = 1f) {
			if (skillType == SkillType.None) {
				return;
			}
			Skill skill = __instance.GetSkill(skillType);
			float level = skill.m_level;
			if (skill.Lower(factor)) {
				__instance.m_player.OnSkillLevelup(skillType, skill.m_level);
				MessageHud.MessageType type = (((int)level != 0) ? MessageHud.MessageType.TopLeft : MessageHud.MessageType.Center);
				__instance.m_player.Message(type, "Skill reduced (control) $skill_" + skill.m_info.m_skill.ToString().ToLower() + ": " + (int)skill.m_level, 0, skill.m_info.m_icon);
				Gogan.LogEvent("Game", "Leveldown", skillType.ToString(), (int)skill.m_level);
			}
		}
	}
}
