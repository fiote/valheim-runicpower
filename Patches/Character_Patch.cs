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
	public class ExtendedCharacterData {
		public RunicStagger runicStagger = new RunicStagger();
		public DamageTypeValues powerModifiers = new DamageTypeValues();

		public class RunicStagger {
			public bool active;
			public float start;
			public float time;
			public float replay = 2f;
			public Vector3 direction;
		}
	}

	[HarmonyPatch(typeof(Character), "UpdateGroundContact")]
	public static class Character_UpdateGroundContact_Patch {
		static bool Prefix(Character __instance, float dt) {
			__instance.UpdateGroundContact_Custom();
			return false;
		}
	}

	[HarmonyPatch(typeof(Character), "UpdateStagger")]
	public static class Character_UpdateStagger_Patch {
		static void Prefix(Character __instance, float dt) {
			__instance.UpdateRunicStagger(dt);
		}
	}

	[HarmonyPatch(typeof(Character), "ApplyDamage")]
	public static class Character_ApplyDamage_Patch {
		static void Prefix(Character __instance, ref HitData hit, bool showDamageText, bool triggerEffects, HitData.DamageModifier mod = HitData.DamageModifier.Normal) {
			Debug.Log("==============================================================");
			Debug.Log("target "+__instance.name + " got damage-applied");

			var runes = __instance.m_seman?.GetRunes();
			if (runes == null || runes.Count == 0) {
				Debug.Log("target"+ __instance + " got no runes");
				return;
			}

			foreach (var rune in runes) {
				Debug.Log("trying modify applied damage (expose? " + rune.GetExpose() + ") (resist: " + rune.effect?.doResist.ToString() + ")");
				rune.ModifyAppliedDamage(ref hit);
			}
		}
	}

	public static class CharacterExtensions {

		public static Dictionary<string, ExtendedCharacterData> mapping = new Dictionary<string, ExtendedCharacterData>();

		public static ExtendedCharacterData GetExtendedData(this Character __instance) {
			var key = __instance.GetInstanceID().ToString();
			if (key == null) return null;
			var ext = mapping.ContainsKey(key) ? mapping[key] : null;
			if (ext == null) mapping[key] = ext = new ExtendedCharacterData();
			return ext;
		}

		public static void UpdateRunicStagger(this Character __instance, float dt) {
			var ext = __instance.GetExtendedData();
			var stagger = ext.runicStagger;
			if (stagger.active) {
				stagger.time += dt;
				if (stagger.time > stagger.replay) {
					__instance.SetRunicStagger(false);
					__instance.m_zanim.SetTrigger("stagger");
				}
			}
		}

		public static float GetStealthRange(this Character __instance) {
			var runes = __instance.m_seman.GetRunes();
			var range = 0f;
			foreach (var rune in runes) if (rune.effect?.stealthiness != 0) range += rune.GetStealhiness();
			return range;
		}

		public static bool IsMagicallyStaggered(this Character c) {
			return c.GetExtendedData().runicStagger.active;
		}

		public static void SetRunicStagger(this Character __instance, bool active, Vector3 direction) {
			var ext = __instance.GetExtendedData();
			ext.runicStagger.active = active;
			if (active) {
				ext.runicStagger.start = 0;
				ext.runicStagger.time = 0;
			}
			ext.runicStagger.direction = direction;
		}

		public static void SetRunicStagger(this Character __instance, bool active) {
			var ext = __instance.GetExtendedData();
			ext.runicStagger.active = active;
		}

		public static void UpdateGroundContact_Custom(this Character __instance) {
			if (!__instance.m_groundContact) return;

			__instance.m_lastGroundCollider = __instance.m_lowestContactCollider;
			__instance.m_lastGroundNormal = __instance.m_groundContactNormal;
			__instance.m_lastGroundPoint = __instance.m_groundContactPoint;
			__instance.m_lastGroundBody = __instance.m_lastGroundCollider ? __instance.m_lastGroundCollider.attachedRigidbody : null;
			if (!__instance.IsPlayer() && __instance.m_lastGroundBody != null && __instance.m_lastGroundBody.gameObject.layer == __instance.gameObject.layer) {
				__instance.m_lastGroundCollider = null;
				__instance.m_lastGroundBody = null;
			}
			float num = Mathf.Max(0f, __instance.m_maxAirAltitude - __instance.transform.position.y);
			if (num > 0.8f) {
				if (__instance.m_onLand != null) {
					Vector3 lastGroundPoint = __instance.m_lastGroundPoint;
					if (__instance.InWater()) {
						lastGroundPoint.y = __instance.m_waterLevel;
					}
					__instance.m_onLand(__instance.m_lastGroundPoint);
				}
				__instance.ResetCloth();
			}
			if (__instance.IsPlayer() && num > 4f) {
				var canTakeFallDamage = true;

				var player = __instance as Player;
				var runes = player.m_seman.GetRunes();
				var prevented = runes?.Find(rune => rune.effect?.ignoreFallDamage == true);

				if (prevented != null) canTakeFallDamage = false;

				var fallDamage = Mathf.Clamp01((num - 4f) / 16f) * 100f;

				if (canTakeFallDamage) {
					HitData hitData = new HitData();
					hitData.m_damage.m_damage = fallDamage;
					hitData.m_point = __instance.m_lastGroundPoint;
					hitData.m_dir = __instance.m_lastGroundNormal;
					__instance.Damage(hitData);
				}
			}
			__instance.ResetGroundContact();
			__instance.m_lastGroundTouch = 0f;
			__instance.m_maxAirAltitude = __instance.transform.position.y;
		}


		public static DamageTypeValues GetPowerModifiers(this Character __instance) {
			var ext = __instance.GetExtendedData();
			var power = ext.powerModifiers.Reset();
			var runes = __instance.m_seman.GetRunes();
			foreach (var rune in runes) rune.AppendPower(ref power);
			return power;
		}
	}
}