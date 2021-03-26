using Common;
using RunicPower.Patches;
using RunicPower;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.ParticleSystem;
using RuneStones.Patches;

namespace RunicPower.Core {

	[Serializable]
	public class RunesConfig {
		public DefaultRecipeConfig defRecipes;
		public List<Rune> runes = new List<Rune>();
	}

	[Serializable]
	public class Rune {
		public string name;
		public string type;
		public string archetype;
		public string description;
		public bool implemented;
		public RecipeConfig recipe;

		public RuneVFX vfx;
		public RuneVFX sfx;

		public string fxcustom;
		public RuneEffect effect;
		public RuneProjectile projectile;

		public static int retrycount = 5;

		public GameObject prefab;
		public ItemDrop itemDrop;
		public StatusEffect statusEffect;

		private Player player;
		private float playerWeaponDmg = 0;
		private DamageTypeValues playerPowerMods = new DamageTypeValues();

		private float rangeExplosion = 5f;
		private float rangeAOE = 20f;
		private float rangeAOEallies = 20f;
		private float rangeAOEfoes = 10f;

		private Skills.SkillType skillType;

		public List<HitData.DamageModPair> resistanceModifiers;

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

				
		public void CreateEffect() {
			if (effect == null || effect.duration == 0) return;
			statusEffect = ScriptableObject.CreateInstance<StatusEffect>();
			statusEffect.m_ttl = GetDuration();
			statusEffect.name = name;
			statusEffect.m_name = name;
			statusEffect.m_category = name;
			statusEffect.m_cooldown = 0f;
			statusEffect.m_icon = itemDrop.m_itemData.m_shared.m_icons[0];
			statusEffect.SetRune(this);
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
			SetPlayer(Player.m_localPlayer);

			var colorClass = "white";
			if (archetype == "Warrior") colorClass = "#C69B6D";
			if (archetype == "Cleric") colorClass = "#F48CBA";
			if (archetype == "Rogue") colorClass = "#FFF468";
			if (archetype == "Wizard") colorClass = "#3FC7EB";

			text.AppendFormat("<color={0}>[{1} {2}]</color> {3}", colorClass, archetype, type, item.m_shared.m_description);
			text.AppendFormat("\n");

			var fx = effect;

			if (fx != null) {
				text.Append("\n-----------------------");

				// REGEN
				if (fx.healthRegen != 0) text.AppendFormat("\nHealth regen <color=orange>+{0}%</color>", fx.healthRegen * 100);
				if (fx.staminaRegen != 0) text.AppendFormat("\nStamina regen <color=orange>+{0}%</color>", fx.staminaRegen * 100);

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

				var elTypes = new List<HitData.DamageType>();
				elTypes.Add(HitData.DamageType.Fire);
				elTypes.Add(HitData.DamageType.Frost);
				elTypes.Add(HitData.DamageType.Lightning);
				elTypes.Add(HitData.DamageType.Poison);
				elTypes.Add(HitData.DamageType.Spirit);

				var phTypes = new List<HitData.DamageType>();
				phTypes.Add(HitData.DamageType.Slash);
				phTypes.Add(HitData.DamageType.Pierce);
				phTypes.Add(HitData.DamageType.Blunt);

				if (fx.doPower.IsValued()) {
					if (fx.doPower.IsElemental()) TooltipAppendPower(ref text, HitData.DamageType.Fire, "Elemental"); else foreach (var dmgType in elTypes) TooltipAppendPower(ref text, dmgType);
					if (fx.doPower.IsPhysical()) TooltipAppendPower(ref text, HitData.DamageType.Slash, "Physical"); else foreach (var dmgType in phTypes) TooltipAppendPower(ref text, dmgType);
				}

				if (fx.doResist.IsValued()) {
					if (fx.doResist.IsElemental()) TooltipAppendResist(ref text, HitData.DamageType.Fire, "Elemental"); else foreach (var dmgType in elTypes) TooltipAppendResist(ref text, dmgType);
					if (fx.doResist.IsPhysical()) TooltipAppendResist(ref text, HitData.DamageType.Slash, "Physical"); else foreach (var dmgType in phTypes) TooltipAppendResist(ref text, dmgType);
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

				text.Append("\n-----------------------");

				text.AppendFormat("\n");
				var duration = GetDuration();
				var texttime = (duration == 0) ? "Instant" : duration + "s";				
				text.AppendFormat("\nDuration: <color=orange>{0}</color>", texttime);

				if (fx.target != "") {
					var dstarget = mapTarget[fx.target];
					if (fx.target == "projectile") {
						if (projectile.explode) {
							text.AppendFormat("\nTarget: <color=orange>{0} (Explosive)</color> ({1} meters)", dstarget, GetSkilledRange(rangeExplosion));
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

			return text.ToString();
		}

		// ================================================================
		// SETTERS
		// ================================================================

		public void SetPlayer(Player player) {
			this.player = player;
			var dmg = player.GetCurrentWeapon().GetDamage();
			playerWeaponDmg = dmg.GetTotalElementalDamage() + dmg.GetTotalPhysicalDamage();

			var runes = player.GetRunes();
			playerPowerMods = new DamageTypeValues();
			foreach (var rune in runes) rune.AppendPower(ref playerPowerMods);

			skillType = (Skills.SkillType)ClassSkill.GetIdByName(archetype);
		}

		// ================================================================
		// GETTERS
		// ================================================================

		public float GetSkill() {
			float skill = player.GetSkillFactor(skillType) * 100f;
			if (skill < 1) skill = 1;
			return skill;
		}

		public int GetHealingHP() {
			var heal = (float)effect?.healthRecover;
			var value = (heal / 100) * playerWeaponDmg;
			return (int)value;
		}

		public int GetHealingStamina() {
			var heal = (float)effect?.staminaRecover;
			var value = (heal / 100) * playerWeaponDmg;
			return (int)value;
		}

		public int GetSkilledTypedValue(DamageTypeValues source, HitData.DamageType dmgType, float skillMultiplier, float weaponMultiplier, float? capValue = null) {
			float skill = GetSkill();
			var value = source.GetByType(dmgType) / 100f;
			if (value == 0) return 0;
			// each skill level increases damage by x
			var skilled = value * skillMultiplier * skill;
			// each skill level increases damage by +x% of weapon damage
			var weapon = playerWeaponDmg * skill * weaponMultiplier / 100f;
			// getting the total base value
			var total = skilled + weapon;
			// applying power modifiers
			var modifier = playerPowerMods.GetByType(dmgType);
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

		public int GetDuration() {
			int value = effect.duration;
			int skill = (int) GetSkill() - 1;
			var multi = (100f + skill*2) / 100f;
			return Mathf.RoundToInt(value * multi);
		}

		public int GetSkilledRange(float value) {
			int skill = (int)GetSkill() - 1;
			var multi = (100f + skill) / 100f;
			return Mathf.RoundToInt(value * multi);
		}

		public int GetSkilledRangeAOE(string type) {
			var range = (type == "allies") ? rangeAOEallies : rangeAOEfoes;
			return GetSkilledRange(range);
		}

		private float GetDamage(HitData.DamageType dmgType) {
			// level 1: 3f + 5% of weapon
			// level 2: 6f + 10% of weapon
			// level 10: 30f + 50% of weapon
			// level 50: 150f + 250% of weapon
			// level 100: 300f + 500% of weapon
			return GetSkilledTypedValue(effect.doDamage, dmgType, 3f, 2f);
		}

		private float GetHealHP(HitData.DamageType dmgType) {
			// level 1: 7f + 2% of weapon
			// level 2: 14f + 4% of weapon
			// level 10: 70f + 20% of weapon
			// level 50: 350f + 100% of weapon
			// level 100: 700f + 200% of weapon
			return GetSkilledTypedValue(effect.doHealHP, dmgType, 7f, 1f);
		}

		private float GetHealST(HitData.DamageType dmgType) {
			// level 1: 7f + 2% of weapon
			// level 2: 14f + 4% of weapon
			// level 10: 70f + 20% of weapon
			// level 50: 350f + 100% of weapon
			// level 100: 700f + 200% of weapon
			return GetSkilledTypedValue(effect.doHealST, dmgType, 7f, 1f);
		}

		private float GetPowerElemental() {
			// level 1: +2%
			// level 2: +4%
			// level 10: +20%
			// level 50: +100%
			// level 100: +200%
			return 0f;
		}

		private float GetPowerPhysical() {
			// level 1: +2%
			// level 2: +4%
			// level 10: +20%
			// level 50: +100%
			// level 100: +200%
			return 0f;
		}

		private int GetPower(HitData.DamageType dmgType) {
			// level 1: +2%
			// level 2: +4%
			// level 10: +20%
			// level 50: +100%
			// level 100: +200%
			return GetSkilledTypedValue(effect.doPower, dmgType, 2f, 0f);
		}

		private int GetResist(HitData.DamageType dmgType) {
			// level 1: +2%
			// level 2: +4%
			// level 10: +20%
			// level 50: +100%
			return GetSkilledTypedValue(effect.doResist, dmgType, 2f, 0f, 100f);
		}

		private float GetMovementBonus() {
			// level 1: +1%
			// level 2: +2%
			// level 10: +10%
			// level 50: +50%
			return GetSkilledValue((float) effect.movementBonus / 100f, 1f, 50f);
		}

		public float GetHealthSteal() {
			// level 1: +1%
			// level 2: +2%
			// level 10: +10%
			// level 50: +50%
			// level 100: +100%
			return GetSkilledValue((float) effect.healthBack / 100f, 1f);
		}

		public float GetExpose() {
			return GetSkilledValue((float)effect.expose / 100f, 2f);
		}

		public float GetStealhiness() {
			return GetSkilledValue((float)effect.stealthiness / 100f, 1f, 100f);
		}
				

		public float GetTickDamage(HitData.DamageType dmgType) {
			var value = GetDamage(dmgType);
			return value / 10f;
		}

		public List<HitData.DamageModPair> GetResistanceModifiers() {
			if (resistanceModifiers == null) {
				resistanceModifiers = new List<HitData.DamageModPair>();
				GetResistanceModifier(HitData.DamageType.Blunt, effect?.physicalResitance);
				GetResistanceModifier(HitData.DamageType.Pierce, effect?.physicalResitance);
				GetResistanceModifier(HitData.DamageType.Slash, effect?.physicalResitance);
				GetResistanceModifier(HitData.DamageType.Fire, effect?.elementalResistance);
				GetResistanceModifier(HitData.DamageType.Frost, effect?.elementalResistance);
				GetResistanceModifier(HitData.DamageType.Lightning, effect?.elementalResistance);
				GetResistanceModifier(HitData.DamageType.Poison, effect?.elementalResistance);
				GetResistanceModifier(HitData.DamageType.Spirit, effect?.elementalResistance);
			}
			return resistanceModifiers;
		}

		// ================================================================
		// MODIFY
		// ================================================================

		public void ModifyStaminaRegen(Player player, ref float staminaMultiplier) {
			if (effect == null) return;
			if (effect.staminaRegen != 0) staminaMultiplier += effect.staminaRegen + 1;
		}

		public void ModifyEquipmentMovement(Player player, ref float equipmentMovement) {
			if (effect == null) return;
			equipmentMovement += GetMovementBonus() / 100f;
		}

		public void AppendPower(ref DamageTypeValues power) {
			if (effect == null) return;
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
			resistanceModifiers.Add(new HitData.DamageModPair() { m_type = type, m_modifier = mapDamageModifier[value] });
		}

		// ================================================================
		// APPLY
		// ================================================================

		public void ApplyEffectOnCharacter(Character target) {
			if (target == null) return;

			Debug.Log("==================================");
			Debug.Log("ApplyEffectOn " + target);
			var weaponDmg = player.GetCurrentWeapon().GetDamage().GetTotalDamage();
			var damageModifiers = player.GetDamageModifiers();

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
				target.AddStamina(healST);
			}
			
			// ===== APPLYING ELEMENTAL EFFECTS =====================

			if (effect.burn) {
				Debug.Log("RUNE IS APPLYING BURN");
				var burning = ObjectDB.instance.m_StatusEffects.Find(x => x.name == "Burning").Clone() as SE_Burning;
				burning.m_ttl = GetDuration();
				burning.m_damageInterval = 1f;
				// no spirit damage, this is a simple fire burn
				burning.m_damage.m_spirit = 0;
				burning.m_damage.m_fire = GetTickDamage(HitData.DamageType.Fire);
				target.m_seman.AddStatusEffect(burning);
			}

			if (effect.slow) {
				Debug.Log("RUNE IS APPLYING SLOW");
				var frost = ObjectDB.instance.m_StatusEffects.Find(x => x.name == "Frost").Clone() as SE_Frost;
				frost.m_ttl = GetDuration();
				// no damage, just slow
				frost.m_freezeTimeEnemy = 0;
				frost.m_freezeTimePlayer = 0;
				target.m_seman.AddStatusEffect(frost);
			}

			if (effect.poison) {
				Debug.Log("RUNE IS APPLYING POISON");
				var poison = ObjectDB.instance.m_StatusEffects.Find(x => x.name == "Poison").Clone() as SE_Poison;
				poison.m_ttl = GetDuration();
				poison.m_damageInterval = 1f;
				poison.m_damagePerHit = GetTickDamage(HitData.DamageType.Poison);
				poison.m_damageLeft = poison.m_damageInterval * poison.m_damagePerHit;
				target.m_seman.AddStatusEffect(poison);
			}

			// ===== DEALING DAMAGE =================================

			if (effect.DoDamage()) {
				Debug.Log("RUNE IS DOING DAMAGE");

				var hitDamage = new HitData();				
				hitDamage.m_damage.m_blunt = GetDamage(HitData.DamageType.Blunt);
				hitDamage.m_damage.m_pierce = GetDamage(HitData.DamageType.Pierce);
				hitDamage.m_damage.m_slash = GetDamage(HitData.DamageType.Slash);
				hitDamage.m_damage.m_fire = GetDamage(HitData.DamageType.Fire);
				hitDamage.m_damage.m_frost = GetDamage(HitData.DamageType.Frost);
				hitDamage.m_damage.m_lightning = GetDamage(HitData.DamageType.Lightning);
				hitDamage.m_damage.m_poison = GetDamage(HitData.DamageType.Poison);
				hitDamage.m_damage.m_spirit = GetDamage(HitData.DamageType.Spirit);
				target.ApplyDamage(hitDamage, true, false);
			}

			// ===== STAGGER ========================================

			if (effect.stagger == true) {
				Debug.Log("RUNE IS STAGGERING");
				var staggerDir = -player.m_lookDir;
				// target.SetRunicStagger(true, staggerDir);
				target.Stagger(staggerDir);
			}

			// ===== PUSH BACK ======================================

			if (effect.pushback == true) {
				Debug.Log("RUNE IS PUSHING BACK");
				var hitPushback = new HitData();
				hitPushback.m_pushForce = 500f;
				var from = player.gameObject.transform.position;
				var to = target.gameObject.transform.position;
				hitPushback.m_dir = (to - from).normalized;
				target.ApplyPushback(hitPushback);
			}

			// ===== ADDING AS EFFFECT ======================================

			// if there is a duration, it means it'a buff. So let's apply it to targets
			if (effect.duration > 0 && statusEffect) {
				Debug.Log("RUNE IS ADDING SOME (DE)BUFF TO " + target.name);
				target.m_seman.AddStatusEffect(statusEffect, true);
			}
		}

		public void ApplyEffectOnSeman(SEMan seman) {
			ApplyEffectOnCharacter(seman?.m_character);
		}

		public void ApplyProjectile(Collider collider, Vector3 hitPoint) {
			if (effect == null) return;
			if (projectile == null) return;
			if (collider == null) return;


			GameObject obj = Projectile.FindHitObject(collider);
			var character = GetGameObjectCharacter(obj);
			if (character == player) return;

			var semans = new List<SEMan>();

			if (projectile.explode) {
				semans = GetFoesAround(hitPoint, GetSkilledRange(rangeExplosion));
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
				if (character.IsMonsterFaction() && !character.IsTamed()) characters.Add(character);
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
				var vfxGo = UnityEngine.Object.Instantiate(vfxPrefab, player.gameObject.transform.position, vfxPrefab.transform.rotation);
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

		public void Cast(Humanoid caster) {
			Debug.Log("============================================");
			// letting everyone knows the caster used the rune power
			Chat.instance.SendText(Talker.Type.Shout, name + "!");

			// getting the caster-player
			player = caster as Player;

			SetPlayer(player);
			CreateEffect();

			// get the effects
			var custom = fxcustom;
			ExecFX(vfx);
			ExecFX(sfx);
			
			// casting RECALL

			// getting the archetype skill Id and adding experience to it
			try {
				player.RaiseSkill(skillType, 1f);
			} catch(Exception e) {
				Debug.LogError("failed to raise skill");
				Debug.LogError(e.ToString());
			}
			// casting RECALL
			if (custom == "recall") {
				var prof = Game.instance.GetPlayerProfile();
				var location = prof.GetCustomSpawnPoint();
				if (location != null) player.TeleportTo(location, player.gameObject.transform.rotation, true);
				return;
			}

			if (effect == null) {
				return;
			}

			if (effect.target == "projectile") {
				var attack = new Attack();
				attack.m_character = caster;
				attack.m_attackHeight = 2f;
				attack.m_attackRange = 1f;
				attack.m_attackHitNoise = 0;
				attack.m_projectileVel = projectile.speed;
				attack.GetProjectileSpawnPoint(out var spawnPoint, out var aimDir);
				aimDir = GameCamera.instance.transform.forward;
				var hitData = new HitData();
				var projPrefab = ZNetScene.instance.GetPrefab(projectile.name);
				GameObject projGo = UnityEngine.Object.Instantiate(projPrefab, spawnPoint, Quaternion.LookRotation(aimDir));
				var proj = projGo.GetComponent<IProjectile>() as Projectile;
				proj.m_gravity = 0f;
				proj.m_ttl = (float) projectile.duration;
				proj.SetRune(this);
				proj.Setup(attack.m_character, aimDir * attack.m_projectileVel, attack.m_attackHitNoise, hitData, null);
			} else {
				// getting the targets
				var range = GetSkilledRangeAOE(effect.target);
				var semans = GetTargetsAroundCharacter(effect.target, player, GetSkilledRange(rangeAOE));
				foreach (var seman in semans) ApplyEffectOnSeman(seman);
			}
		}
	}

	[Serializable]
	public class DamageTypeValues {
		public int m_blunt;
		public int m_pierce;
		public int m_slash;

		public int m_fire;
		public int m_frost;
		public int m_lightning;
		public int m_poison;
		public int m_spirit;

		public float Elemental() {
			return m_fire + m_frost + m_lightning + m_poison + m_spirit;
		}

		public bool IsElemental() {
			return m_fire != 0 && m_fire == m_frost && m_lightning == m_poison && m_poison == m_spirit && m_spirit == m_fire;
		}

		public bool IsValued() {
			return IsPhysical() || IsElemental();
		}

		public float Physical() {
			return m_blunt + m_pierce + m_slash;
		}

		public bool IsPhysical() {
			return m_blunt != 0 && m_blunt == m_pierce && m_pierce == m_slash;
		}

		public float Total() {
			return Elemental() + Physical();
		}

		public DamageTypeValues Reset() {
			m_blunt = 0;
			m_pierce = 0;
			m_slash = 0;
			m_fire = 0;
			m_frost = 0;
			m_lightning = 0;
			m_poison = 0;
			m_spirit = 0;
			return this;
		}

		public void AddByType(HitData.DamageType dmgType, int value) {
			SetByType(dmgType, GetByType(dmgType) + value);
		}

		public void SetByType(HitData.DamageType dmgType, int value) {
			if (dmgType is HitData.DamageType.Blunt) m_blunt = value;
			if (dmgType is HitData.DamageType.Pierce) m_pierce = value;
			if (dmgType is HitData.DamageType.Slash) m_slash = value;
			if (dmgType is HitData.DamageType.Fire) m_fire = value;
			if (dmgType is HitData.DamageType.Frost) m_frost = value;
			if (dmgType is HitData.DamageType.Lightning) m_lightning = value;
			if (dmgType is HitData.DamageType.Poison) m_poison = value;
			if (dmgType is HitData.DamageType.Spirit) m_spirit = value;
		}

		public int GetByType(HitData.DamageType dmgType) {
			if (dmgType is HitData.DamageType.Blunt) return m_blunt;
			if (dmgType is HitData.DamageType.Pierce) return m_pierce;
			if (dmgType is HitData.DamageType.Slash) return m_slash;
			if (dmgType is HitData.DamageType.Fire) return m_fire;
			if (dmgType is HitData.DamageType.Frost) return m_frost;
			if (dmgType is HitData.DamageType.Lightning) return m_lightning;
			if (dmgType is HitData.DamageType.Poison) return m_poison;
			if (dmgType is HitData.DamageType.Spirit) return m_spirit;
			return 0;
		}

		override public string ToString() {
			return "DamageTypeValues(m_blunt=" + m_blunt + ", m_pierce=" + m_pierce + ", m_slash=" + m_slash + ", m_fire=" + m_fire + ", m_frost=" + m_frost + ", m_lightning=" + m_lightning + ", m_poison=" + m_poison + ", m_spirit=" + m_spirit + ")";
		}
	}

	[Serializable]
	public class RuneEffect {
		public int duration;
		public string target;
		
		public int healthRegen;
		public int staminaRegen;

		public decimal healthBack;
		public decimal staminaBack;

		public int healthRecover;
		public int staminaRecover;

		public string physicalResitance;
		public string elementalResistance;

		public DamageTypeValues doDamage = new DamageTypeValues();
		public DamageTypeValues doResist = new DamageTypeValues();
		public DamageTypeValues doPower = new DamageTypeValues();
		public DamageTypeValues doHealHP = new DamageTypeValues();
		public DamageTypeValues doHealST = new DamageTypeValues();

		public decimal stealthiness;

		public bool stagger;
		public bool pushback;
		public bool pull;
		public bool fear;
		public bool burn;
		public bool slow;
		public bool cripple;
		public bool poison;

		public decimal expose;
		public decimal movementBonus;
		public bool ignoreFallDamage;

		public bool DoDamage() {
			return doDamage.Total() != 0;
		}

		public bool DoHeal() {
			return (doHealST.Total() + doHealHP.Total()) != 0;
		}
	}

	[Serializable]
	public class RuneProjectile {
		public string name;
		public int speed;
		public decimal duration;
		public bool explode;
	}


	public class RuneVFX {
		public string name;
		public List<string> list = new List<string>();
	}
}
