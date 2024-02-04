using RunicPower.Patches;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RunicPower.Core {
	public class Rune {
		public RuneData data;
		public Player caster;

		public float casterWeaponDmg = 0f, casterBlockPower = 0f;
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

		public Dictionary<string, float> fixedValues = new Dictionary<string, float>();

		// ================================================================
		// EFFECT
		// ================================================================

		public StatusEffect CreateEffect() {
			if (data.effect == null || data.effect.duration == 0) return null;
			statusEffect = ScriptableObject.CreateInstance<StatusEffect>();
			statusEffect.m_ttl = GetDuration();
			statusEffect.name = data.recipe.item;
			statusEffect.m_name = data.name;
			statusEffect.m_category = data.name;
			statusEffect.m_cooldown = 0f;
			statusEffect.m_icon = data.itemDrop.m_itemData.m_shared.m_icons[0];
			return statusEffect;
		}

		public void ParseBuffs(string dsbuffs) {
			fixedValues = new Dictionary<string, float>();
			var buffs = dsbuffs.Split(';');

			foreach (var buff in buffs) {
				var parts = buff.Split('=');
				var key = parts[0];
				var value = float.Parse(parts[1]);
				fixedValues[key] = value;
			}
		}

		public float GetFixed(string key) {
			if (fixedValues.ContainsKey(key)) return fixedValues[key];
			return 0;
		}

		public string GetEffectString() {
			var buffs = new List<string>();
			GetEffectStringPart(ref buffs, "hpRegen", GetHealthRegen());
			GetEffectStringPart(ref buffs, "stRegen", GetStaminaRegen());
			GetEffectStringPart(ref buffs, "movement", GetMovementBonus());
			GetEffectStringPart(ref buffs, "stealth", GetStealhiness());
			GetEffectStringPart(ref buffs, "exposed", GetExpose());
			GetEffectStringPart(ref buffs, "hpSteal", GetHealthSteal());

			foreach (HitData.DamageType dmgType in dmgTypes) {
				GetEffectStringPart(ref buffs, "power." + dmgType, GetPower(dmgType));
				GetEffectStringPart(ref buffs, "resist." + dmgType, GetResist(dmgType));
			}

			GetEffectStringPart(ref buffs, "duration", GetDuration());

			var parts = new List<string>();
			parts.Add("RUNICPOWER");
			parts.Add(data.recipe.item);
			parts.Add(caster.GetZDOID().ToString());
			parts.Add(string.Join(";", buffs));


			var fxstring = string.Join("|", parts);
			return fxstring;
		}

		public void GetEffectStringPart(ref List<string> parts, string key, float value) {
			if (value == 0) return;
			parts.Add(key + "=" + value.ToString());
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
			if (power != 0) text.AppendFormat("Increases <color=orange>{1}</color> power by <color=orange>{0:0.0}%</color>\n", power, label);
			return power != 0;
		}

		public bool TooltipAppendResist(ref StringBuilder text, HitData.DamageType dmgType, string label = null) {
			var resist = GetResist(dmgType);
			if (label == null) label = dmgType.ToString();
			if (resist != 0) text.AppendFormat("Increases <color=orange>{1}</color> resistance by <color=orange>{0:0.0}%</color>\n", resist, label);
			return resist != 0;
		}

		string cachedTooltip;
		ItemDrop.ItemData cachedItem;

		public void ClearCache() {
			cachedTooltip = null;
			cachedItem = null;
		}

		public string GetTooltip(ItemDrop.ItemData item) {
			if (item != null) {
				if (item != cachedItem) {
					ClearCache();
				}
				if (cachedTooltip != null) {
					return cachedTooltip;
				}
			}

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
			} else {
				text.AppendFormat("<color={0}>[{1} {2}]</color>\n", colorClass, data.archetype, data.type);
			}

			var fx = data.effect;

			if (fx != null) {

				if (RequiresWeapon()) {
					text.AppendFormat("\nRequires [Weapon] equipped.\n");
				}

				if (RequiresShield()) {
					text.AppendFormat("\nRequires [Shield] equipped.\n");
				}

				if (complete && data.ranked) {
					var level = Mathf.FloorToInt(GetSkillLevel());
					var extra = (level < 10) ? "-" : "";
					var max = GetMaxLevel();
					text.AppendFormat("\n--- <color=orange>Level {0} / {2}</color> ----{1}\n", level, extra, max);
				}

				// REGEN
				if (fx.healthRegen) text.AppendFormat("Health regen <color=orange>+{0:0.0}%</color>\n", GetHealthRegen() * 100f);
				if (fx.staminaRegen) text.AppendFormat("Stamina regen <color=orange>+{0:0.0}%</color>\n", GetStaminaRegen() * 100f);

				// MOVEMENT
				if (fx.movementBonus) text.AppendFormat("Movement speed <color=orange>+{0:0.0}%</color>\n", GetMovementBonus());

				var health = GetRecoverHealth();
				var stamina = GetRecoverStamina();
				if (health != 0) text.AppendFormat("Recovers <color=orange>{0:0.0}%</color> Health\n", health);
				if (stamina != 0) text.AppendFormat("Recovers <color=orange>{0:0.0}%</color> Stamina\n", stamina);

				foreach (HitData.DamageType dmgType in dmgTypes) {
					var damage = GetDamage(dmgType);
					if (damage != 0) text.AppendFormat("Deals <color=orange>{0}</color> Damage (<color=orange>{1}</color>)\n", Mathf.RoundToInt(damage), dmgType);
				}

				if (fx.power.IsValued()) {
					if (fx.power.IsElemental()) TooltipAppendPower(ref text, HitData.DamageType.Fire, "Elemental"); else foreach (var dmgType in RuneData.elTypes) TooltipAppendPower(ref text, dmgType);
					if (fx.power.IsPhysical()) TooltipAppendPower(ref text, HitData.DamageType.Slash, "Physical"); else foreach (var dmgType in RuneData.phTypes) TooltipAppendPower(ref text, dmgType);
					foreach (var dmgType in RuneData.otTypes) TooltipAppendPower(ref text, dmgType);
				}

				if (fx.resist.IsValued()) {
					if (fx.resist.IsElemental()) TooltipAppendResist(ref text, HitData.DamageType.Fire, "Elemental"); else foreach (var dmgType in RuneData.elTypes) TooltipAppendResist(ref text, dmgType);
					if (fx.resist.IsPhysical()) TooltipAppendResist(ref text, HitData.DamageType.Slash, "Physical"); else foreach (var dmgType in RuneData.phTypes) TooltipAppendResist(ref text, dmgType);
				}

				var duration = GetDuration();

				// RANDOM EFFECTS
				if (fx.stagger) text.AppendFormat("<color=orange>Staggers</color> the target\n");
				if (fx.pushback) text.AppendFormat("<color=orange>Pushes</color> the target\n");
				if (fx.pull) text.AppendFormat("<color=orange>Pulls</color> the target\n");
				if (fx.fear) text.AppendFormat("<color=orange>Fears</color> the target\n");
				if (fx.burn) {
					text.AppendFormat("<color=orange>Burns</color> the target\n");
					duration = 10;
				}
				if (fx.poison) text.AppendFormat("<color=orange>Poison</color> the target\n");
				if (fx.slow) text.AppendFormat("<color=orange>Slows</color> the target by <color=orange>{0}%</color>\n", Mathf.RoundToInt(100 - GetSlowFactor() * 100));
				if (fx.cripple) text.AppendFormat("<color=orange>Cripples</color> the target\n");

				if (fx.stealthiness) text.AppendFormat("Increase stealthiness by <color=orange>{0:0.0}%</color>\n", GetStealhiness());
				if (fx.expose) text.AppendFormat("Target takes <color=orange>{0:0.0}%</color> more damage\n", GetExpose());
				if (fx.healthBack) text.AppendFormat("Recovers <color=orange>{0:0.0}%</color> of each attack as <color=orange>HP</color>\n", GetHealthSteal());

				if (complete) {
					text.Append("-----------------------\n\n");

					var texttime = (duration == 0) ? "Instant" : duration + "s";
					text.AppendFormat("Duration: <color=orange>{0}</color>\n", texttime);

					if (fx.target != "") {
						var dstarget = mapTarget[fx.target];
						if (fx.target == "projectile") {
							if (data.projectile.explode) {
								text.AppendFormat("Target: <color=orange>{0} (Explosive)</color> ({1} meters)\n", dstarget, GetSkilledRange(data.rangeExplosion));
							} else {
								text.AppendFormat("Target: <color=orange>{0}</color>\n", dstarget);
							}
						} else {
							if (fx.target == "self") {
								text.AppendFormat("Target: <color=orange>{0}</color>\n", dstarget);
							} else {
								text.AppendFormat("Target: <color=orange>{0}</color> ({1} meters)\n", dstarget, GetSkilledRangeAOE(fx.target));
							}
						}
					}
				}
			} else {
				text.AppendLine();
			}

			var cd = GetCooldown();
			if (cd != 0) text.AppendFormat("Cooldown: <color=orange>{0} seconds</color>\n", cd);

			cachedTooltip = text.ToString();
			cachedItem = item;

			return cachedTooltip;
		}

		public string GetEffectTooltip() {
			return GetTooltip(null);
		}

		public int GetMaxLevel() {
			return data.rank * 20;
		}

		// ================================================================
		// SETTERS
		// ================================================================

		public void SetCaster(Player player) {
			if (caster == player) return;
			caster = player;
			UpdateCaster();
		}

		public void UpdateCaster() {
			if (caster == null) return;
			// caster weapon damage
			var item = caster.GetCurrentWeapon();
			var sktype = item.m_shared.m_skillType;
			caster.GetSkills().GetRandomSkillRange(out var min, out var max, sktype);
			var avg = (min + max) / 2f;

			var dmg = item.GetDamage();
			var ph = dmg.GetTotalPhysicalDamage();
			var el = dmg.GetTotalElementalDamage();
			var damage = ph + el;
			casterWeaponDmg = damage * avg;


			var shield = caster.GetCurrentBlocker();
			float skillFactor = caster.GetSkillFactor(Skills.SkillType.Blocking);
			casterBlockPower = shield.GetBlockPower(skillFactor);

			casterPowerMods = caster.ExtendedCharacter(false)?.runicPowerModifier ?? new DamageTypeValues();
		}

		// ================================================================
		// GETTERS
		// ================================================================

		public int GetMinLevel() {
			return data.ranked ? (data.rank - 1) * 20 : 1;
		}

		public float GetSkillLevel(bool limits = true) {
			if (caster == null) return 1;
			float skill = GetSkillFactor() * 100f;
			if (skill < 1) skill = 1;
			if (limits) {
				var min = 1; // GetMinLevel();
				var max = GetMaxLevel();
				skill = Mathf.Clamp(skill, min, max);
			}
			return skill;
		}

		public float GetSkillFactor() {
			return caster?.GetSkillFactor(data.skillType) ?? 0;
		}

		public int GetDuration() {
			var vfixed = GetFixed("duration");
			if (vfixed != 0) return Mathf.RoundToInt(vfixed);

			int value = data.effect.duration;
			int skill = (int)GetSkillLevel() - 1;
			var multi = (100f + skill * 2) / 100f;
			return Mathf.RoundToInt(value * multi);
		}

		public float GetSlowFactor() {
			var vmin = 10f;
			var vmax = 80f;
			var vslow = vmin + (vmax - vmin) * GetSkillLevel() / 100f;
			return (100f - vslow) / 100f;
		}

		public int GetSkilledRange(float value) {
			int skill = (int)GetSkillLevel() - 1;
			var multi = (100f + skill) / 100f;
			return Mathf.RoundToInt(value * multi);
		}

		public int GetSkilledRangeAOE(string type) {
			var range = (type == "allies") ? data.rangeAOEallies : data.rangeAOEfoes;
			return GetSkilledRange(range);
		}

		private float GetDamage(HitData.DamageType dmgType) {
			if (!data.effect.damage) return 0;
			if (data.effect.damage_type != dmgType) return 0;
			var damage = 0f;

			if (data.effect.damage_mode == DamageMode.Weapon) {
				var point = GetPointValue(data.effect, true);
				damage = casterWeaponDmg * (point / 100f);
			}

			if (data.effect.damage_mode == DamageMode.Shield) {
				var point = GetPointValue(data.effect, true);
				damage = casterWeaponDmg * (point / 100f);
			}

			if (data.effect.damage_mode == DamageMode.Skill) {
				var absolute = GetAbsoluteValue(data.effect, true);
				damage = absolute;
			}

			return damage;
		}

		private float GetRecoverHealth() {
			return GetPointValue(data.effect, data.effect.healthRecover);
		}

		private float GetRecoverStamina() {
			return GetPointValue(data.effect, data.effect.staminaRecover);
		}

		private float GetPower(HitData.DamageType dmgType) {
			var vfixed = GetFixed("power." + dmgType);
			if (vfixed != 0) return Mathf.RoundToInt(vfixed);

			var flag = data.effect.power.GetByType(dmgType);
			return GetPointValue(data.effect, flag);
		}

		private int GetCooldown() {
			var factor = GetSkillLevel() / 100f;
			var cooldown = data.cooldown;
			var value = cooldown * (1 - factor / 2);
			return Mathf.RoundToInt(value);
		}

		private float GetResist(HitData.DamageType dmgType) {
			var vfixed = GetFixed("resist." + dmgType);
			if (vfixed != 0) return Mathf.RoundToInt(vfixed);

			var flag = data.effect.resist.GetByType(dmgType);
			return GetPointValue(data.effect, flag);
		}

		private float GetMovementBonus() {
			var vfixed = GetFixed("movement");
			if (vfixed != 0) return vfixed;
			return GetPointValue(data.effect, data.effect.movementBonus);
		}

		public float GetHealthSteal() {
			var vfixed = GetFixed("hpSteal");
			if (vfixed != 0) return vfixed;
			return GetPointValue(data.effect, data.effect.healthBack);
		}

		public float GetHealthRegen() {
			var vfixed = GetFixed("hpRegen");
			if (vfixed != 0) return vfixed;
			return GetPointValue(data.effect, data.effect.healthRegen) / 100f;
		}

		public float GetStaminaRegen() {
			var vfixed = GetFixed("stRegen");
			if (vfixed != 0) return vfixed;
			return GetPointValue(data.effect, data.effect.staminaRegen) / 100f;
		}

		public float GetExpose() {
			var vfixed = GetFixed("exposed");
			if (vfixed != 0) return vfixed;
			return GetPointValue(data.effect, data.effect.expose);
		}

		public float GetStealhiness() {
			var vfixed = GetFixed("stealth");
			if (vfixed != 0) return vfixed;
			return GetPointValue(data.effect, data.effect.stealthiness);
		}

		public float GetPointValue(RuneEffect effect, decimal check) {
			return GetPointValue(effect, check != 0);
		}

		public float GetPointValue(RuneEffect effect, bool check) {
			if (!check) return 0f;

			if (effect.value != default) return (float)effect.value;

			var min = (float)effect.v1;
			var max = (float)effect.v100;
			var factor = GetSkillLevel() / 100f;
			return min + (max - min) * factor;
		}

		public float GetAbsoluteValue(RuneEffect effect, bool check) {
			if (!check) return 0f;
			var min = (float)effect.v1;
			var add = (float)effect.vx;
			var level = GetSkillLevel();
			return min + add * level;
		}

		// ================================================================
		// MODIFY
		// ================================================================

		public void ModifyStaminaRegen(ref float staminaMultiplier) {
			if (data.effect == null) return;
			var regen = GetStaminaRegen();
			if (regen != 0) staminaMultiplier += regen;
		}

		public void ModifyHealthRegen(ref float healthMultiplier) {
			if (data.effect == null) return;
			var regen = GetHealthRegen();
			if (regen != 0) healthMultiplier += regen;
		}

		public void ModifyEquipmentMovement(ref float equipmentMovement) {
			if (data.effect == null) return;
			equipmentMovement += GetMovementBonus() / 100f;
		}

		public void ModifyStealth(ref float invisibilityRange) {
			if (data.effect == null) return;
			invisibilityRange += GetStealhiness();
		}

		public void ModifyHealthSteal(ref float healthSteal) {
			if (data.effect == null) return;
			healthSteal += GetHealthSteal();
		}

		public void ModifyResist(ref DamageTypeValues mod) {
			if (data.effect == null) return;
			foreach (HitData.DamageType dmgType in dmgTypes) {
				var resist = GetResist(dmgType);
				var expose = GetExpose();
				mod.AddByType(dmgType, resist - expose);
			}
		}

		public void ModifyPower(ref DamageTypeValues mod) {
			if (data.effect == null) return;
			foreach (HitData.DamageType dmgType in dmgTypes) {
				var power = GetPower(dmgType);
				mod.AddByType(dmgType, power);
			}
		}

		public static void ApplyModifierToHitData(DamageTypeValues modifier, ref HitData hit, int multiplier) {
			ApplyModifierToFloat(modifier, HitData.DamageType.Blunt, ref hit.m_damage.m_blunt, multiplier);
			ApplyModifierToFloat(modifier, HitData.DamageType.Pierce, ref hit.m_damage.m_pierce, multiplier);
			ApplyModifierToFloat(modifier, HitData.DamageType.Slash, ref hit.m_damage.m_slash, multiplier);
			ApplyModifierToFloat(modifier, HitData.DamageType.Fire, ref hit.m_damage.m_fire, multiplier);
			ApplyModifierToFloat(modifier, HitData.DamageType.Frost, ref hit.m_damage.m_frost, multiplier);
			ApplyModifierToFloat(modifier, HitData.DamageType.Lightning, ref hit.m_damage.m_lightning, multiplier);
			ApplyModifierToFloat(modifier, HitData.DamageType.Poison, ref hit.m_damage.m_poison, multiplier);
			ApplyModifierToFloat(modifier, HitData.DamageType.Spirit, ref hit.m_damage.m_spirit, multiplier);
		}

		public static void ApplyModifierToFloat(DamageTypeValues modifier, HitData.DamageType dmgType, ref float damage, int multiplier) {
			var mod = modifier.GetByType(dmgType);
			if (mod != 0) {
				var diff = damage * (mod / 100f);
				damage += diff * multiplier;
			}
		}

		// ================================================================
		// APPLY
		// ================================================================

		public void ApplyEffectOnCharacter(Character target) {
			if (target == null) return;

			IDestructible destructable = null;
			Player player = null;

			try { destructable = target; } catch (Exception) {
			}

			try { player = (Player)target; } catch (Exception) {
			}

			// ===== RESTORING HEALTH ========================================

			float healHP = GetRecoverHealth() / 100f * target.GetMaxHealth();
			if (healHP != 0) target.Heal(healHP, true);

			// ===== RESTORING STAMINA =======================================

			float healST = GetRecoverStamina() / 100f * target.GetMaxStamina();
			if (healST != 0) player?.UseStamina(healST * -1);

			// ===== DEALING DAMAGE =================================

			var hitDamage = new HitData();
			hitDamage.SetAttacker(caster);

			if (data.effect.damage) {
				hitDamage.m_damage.m_blunt = GetDamage(HitData.DamageType.Blunt);
				hitDamage.m_damage.m_pierce = GetDamage(HitData.DamageType.Pierce);
				hitDamage.m_damage.m_slash = GetDamage(HitData.DamageType.Slash);
				hitDamage.m_damage.m_fire = GetDamage(HitData.DamageType.Fire);
				hitDamage.m_damage.m_frost = GetDamage(HitData.DamageType.Frost);
				hitDamage.m_damage.m_lightning = GetDamage(HitData.DamageType.Lightning);
				hitDamage.m_damage.m_poison = GetDamage(HitData.DamageType.Poison);
				hitDamage.m_damage.m_spirit = GetDamage(HitData.DamageType.Spirit);
				hitDamage.m_statusEffectHash = StringExtensionMethods.GetStableHashCode("runicDamage");
				target.Damage(hitDamage);
			}

			// ===== APPLYING ELEMENTAL EFFECTS =====================

			if (data.effect.burn) {
				var burning = ObjectDB.instance.m_StatusEffects.Find(x => x.name == "Burning").Clone() as SE_Burning;
				// no spirit damage, this is a simple fire burn
				burning.AddSpiritDamage(0);
				// each burn tick should apply 10% of the total value
				var baseValue = hitDamage.GetTotalDamage();
				var burnValue = baseValue / 10f;
				// getting the full duration (it increases with still)
				burning.m_ttl = 10f;
				burning.m_damageInterval = 1f;
				// calculating the total burn value and adding it
				var burnTotal = burnValue * burning.m_ttl;
				burning.AddFireDamage(burnTotal);
				target.m_seman.AddStatusEffect(burning);
			}

			if (data.effect.slow) {
				var frost = ObjectDB.instance.m_StatusEffects.Find(x => x.name == "Frost").Clone() as SE_Frost;
				frost.m_ttl = GetDuration();
				// no damage, just slow
				frost.m_freezeTimeEnemy = 0;
				frost.m_freezeTimePlayer = 0;
				frost.m_minSpeedFactor = GetSlowFactor();
				target.m_seman.AddStatusEffect(frost);
			}

			if (data.effect.poison) {
				var poison = ObjectDB.instance.m_StatusEffects.Find(x => x.name == "Poison").Clone() as SE_Poison;
				poison.m_ttl = GetDuration();
				poison.m_damageInterval = 1f;
				poison.m_damagePerHit = hitDamage.GetTotalDamage() / 10f;
				poison.m_damageLeft = poison.m_damageInterval * poison.m_damagePerHit;
				target.m_seman.AddStatusEffect(poison);
			}

			// ===== STAGGER ========================================

			if (data.effect.stagger == true) {
				var staggerDir = -caster.m_lookDir;
				target.Stagger(staggerDir);
			}

			// ===== PUSH BACK ======================================

			if (data.effect.pushback == true) {
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
				var fxString = GetEffectString();
				target.m_seman.AddStatusEffect(fxString, true);
			}
		}

		public void ApplyEffectOnSeman(SEMan seman) {
			ApplyEffectOnCharacter(seman?.m_character);
		}

		public void ApplyProjectile(Collider collider, Vector3 hitPoint) {
			if (data.effect == null) {
				return;
			}
			if (data.projectile == null) {
				return;
			}
			if (collider == null) {
				return;
			}

			GameObject obj = Projectile.FindHitObject(collider);
			var character = GetGameObjectCharacter(obj);
			if (character == caster) {
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
			var list = Character.GetAllCharacters().FindAll(other => caster.CanHarmWithRunes(other));
			return GetInRange(list, center, range);
		}

		public List<SEMan> GetAlliesAround(Vector3 center, float range) {
			var list = Character.GetAllCharacters().FindAll(other => caster.CanHelpWithRunes(other));
			return GetInRange(list, center, range);
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

		public bool RequiresWeapon() {
			return data.effect != null && data.effect.damage_mode == DamageMode.Weapon;
		}

		public bool RequiresShield() {
			return data.effect != null && data.effect.damage_mode == DamageMode.Shield;
		}

		public bool GotShield() {
			if (caster == null) return false;
			var shield = caster.GetLeftItem();
			return shield != null;
		}

		public bool GotWeapon() {
			if (caster == null) return false;
			var weapon = caster.GetRightItem();
			return weapon != null && weapon.IsWeapon();
		}

		public bool CustomEffect() {
			return data.fxcustom != default;
		}

		public bool CanCastCustom(bool showError = false) {
			var custom = data.fxcustom;
			var warpfix = data.warpfix;

			if (custom == "recall") {
				var blackforest = new List<string>() {
					"CopperOre",
					"Copper",
					"TinOre",
					"Tin",
					"Bronze",
				};

				var swamp = new List<string>() {
					"IronScrap",
					"Iron",
				};

				var mountains = new List<string>() {
					"SilverOre",
					"Silver",
				};

				var plains = new List<string>() {
					"BlackMetalScrap",
					"BlackMetal"
				};

				var forbidden = new List<string>();
				if (warpfix != "blackforest") forbidden.AddRange(blackforest);
				if (warpfix != "swamp") forbidden.AddRange(swamp);
				if (warpfix != "moutains") forbidden.AddRange(mountains);
				if (warpfix != "plains") forbidden.AddRange(plains);

				var forbid = false;
				foreach (var item in caster.GetInventory().m_inventory) forbid = forbid || forbidden.Contains(item.m_dropPrefab.name);

				if (forbid) {
					if (showError) RunicPower.ShowMessage(MsgKey.ITEM_PREVENTS_RECALL);
					return false;
				}
			}

			return true;
		}

		public void Cast() {
			// letting everyone knows the caster used the rune power
			UpdateCaster();
			RunicPower.AddCooldown(data.name, GetCooldown());

			var cfgMessage = RunicPower.configCastingMessage.Value;
			var message = data.name + "!";

			if (cfgMessage == RunicPower.CastingMessage.GLOBAL) Chat.instance.SendText(Talker.Type.Shout, message);
			if (cfgMessage == RunicPower.CastingMessage.NORMAL) Chat.instance.SendText(Talker.Type.Normal, message);
			if (cfgMessage == RunicPower.CastingMessage.SELF) Chat.instance.AddInworldText(caster.gameObject, caster.GetPlayerID(), caster.GetHeadPoint(), Talker.Type.Normal, UserInfo.GetLocalUser(), message);

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
			if (data.archetype != "Generic") {
				AddExperience();
				RunicPower.ClearCache();
			}

			// casting RECALL
			if (custom == "recall") {
				var prof = Game.instance.GetPlayerProfile();
				var location = prof.GetCustomSpawnPoint();
				if (location != null) caster.TeleportTo(location, caster.gameObject.transform.rotation, true);
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
				proj.Setup(attack.m_character, aimDir * attack.m_projectileVel, attack.m_attackHitNoise, hitData, null, null);
			} else {
				// getting the targets
				var semans = GetTargetsAroundCharacter(data.effect.target, caster, GetSkilledRange(data.rangeAOE));
				foreach (var seman in semans) ApplyEffectOnSeman(seman);
			}
		}

		public void AddExperience() {
			var value = data.rank;

			var min = GetMinLevel();
			var skill = GetSkillLevel(false);

			// if we're using a higher-level rune compared to our skill level (i.e using a rank2 rune when that skill level is below 20)
			if (min > skill) {
				// let's give a nice EXP boost
				value *= 5;
			}

			caster.RaiseSkill(data.skillType, value);

			var cskills = RunicPower.listofCSkills;
			var qty = cskills.Count - 1;
			var lower = value / qty;

			cskills.ForEach(cskill => {
				var ctype = cskill.GetSkillType();
				if (ctype != data.skillType) {
					caster.LowerSkill(ctype, lower);
				}
			});
		}
	}
}

