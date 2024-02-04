using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * THIS CODE IS FROM https://github.com/pipakin/PipakinsMods/tree/master/SkillInjector
 * I DO NOT OWN IT
 * I DO NOT CLAIM TO HAVE MADE IT
 * I only included it in my sourcecode to avoid forcing users to download aditional mods to make runicpower works
*/

namespace RunicPower.Core {

	public class SkillInjector : BaseUnityPlugin {
		private class SkillInfo {
			public Skills.SkillDef m_def;

			public string name;

			public Skills.SkillType m_template;
		}

		private static Dictionary<int, SkillInfo> m_defs = new Dictionary<int, SkillInfo>();

		public static void RegisterNewSkill(int id, string name, string description, float increment, Sprite icon, Skills.SkillType template = Skills.SkillType.None) {
			if (m_defs.ContainsKey(id)) {
				throw new Exception(Localization.instance.Localize("Id " + id + " already in use by skill $skill_" + id));
			}

			m_defs[id] = new SkillInfo {
				m_def = new Skills.SkillDef {
					m_description = description,
					m_icon = icon,
					m_increseStep = increment,
					m_skill = (Skills.SkillType)id
				},
				name = name,
				m_template = template
			};
		}

		public static Skills.SkillDef GetSkillDef(Skills.SkillType type, List<Skills.SkillDef> skills = null) {
			int key = (int)type;
			if (!m_defs.ContainsKey(key)) {
				return null;
			}

			SkillInfo skillInfo = m_defs[key];
			if (skillInfo.m_template != 0 && skills != null) {
				foreach (Skills.SkillDef skill in skills) {
					if (skill.m_skill == skillInfo.m_template) {
						skillInfo.m_def.m_description = skillInfo.m_def.m_description ?? skill.m_description;
						skillInfo.m_def.m_icon = skillInfo.m_def.m_icon ?? skill.m_icon;
					}
				}
			}

			Traverse.Create((object)Localization.instance).Method("AddWord", new object[2]
			{
				"skill_" + key,
				skillInfo.name
			}).GetValue(new object[2]
			{
				"skill_" + key,
				skillInfo.name
			});
			return skillInfo.m_def;
		}


		[HarmonyPatch(typeof(Skills), "GetSkillDef")]
		public static class SkillInjectionPatch {
			[HarmonyPostfix]
			public static void Postfix(Skills.SkillType type, ref Skills.SkillDef __result, List<Skills.SkillDef> ___m_skills) {
				if (__result == null) {
					Skills.SkillDef skillDef = GetSkillDef(type, ___m_skills);
					if (skillDef != null) {
						___m_skills.Add(skillDef);
						__result = skillDef;
					}
				}
			}
		}

		[HarmonyPatch(typeof(Skills), "IsSkillValid")]
		public static class SkillValidPatch {
			[HarmonyPostfix]
			public static void Postfix(Skills.SkillType type, ref bool __result) {
				if (!__result && m_defs.ContainsKey((int)type)) {
					__result = true;
				}
			}
		}

		[HarmonyPatch(typeof(Skills), "CheatRaiseSkill")]
		public static class CheatRaiseSkillPatch {
			[HarmonyPrefix]
			public static bool Prefix(string name, float value, Skills __instance, Player ___m_player) {
				foreach (int key in m_defs.Keys) {
					SkillInfo skillInfo = m_defs[key];
					if (skillInfo.name.ToLower() == name) {
						Skills.Skill value2 = Traverse.Create((object)__instance).Method("GetSkill", new object[1] { (Skills.SkillType)key }).GetValue<Skills.Skill>(new object[1] { (Skills.SkillType)key });
						value2.m_level += value;
						value2.m_level = Mathf.Clamp(value2.m_level, 0f, 100f);
						___m_player.Message(MessageHud.MessageType.TopLeft, "Skill incresed " + skillInfo.name + ": " + (int)value2.m_level, 0, value2.m_info.m_icon);
						Console.instance.Print("Skill " + skillInfo.name + " = " + value2.m_level);
						return false;
					}
				}

				return true;
			}
		}

		[HarmonyPatch(typeof(Skills), "CheatResetSkill")]
		public static class CheatResetSkillPatch {
			[HarmonyPrefix]
			public static bool Prefix(string name, Skills __instance, Player ___m_player) {
				foreach (int key in m_defs.Keys) {
					SkillInfo skillInfo = m_defs[key];
					if (skillInfo.name.ToLower() == name) {
						___m_player.GetSkills().ResetSkill((Skills.SkillType)key);
						return false;
					}
				}

				return true;
			}
		}
	}

}