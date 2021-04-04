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
	public static class Character_Prototype {

		public static Dictionary<string, Character_Extended> mapping = new Dictionary<string, Character_Extended>();

		public static Character_Extended ExtendedCharacter(this Character __instance) {
			var key = __instance.GetInstanceID().ToString();
			if (key == null) return null;
			var ext = mapping.ContainsKey(key) ? mapping[key] : null;
			if (ext == null) mapping[key] = ext = new Character_Extended(__instance);
			return ext;
		}

		public static bool IsInvisibleTo(this Character __instance, BaseAI monster) {
			var range = __instance.ExtendedCharacter()?.runicInvisibilityRange ?? 0;
			if (range == 0) return false;

			var dist = Vector3.Distance(__instance.transform.position, monster.transform.position);
			return (dist < range);
		}

		public static void RPC_Damage_RP(this Character __instance, long sender, HitData hit) {
			if (__instance.IsDebugFlying() || !__instance.m_nview.IsOwner() || __instance.GetHealth() <= 0f || __instance.IsDead() || __instance.IsTeleporting() || __instance.InCutscene() || (hit.m_dodgeable && __instance.IsDodgeInvincible())) {
				return;
			}
			Character attacker = hit.GetAttacker();
			if ((hit.HaveAttacker() && attacker == null) || (__instance.IsPlayer() && !__instance.IsPVPEnabled() && attacker != null && attacker.IsPlayer())) {
				return;
			}
			if (attacker != null && !attacker.IsPlayer()) {
				float difficultyDamageScale = Game.instance.GetDifficultyDamageScale(__instance.transform.position);
				hit.ApplyModifier(difficultyDamageScale);
			}
			__instance.m_seman.OnDamaged(hit, attacker);
			if (__instance.m_baseAI != null && !__instance.m_baseAI.IsAlerted() && hit.m_backstabBonus > 1f && Time.time - __instance.m_backstabTime > 300f) {
				__instance.m_backstabTime = Time.time;
				hit.ApplyModifier(hit.m_backstabBonus);
				__instance.m_backstabHitEffects.Create(hit.m_point, Quaternion.identity, __instance.transform);
			}
			if (__instance.IsStaggering() && !__instance.IsPlayer()) {
				hit.ApplyModifier(2f);
				__instance.m_critHitEffects.Create(hit.m_point, Quaternion.identity, __instance.transform);
			}
			if (hit.m_blockable && __instance.IsBlocking()) {
				__instance.BlockAttack(hit, attacker);
			}
			__instance.ApplyPushback(hit);
			if (!string.IsNullOrEmpty(hit.m_statusEffect)) {
				StatusEffect statusEffect = __instance.m_seman.GetStatusEffect(hit.m_statusEffect);
				if (statusEffect == null) {
					statusEffect = __instance.m_seman.AddStatusEffect(hit.m_statusEffect);
				}
				if (statusEffect != null && attacker != null) {
					statusEffect.SetAttacker(attacker);
				}
			}
			HitData.DamageModifiers damageModifiers = __instance.GetDamageModifiers();
			hit.ApplyResistance(damageModifiers, out var significantModifier);
			if (__instance.IsPlayer()) {
				float bodyArmor = __instance.GetBodyArmor();
				hit.ApplyArmor(bodyArmor);
				__instance.DamageArmorDurability(hit);
			}
			__instance.ApplyDamage(hit, showDamageText: true, triggerEffects: true, significantModifier);
		}

		public static void UpdateGroundContact_RP(this Character __instance, float dt) {
			if (!__instance.m_groundContact) {
				return;
			}
			__instance.m_lastGroundCollider = __instance.m_lowestContactCollider;
			__instance.m_lastGroundNormal = __instance.m_groundContactNormal;
			__instance.m_lastGroundPoint = __instance.m_groundContactPoint;
			__instance.m_lastGroundBody = (__instance.m_lastGroundCollider) ? __instance.m_lastGroundCollider.attachedRigidbody : null;
			if (!__instance.IsPlayer() && __instance.m_lastGroundBody != null && (__instance.m_lastGroundBody.gameObject.layer == __instance.gameObject.layer)) {
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
				var prevented = __instance.ExtendedCharacter()?.runicIgnoreFallDamage ?? false;
				if (!prevented) {
					HitData hitData = new HitData();
					hitData.m_damage.m_damage = Mathf.Clamp01((num - 4f) / 16f) * 100f;
					hitData.m_point = __instance.m_lastGroundPoint;
					hitData.m_dir = __instance.m_lastGroundNormal;
					__instance.Damage(hitData);
				}
			}
			__instance.ResetGroundContact();
			__instance.m_lastGroundTouch = 0f;
			__instance.m_maxAirAltitude = __instance.transform.position.y;
		}



	}
}