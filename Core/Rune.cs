using RunicPower;
using RunicPower.Core;
using RunicPower.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RuneStones.Core {
	public class Rune {
		public RuneData data;
		public Player caster;

		public float casterWeaponDmg = 0;
		public DamageTypeValues casterPowerMods = new DamageTypeValues();

		public StatusEffect statusEffect;

		public Rune(RuneData data, Player caster) {
			this.data = data;
			this.caster = caster;
			this.data.Config();
			CreateEffect();
		}

		public static Array dmgTypes = Enum.GetValues(typeof(HitData.DamageType));

		public static Dictionary<string, HitData.DamageModifier> mapDamageModifier = new Dictionary<string, HitData.DamageModifier>() {
			{"Ignore", HitData.DamageModifier.Ignore },
			{"Immune", HitData.DamageModifier.Immune },
			{"Normal", HitData.DamageModifier.Normal },
			{"Resistant", HitData.DamageModifier.Resistant },
			{"VeryResistant", HitData.DamageModifier.VeryResistant },
			{"VeryWeak", HitData.DamageModifier.VeryWeak },
			{"Weak", HitData.DamageModifier.Weak }
		};

		// ================================================================
		// EFFECT
		// ================================================================

		public void CreateEffect() {
			if (data.effect == null || data.effect.duration == 0) return;
			statusEffect = ScriptableObject.CreateInstance<StatusEffect>();
			statusEffect.m_ttl = GetDuration();
			statusEffect.name = data.name;
			statusEffect.m_name = data.name;
			statusEffect.m_category = data.name;
			statusEffect.m_cooldown = 0f;
			statusEffect.m_icon = data.itemDrop.m_itemData.m_shared.m_icons[0];
			statusEffect.SetRune(this);
		}

		public void ParseBuffs(string dsbuffs) {
			Debug.Log("PARSE BUFFS " + dsbuffs);
		}

		public string GetEffectString() {
			var buffs = new List<string>();
			GetEffectStringPart(ref buffs, "hpRegen", getHealthRegen());
			GetEffectStringPart(ref buffs, "stRegen", getStaminaRegen());
			GetEffectStringPart(ref buffs, "movement", GetMovementBonus());
			GetEffectStringPart(ref buffs, "ignore", data.effect?.ignoreFallDamage);
			GetEffectStringPart(ref buffs, "stealth", GetStealhiness());
			GetEffectStringPart(ref buffs, "exposed", GetExpose());
			GetEffectStringPart(ref buffs, "hpSteam", GetHealthSteal());

			if (data.effect.doPower.IsValued()) {
				if (data.effect.doPower.IsElemental()) {
					GetEffectStringPart(ref buffs, "power.Elemental", GetPower(HitData.DamageType.Fire));
				} else {
					foreach (var dmgType in RuneData.elTypes) GetEffectStringPart(ref buffs, "power."+dmgType, GetPower(dmgType));
				}
				if (data.effect.doPower.IsPhysical()) {
						GetEffectStringPart(ref buffs, "power.Slash", GetPower(HitData.DamageType.Physical));
				} else {
					foreach (var dmgType in RuneData.phTypes) GetEffectStringPart(ref buffs, "power." + dmgType, GetPower(dmgType));
				}
			}

			if (data.effect.doResist.IsValued()) {
				if (data.effect.doResist.IsElemental()) {
					GetEffectStringPart(ref buffs, "resist.Elemental", GetResist(HitData.DamageType.Fire));
				} else {
					foreach (var dmgType in RuneData.elTypes) GetEffectStringPart(ref buffs, "resist." + dmgType, GetResist(dmgType));
				}
				if (data.effect.doResist.IsPhysical()) {
					GetEffectStringPart(ref buffs, "resist.Slash", GetResist(HitData.DamageType.Physical));
				} else {
					foreach (var dmgType in RuneData.phTypes) GetEffectStringPart(ref buffs, "resist." + dmgType, GetResist(dmgType));
				}
			}

			var parts = new List<string>();
			parts.Add("RUNICPOWER");
			parts.Add(data.recipe.item);
			parts.Add(string.Join(";", buffs));

			return string.Join("|", parts);
		}

		public void GetEffectStringPart(ref List<string> parts, string key, float value) {
			if (value == 0) return;
			parts.Add(key+"="+value.ToString());
		}

		public void GetEffectStringPart(ref List<string> parts, string key, bool? value = false) {
			if (value == true) parts.Add(key + "=1");
		}


		public class RuneBuff {
			public string code;
			public float value;
		}

		// ================================================================
		// TOOLTIP
		// ================================================================

		public static Dictionary<string, string> mapTarget = new Dictionary<string, string>() {
			{"self", "Caster" },
			{"allies", "Close allies"},
			{"foes", "Close enemies" },
			{"projectile", "Projectile" }
		};

		public bool TooltipAppendPower(ref StringBuilder text, HitData.DamageType dmgType, string label = null) {
			var power = GetPower(dmgType);
			if (label == null) label = dmgType.ToString();
			if (power != 0) text.AppendFormat("\nIncreases <color=orange>{1}</color> power by <color=orange>{0}%</color>", power, label);
			return power != 0;
		}

		public bool TooltipAppendResist(ref StringBuilder text, HitData.DamageType dmgType, string label = null) {
			var resist = GetResist(dmgType);
			if (label == null) label = dmgType.ToString();
			if (resist != 0) text.AppendFormat("\nIncreases <color=orange>{1}</color> resistance by <color=orange>{0}%</color>", resist, label);
			return resist != 0;
		}

		public string GetTooltip(ItemDrop.ItemData item) {
			StringBuilder text = new StringBuilder(256);
			UpdateCaster();

			var complete = item != null;

			var colorClass = "white";
			if (data.archetype == "Warrior") colorClass = "#C69B6D";
			if (data.archetype == "Cleric") colorClass = "#F48CBA";
			if (data.archetype == "Rogue") colorClass = "#FFF468";
			if (data.archetype == "Wizard") colorClass = "#3FC7EB";

			if (complete) {
				text.AppendFormat("<color={0}>[{1} {2}]</color> {3}\n", colorClass, data.archetype, data.type, item.m_shared.m_description);
			}

			var fx = data.effect;

			if (fx != null) {
				if (complete) text.Append("\n-----------------------");

				// REGEN
				if (fx.healthRegen != 0) text.AppendFormat("\nHealth regen <color=orange>+{0}%</color>", getHealthRegen());
				if (fx.staminaRegen != 0) text.AppendFormat("\nStamina regen <color=orange>+{0}%</color>", getStaminaRegen());

				// MOVEMENT
				if (fx.movementBonus != 0) text.AppendFormat("\nMovement speed <color=orange>+{0}%</color>", GetMovementBonus());
				if (fx.ignoreFallDamage) text.AppendFormat("\nFall damage <color=orange>-100%</color>");

				foreach (HitData.DamageType dmgType in dmgTypes) {
					var health = GetHealHP(dmgType);
					var stamina = GetHealST(dmgType);
					var damage = GetDamage(dmgType);
					if (health != 0) text.AppendFormat("\nRecovers <color=orange>{0}</color> Health (<color=orange>{1}</color>)", health, dmgType);
					if (stamina != 0) text.AppendFormat("\nRecovers <color=orange>{0}</color> Stamina (<color=orange>{1}</color>)", stamina, dmgType);
					if (damage != 0) text.AppendFormat("\nDeals <color=orange>{0}</color> Damage (<color=orange>{1}</color>)", damage, dmgType);
				}
				if (fx.doPower.IsValued()) {
					if (fx.doPower.IsElemental()) TooltipAppendPower(ref text, HitData.DamageType.Fire, "Elemental"); else foreach (var dmgType in RuneData.elTypes) TooltipAppendPower(ref text, dmgType);
					if (fx.doPower.IsPhysical()) TooltipAppendPower(ref text, HitData.DamageType.Slash, "Physical"); else foreach (var dmgType in RuneData.phTypes) TooltipAppendPower(ref text, dmgType);
				}

				if (fx.doResist.IsValued()) {
					if (fx.doResist.IsElemental()) TooltipAppendResist(ref text, HitData.DamageType.Fire, "Elemental"); else foreach (var dmgType in RuneData.elTypes) TooltipAppendResist(ref text, dmgType);
					if (fx.doResist.IsPhysical()) TooltipAppendResist(ref text, HitData.DamageType.Slash, "Physical"); else foreach (var dmgType in RuneData.phTypes) TooltipAppendResist(ref text, dmgType);
				}

				// RANDOM EFFECTS
				if (fx.stagger) text.AppendFormat("\n<color=orange>Staggers</color> the target");
				if (fx.pushback) text.AppendFormat("\n<color=orange>Pushes</color> the target");
				if (fx.pull) text.AppendFormat("\n<color=orange>Pulls</color> the target");
				if (fx.fear) text.AppendFormat("\n<color=orange>Fears</color> the target");
				if (fx.burn) text.AppendFormat("\n<color=orange>Burns</color> the target");
				if (fx.poison) text.AppendFormat("\n<color=orange>Poison</color> the target");
				if (fx.slow) text.AppendFormat("\n<color=orange>Slows</color> the target");
				if (fx.cripple) text.AppendFormat("\n<color=orange>Cripples</color> the target");

				if (fx.stealthiness != 0) text.AppendFormat("\nBecomes invisible to foes within <color=orange>{0} meters</color>", GetStealhiness());
				if (fx.expose != 0) text.AppendFormat("\nIncreases damage taken by <color=orange>{0}%</color>", GetExpose());
				if (fx.healthBack != 0) text.AppendFormat("\nRecovers <color=orange>{0}%</color> of each attack as <color=orange>HP</color>", GetHealthSteal());

				var runeMods = GetResistanceModifiers();

				if (runeMods.Count > 0) {
					string runeModsToString = SE_Stats.GetDamageModifiersTooltipString(runeMods);
					text.Append(runeModsToString);
				}

				if (complete) {
					text.Append("\n-----------------------");

					text.AppendFormat("\n");
					var duration = GetDuration();
					var texttime = (duration == 0) ? "Instant" : duration + "s";
					text.AppendFormat("\nDuration: <color=orange>{0}</color>", texttime);

					if (fx.target != "") {
						var dstarget = mapTarget[fx.target];
						if (fx.target == "projectile") {
							if (data.projectile.explode) {
								text.AppendFormat("\nTarget: <color=orange>{0} (Explosive)</color> ({1} meters)", dstarget, GetSkilledRange(data.rangeExplosion));
							} else {
								text.AppendFormat("\nTarget: <color=orange>{0}</color>", dstarget);
							}
						} else {
							if (fx.target == "self") {
								text.AppendFormat("\nTarget: <color=orange>{0}</color>", dstarget);
							} else {
								text.AppendFormat("\nTarget: <color=orange>{0}</color> ({1} meters)", dstarget, GetSkilledRangeAOE(fx.target));
							}
						}
					}
				}
			}

			return text.ToString();
		}

		public string GetEffectTooltip() {
			return GetTooltip(null);
		}

		// ================================================================
		// SETTERS
		// ================================================================

		public void SetCaster(Player player) {
			caster = player;
			UpdateCaster();
		}

		public void UpdateCaster() {
			if (caster == null) return;
			var dmg = caster.GetCurrentWeapon().GetDamage();
			var runes = caster.GetRunes();
			casterWeaponDmg = dmg.GetTotalElementalDamage() + dmg.GetTotalPhysicalDamage();
			casterPowerMods = new DamageTypeValues();
			foreach (var rune in runes) rune.AppendPower(ref casterPowerMods);
		}

		// ================================================================
		// GETTERS
		// ================================================================

		public float GetSkill() {
			if (caster == null) return 1;

			float skill = caster.GetSkillFactor(data.skillType) * 100f;
			if (skill < 1) skill = 1;
			return skill;
		}

		public int GetDuration() {
			int value = data.effect.duration;
			int skill = (int)GetSkill() - 1;
			var multi = (100f + skill * 2) / 100f;
			return Mathf.RoundToInt(value * multi);
		}

		public int GetHealingHP() {
			var heal = (float)data.effect?.healthRecover;
			var value = (heal / 100) * casterWeaponDmg;
			return (int)value;
		}

		public int GetHealingStamina() {
			var heal = (float)data.effect?.staminaRecover;
			var value = (heal / 100) * casterWeaponDmg;
			return (int)value;
		}

		public int GetSkilledTypedValue(DamageTypeValues source, HitData.DamageType dmgType, float skillMultiplier, float weaponMultiplier, float? capValue = null) {
			float skill = GetSkill();
			var value = source.GetByType(dmgType) / 100f;
			if (value == 0) return 0;
			// each skill level increases damage by x
			var skilled = value * skillMultiplier * skill;
			// each skill level increases damage by +x% of weapon damage
			var weapon = casterWeaponDmg * skill * weaponMultiplier / 100f;
			// getting the total base value
			var total = skilled + weapon;
			// applying power modifiers
			var modifier = casterPowerMods.GetByType(dmgType);
			var modified = total * (100 + modifier) / 100f;
			// checking for cap values
			if (capValue != null && modified > capValue) modified = (float)capValue;
			// returning a rounded value
			return Mathf.RoundToInt(modified);
		}

		public int GetSkilledValue(float value, float skillMultiplier, float? capValue = null) {
			float skill = GetSkill();
			if (value == 0) return 0;
			// each skill level increases damage by x
			var skilled = value * skillMultiplier * skill;
			// checking for cap values
			if (capValue != null && skilled > capValue) skilled = (float)capValue;
			// returning a rounded value
			return Mathf.RoundToInt(skilled);
		}

		public int GetSkilledRange(float value) {
			int skill = (int)GetSkill() - 1;
			var multi = (100f + skill) / 100f;
			return Mathf.RoundToInt(value * multi);
		}

		public int GetSkilledRangeAOE(string type) {
			var range = (type == "allies") ? data.rangeAOEallies : data.rangeAOEfoes;
			return GetSkilledRange(range);
		}

		private float GetDamage(HitData.DamageType dmgType) {
			// level 1: 3f + 5% of weapon
			// level 10: 30f + 20% of weapon
			// level 100: 300f + 200% of weapon
			return GetSkilledTypedValue(data.effect.doDamage, dmgType, 2f, 2f);
		}

		private float GetHealHP(HitData.DamageType dmgType) {
			// level 1: 7f + 2% of weapon
			// level 10: 70f + 20% of weapon
			// level 100: 700f + 200% of weapon
			return GetSkilledTypedValue(data.effect.doHealHP, dmgType, 6f, 1f);
		}

		private float GetHealST(HitData.DamageType dmgType) {
			// level 1: 7f + 2% of weapon
			// level 10: 70f + 20% of weapon
			// level 100: 700f + 200% of weapon
			return GetSkilledTypedValue(data.effect.doHealST, dmgType, 6f, 1f);
		}

		private int GetPower(HitData.DamageType dmgType) {
			// level 1: +2%
			// level 2: +4%
			// level 10: +20%
			// level 50: +100%
			// level 100: +200%
			return GetSkilledTypedValue(data.effect.doPower, dmgType, 2f, 0f);
		}

		private int GetResist(HitData.DamageType dmgType) {
			// level 1: +2%
			// level 2: +4%
			// level 10: +20%
			// level 50: +100%
			return GetSkilledTypedValue(data.effect.doResist, dmgType, 2f, 0f, 100f);
		}

		private float GetMovementBonus() {
			// level 1: +1%
			// level 2: +2%
			// level 10: +10%
			// level 50: +50%
			return GetSkilledValue((float)data.effect.movementBonus / 100f, 1f, 50f);
		}

		public float GetHealthSteal() {
			// level 1: +1%
			// level 2: +2%
			// level 10: +10%
			// level 50: +50%
			// level 100: +100%
			return GetSkilledValue((float)data.effect.healthBack / 100f, 1f);
		}

		public float GetExpose() {
			return GetSkilledValue((float)data.effect.expose / 100f, 2f);
		}

		public float GetStealhiness() {
			return GetSkilledValue((float)data.effect.stealthiness / 100f, 1f, 100f);
		}

		public List<HitData.DamageModPair> GetResistanceModifiers() {
			if (data.resistanceModifiers == null) {
				data.resistanceModifiers = new List<HitData.DamageModPair>();
				GetResistanceModifier(HitData.DamageType.Blunt, data.effect?.physicalResitance);
				GetResistanceModifier(HitData.DamageType.Pierce, data.effect?.physicalResitance);
				GetResistanceModifier(HitData.DamageType.Slash, data.effect?.physicalResitance);
				GetResistanceModifier(HitData.DamageType.Fire, data.effect?.elementalResistance);
				GetResistanceModifier(HitData.DamageType.Frost, data.effect?.elementalResistance);
				GetResistanceModifier(HitData.DamageType.Lightning, data.effect?.elementalResistance);
				GetResistanceModifier(HitData.DamageType.Poison, data.effect?.elementalResistance);
				GetResistanceModifier(HitData.DamageType.Spirit, data.effect?.elementalResistance);
			}
			return data.resistanceModifiers;
		}

		public float getHealthRegen() {
			if (data.effect == null) return 0;
			return data.effect.healthRegen * 100;
		}

		public float getStaminaRegen() {
			if (data.effect == null) return 0;
			return data.effect.staminaRegen * 100;
		}

		// ================================================================
		// MODIFY
		// ================================================================

		public void ModifyStaminaRegen(Player player, ref float staminaMultiplier) {
			if (data.effect == null) return;
			if (data.effect.staminaRegen != 0) staminaMultiplier += data.effect.staminaRegen + 1;
		}

		public void ModifyEquipmentMovement(Player player, ref float equipmentMovement) {
			if (data.effect == null) return;
			equipmentMovement += GetMovementBonus() / 100f;
		}

		public void AppendPower(ref DamageTypeValues power) {
			if (data.effect == null) return;
			// MULTIPLIERS
			foreach (HitData.DamageType dmgType in dmgTypes) {
				var value = GetPower(dmgType);
				if (value != 0) power.AddByType(dmgType, value);
			}
		}

		public void ModifyAppliedDamage(ref HitData hitData) {
			ModifyAppliedDamageByType(HitData.DamageType.Blunt, ref hitData.m_damage.m_blunt);
			ModifyAppliedDamageByType(HitData.DamageType.Pierce, ref hitData.m_damage.m_pierce);
			ModifyAppliedDamageByType(HitData.DamageType.Slash, ref hitData.m_damage.m_slash);
			ModifyAppliedDamageByType(HitData.DamageType.Fire, ref hitData.m_damage.m_fire);
			ModifyAppliedDamageByType(HitData.DamageType.Frost, ref hitData.m_damage.m_frost);
			ModifyAppliedDamageByType(HitData.DamageType.Lightning, ref hitData.m_damage.m_lightning);
			ModifyAppliedDamageByType(HitData.DamageType.Poison, ref hitData.m_damage.m_poison);
			ModifyAppliedDamageByType(HitData.DamageType.Spirit, ref hitData.m_damage.m_spirit);
		}

		public void ApplyHealthSteal(HitData hit, Character attacker) {
			var steal = GetHealthSteal();
			if (steal != 0) {
				var totalf = hit.GetTotalDamage();
				var back = totalf * steal / 100f;
				attacker.Heal(back);
			}
		}

		public void ModifyAppliedDamageByType(HitData.DamageType type, ref float damage) {
			var resist = GetResist(type);
			var expose = GetExpose();

			var original = damage;

			if (resist != 0) {
				damage = original * (100f - resist) / 100f;
				Debug.Log("damage " + type + " resistance reduced " + original + " to " + damage + "(-" + resist + "%)");
			}

			if (expose != 0) {
				damage = damage * (100f + expose) / 100f;
				Debug.Log("damage " + type + " expose increased " + original + " to " + damage + "(+" + expose + "%)");
			}
		}

		public void GetResistanceModifier(HitData.DamageType type, String value) {
			if (value == null || value == "") return;
			if (!mapDamageModifier.ContainsKey(value)) return;
			data.resistanceModifiers.Add(new HitData.DamageModPair() { m_type = type, m_modifier = mapDamageModifier[value] });
		}

		// ================================================================
		// APPLY
		// ================================================================

		public void ApplyEffectOnCharacter(Character target) {
			if (target == null) return;

			IDestructible destructable = null;
			Player player = null;

			try { destructable = target; } catch (Exception) { }
			try { player = (Player)target; } catch (Exception) { }


			Debug.Log("==================================");
			Debug.Log("ApplyEffectOn " + target);

			// ===== RESTORING HEALTH ========================================

			float healHP = 0;
			foreach (HitData.DamageType dmgType in dmgTypes) healHP += GetHealHP(dmgType);

			if (healHP != 0) {
				Debug.Log("RUNE IS RECOVERING HEALTH");
				target.Heal(healHP, true);
			}

			// ===== RESTORING STAMINA =======================================

			float healST = 0;
			foreach (HitData.DamageType dmgType in dmgTypes) healST += GetHealST(dmgType);

			if (healST != 0) {
				Debug.Log("RUNE IS RECOVERING STAMINA");
				player?.UseStamina(healST * -1);
			}

			// ===== DEALING DAMAGE =================================

			var hitDamage = new HitData();

			if (data.effect.DoDamage()) {
				Debug.Log("RUNE IS DOING DAMAGE");
				hitDamage.m_damage.m_blunt = GetDamage(HitData.DamageType.Blunt);
				hitDamage.m_damage.m_pierce = GetDamage(HitData.DamageType.Pierce);
				hitDamage.m_damage.m_slash = GetDamage(HitData.DamageType.Slash);
				hitDamage.m_damage.m_fire = GetDamage(HitData.DamageType.Fire);
				hitDamage.m_damage.m_frost = GetDamage(HitData.DamageType.Frost);
				hitDamage.m_damage.m_lightning = GetDamage(HitData.DamageType.Lightning);
				hitDamage.m_damage.m_poison = GetDamage(HitData.DamageType.Poison);
				hitDamage.m_damage.m_spirit = GetDamage(HitData.DamageType.Spirit);
				destructable.Damage(hitDamage);
			}

			// ===== APPLYING ELEMENTAL EFFECTS =====================

			if (data.effect.burn) {
				Debug.Log("RUNE IS APPLYING BURN");
				var burning = ObjectDB.instance.m_StatusEffects.Find(x => x.name == "Burning").Clone() as SE_Burning;
				burning.m_ttl = GetDuration();
				burning.m_damageInterval = 1f;
				// no spirit damage, this is a simple fire burn
				burning.m_damage.m_spirit = 0;
				burning.m_damage.m_fire = hitDamage.GetTotalDamage() / 10f;
				target.m_seman.AddStatusEffect(burning);
			}

			if (data.effect.slow) {
				Debug.Log("RUNE IS APPLYING SLOW");
				var frost = ObjectDB.instance.m_StatusEffects.Find(x => x.name == "Frost").Clone() as SE_Frost;
				frost.m_ttl = GetDuration();
				// no damage, just slow
				frost.m_freezeTimeEnemy = 0;
				frost.m_freezeTimePlayer = 0;
				target.m_seman.AddStatusEffect(frost);
			}

			if (data.effect.poison) {
				Debug.Log("RUNE IS APPLYING POISON");
				var poison = ObjectDB.instance.m_StatusEffects.Find(x => x.name == "Poison").Clone() as SE_Poison;
				poison.m_ttl = GetDuration();
				poison.m_damageInterval = 1f;
				poison.m_damagePerHit = hitDamage.GetTotalDamage() / 10f;
				poison.m_damageLeft = poison.m_damageInterval * poison.m_damagePerHit;

				target.m_seman.AddStatusEffect(poison);
			}

			// ===== STAGGER ========================================

			if (data.effect.stagger == true) {
				Debug.Log("RUNE IS STAGGERING");
				var staggerDir = -caster.m_lookDir;
				target.Stagger(staggerDir);
			}

			// ===== PUSH BACK ======================================

			if (data.effect.pushback == true) {
				Debug.Log("RUNE IS PUSHING BACK");
				var hitPushback = new HitData();
				hitPushback.m_pushForce = 500f;
				var from = caster.gameObject.transform.position;
				var to = target.gameObject.transform.position;
				hitPushback.m_dir = (to - from).normalized;
				// TODO: RPC_Pushback
				target.ApplyPushback(hitPushback);
			}

			// ===== ADDING AS EFFFECT ======================================

			// if there is a duration, it means it'a buff. So let's apply it to targets
			if (data.effect.duration > 0) {
				Debug.Log("RUNE IS ADDING SOME (DE)BUFF TO " + target.name);
				var fxString = GetEffectString();
				Debug.Log("fxString = " + fxString);
				target.m_seman.AddStatusEffect(fxString, true);
			}
		}

		public void ApplyEffectOnSeman(SEMan seman) {
			ApplyEffectOnCharacter(seman?.m_character);
		}

		public void ApplyProjectile(Collider collider, Vector3 hitPoint) {
			Debug.Log("ApplyProjectile");
			if (data.effect == null) {
				Debug.Log("effect == null");
				return;
			}
			if (data.projectile == null) {
				Debug.Log("projectile == null");
				return;
			}
			if (collider == null) {
				Debug.Log("collider == null");
				return;
			}

			GameObject obj = Projectile.FindHitObject(collider);
			var character = GetGameObjectCharacter(obj);
			if (character == caster) {
				Debug.Log("character is player, ignoring projectile hit");
				return;
			}

			var semans = new List<SEMan>();

			if (data.projectile.explode) {
				semans = GetFoesAround(hitPoint, GetSkilledRange(data.rangeExplosion));
			} else {
				semans.Add(character?.m_seman);
			}

			foreach (var seman in semans) ApplyEffectOnSeman(seman);
		}

		public Character GetGameObjectCharacter(GameObject obj) {
			IDestructible destructible = obj.GetComponent<IDestructible>();
			if (destructible == null) return null;
			return destructible as Character;
		}

		public List<SEMan> GetTargetsAroundCharacter(string type, Character character, float range) {
			var targets = new List<SEMan>();

			if (type == "self") {
				if (character != null) targets.Add(character.m_seman);
			}

			if (type == "foes") {
				targets = GetFoesAround(character.transform.position, GetSkilledRangeAOE(type));
			}

			if (type == "allies") {
				targets = GetAlliesAround(character.transform.position, GetSkilledRangeAOE(type));
			}

			return targets;
		}

		public List<SEMan> GetFoesAround(Vector3 center, float range) {
			var characters = new List<Character>();

			foreach (Character character in Character.GetAllCharacters()) {
				var enemy = character.IsMonsterFaction() || character.IsBoss();
				if (character.IsTamed()) enemy = false;
				if (enemy) characters.Add(character);
			}

			return GetInRange(characters, center, range);
		}

		public List<SEMan> GetAlliesAround(Vector3 center, float range) {
			var players = new List<Player>();
			Player.GetPlayersInRange(center, range, players);

			var targets = new List<SEMan>();
			foreach (var p in players) targets.Add(p.m_seman);
			return targets;
		}

		public List<SEMan> GetInRange(List<Character> characters, Vector3 center, float range) {
			var targets = new List<SEMan>();

			foreach (var character in characters) {
				var pos = character.transform.position;
				var dist = Vector3.Distance(pos, center);
				if (Vector3.Distance(character.transform.position, center) < range) {
					targets.Add(character.m_seman);
				}
			}

			return targets;
		}

		public void ExecFX(RuneVFX fx) {
			if (fx != null) {
				var vfxPrefab = ZNetScene.instance.GetPrefab(fx.name);
				vfxPrefab.SetActive(false);
				var vfxGo = UnityEngine.Object.Instantiate(vfxPrefab, caster.gameObject.transform.position, vfxPrefab.transform.rotation);
				vfxGo.SetActive(false);

				Component[] components = vfxGo.GetComponentsInChildren<Component>(true);

				if (fx.list.Count > 0) {
					foreach (var c in components) {
						var cname = c.name.ToLower();
						if (!fx.list.Exists(x => cname.Contains(x))) c.gameObject.SetActive(false);
					}
				}

				vfxPrefab.SetActive(true);
				vfxGo.SetActive(true);
			}
		}

		public void Cast() {
			Debug.Log("============================================");
			// letting everyone knows the caster used the rune power
			Chat.instance.SendText(Talker.Type.Shout, data.name + "!");

			// get the effects
			var custom = data.fxcustom;

			if (data.type == "Buff") {
				ExecFX(RuneData.genericVFX);
				ExecFX(RuneData.genericSFX);
			} else {
				ExecFX(data.vfx);
				ExecFX(data.sfx);
			}

			// casting RECALL

			// getting the archetype skill Id and adding experience to it
			if (data.archetype != "Generic") caster.RaiseSkill(data.skillType, 1f);

			// casting RECALL
			if (custom == "recall") {
				var prof = Game.instance.GetPlayerProfile();
				var location = prof.GetCustomSpawnPoint();
				if (location != null) this.caster.TeleportTo(location, this.caster.gameObject.transform.rotation, true);
				return;
			}

			if (data.effect == null) {
				return;
			}

			if (data.effect.target == "projectile") {
				var attack = new Attack();
				attack.m_character = caster;
				attack.m_attackHeight = 2f;
				attack.m_attackRange = 1f;
				attack.m_attackHitNoise = 0;
				attack.m_projectileVel = data.projectile.speed;
				attack.GetProjectileSpawnPoint(out var spawnPoint, out var aimDir);
				aimDir = GameCamera.instance.transform.forward;
				var hitData = new HitData();
				var projPrefab = ZNetScene.instance.GetPrefab(data.projectile.name);
				GameObject projGo = UnityEngine.Object.Instantiate(projPrefab, spawnPoint, Quaternion.LookRotation(aimDir));
				var proj = projGo.GetComponent<IProjectile>() as Projectile;
				proj.m_gravity = 0f;
				proj.m_ttl = (float)data.projectile.duration;
				proj.SetRune(this);
				proj.Setup(attack.m_character, aimDir * attack.m_projectileVel, attack.m_attackHitNoise, hitData, null);
			} else {
				// getting the targets
				var semans = GetTargetsAroundCharacter(data.effect.target, this.caster, GetSkilledRange(data.rangeAOE));
				foreach (var seman in semans) ApplyEffectOnSeman(seman);
			}
		}

	}
}
