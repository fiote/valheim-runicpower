using Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RunicPower.Core {

	[Serializable]
	public class RunesConfig {
		public DefaultRecipeConfig defRecipes;
		public RuneSets reSets;
		public List<RuneData> runes = new List<RuneData>();
	}

	public class RuneSets {
		public List<string> s1, s2, s3, s4;
	}

	[Serializable]
	public class RuneData {
		public string name;
		public string core;

		public string type;
		public string archetype;
		public string description;
		public bool implemented;

		public bool ranked = true;
		public int maxstack = default;
		public int rank;

		public string reSet;
		public List<string> resources = new List<string>();
		public RecipeConfig recipe;

		public int cooldown;

		public RuneVFX vfx;
		public RuneVFX sfx;

		public static RuneVFX genericVFX = new RuneVFX().SetName("vfx_Potion_stamina_medium");
		public static RuneVFX genericSFX = new RuneVFX().SetName("sfx_bowl_AddItem");

		public static List<HitData.DamageType> elTypes = new List<HitData.DamageType> {
			HitData.DamageType.Fire,
			HitData.DamageType.Frost,
			HitData.DamageType.Lightning,
			HitData.DamageType.Poison,
			HitData.DamageType.Spirit
		};

		public static List<HitData.DamageType> phTypes = new List<HitData.DamageType> {
			HitData.DamageType.Slash,
			HitData.DamageType.Pierce,
			HitData.DamageType.Blunt
		};

		public static List<HitData.DamageType> otTypes = new List<HitData.DamageType> {
			HitData.DamageType.Chop,
			HitData.DamageType.Pickaxe
		};

		public string fxcustom;
		public string warpfix;

		public RuneEffect effect;
		public RuneProjectile projectile;

		public static int retrycount = 5;

		public GameObject prefab;
		public ItemDrop itemDrop;

		public Player caster;
		public float casterWeaponDmg = 0;
		public DamageTypeValues casterPowerMods = new DamageTypeValues();

		public float rangeExplosion = 5f;
		public float rangeAOE = 20f;
		public float rangeAOEallies = 20f;
		public float rangeAOEfoes = 10f;

		public Skills.SkillType skillType;

		public List<HitData.DamageModPair> resistanceModifiers;

		public void Config() {
			skillType = (Skills.SkillType)ClassSkill.GetIdByName(archetype);
		}
	}

	public class DamageTypeFlags {
		public bool blunt;
		public bool pierce;
		public bool slash;

		public bool fire;
		public bool frost;
		public bool lightning;
		public bool poison;
		public bool spirit;

		public bool chop;
		public bool pickaxe;

		public bool GetByType(HitData.DamageType dmgType) {
			if (dmgType is HitData.DamageType.Blunt) return blunt;
			if (dmgType is HitData.DamageType.Pierce) return pierce;
			if (dmgType is HitData.DamageType.Slash) return slash;
			if (dmgType is HitData.DamageType.Fire) return fire;
			if (dmgType is HitData.DamageType.Frost) return frost;
			if (dmgType is HitData.DamageType.Lightning) return lightning;
			if (dmgType is HitData.DamageType.Poison) return poison;
			if (dmgType is HitData.DamageType.Spirit) return spirit;
			if (dmgType is HitData.DamageType.Chop) return chop;
			if (dmgType is HitData.DamageType.Pickaxe) return pickaxe;
			return false;
		}

		public bool IsValued() {
			return fire || frost || lightning || poison || spirit || blunt || pierce || slash || chop || pickaxe;
		}

		public bool IsElemental() {
			return fire && frost && lightning && poison && spirit;
		}

		public bool IsPhysical() {
			return blunt && pierce && slash;
		}

		override public string ToString() {
			var parts = new List<string>();
			parts.Add($"blunt={blunt}");
			parts.Add($"pierce={pierce}");
			parts.Add($"slash={slash}");
			parts.Add($"fire={fire}");
			parts.Add($"lightning={lightning}");
			parts.Add($"poison={poison}");
			parts.Add($"spirit={spirit}");
			parts.Add($"chop={chop}");
			parts.Add($"pickaxe={pickaxe}");

			var content = String.Join(", ", parts);
			if (content == "") content = "empty";

			return $"DamageTypeFlags({content})";
		}
	}

	[Serializable]
	public class DamageTypeValues {
		public float m_blunt;
		public float m_pierce;
		public float m_slash;

		public float m_fire;
		public float m_frost;
		public float m_lightning;
		public float m_poison;
		public float m_spirit;

		public float m_chop;
		public float m_pickaxe;

		public bool IsValued() {
			return IsPhysical() || IsElemental();
		}

		public bool IsElemental() {
			return m_fire != 0 && m_fire == m_frost && m_lightning == m_poison && m_poison == m_spirit && m_spirit == m_fire;
		}

		public bool IsPhysical() {
			return m_blunt != 0 && m_blunt == m_pierce && m_pierce == m_slash;
		}

		public float Elemental() {
			return m_fire + m_frost + m_lightning + m_poison + m_spirit;
		}

		public float Physical() {
			return m_blunt + m_pierce + m_slash;
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
			m_chop = 0;
			m_pickaxe = 0;
			return this;
		}

		public void AddByType(HitData.DamageType dmgType, float value) {
			SetByType(dmgType, GetByType(dmgType) + value);
		}

		public void SetByType(HitData.DamageType dmgType, float value) {
			if (dmgType is HitData.DamageType.Blunt) m_blunt = value;
			if (dmgType is HitData.DamageType.Pierce) m_pierce = value;
			if (dmgType is HitData.DamageType.Slash) m_slash = value;
			if (dmgType is HitData.DamageType.Fire) m_fire = value;
			if (dmgType is HitData.DamageType.Frost) m_frost = value;
			if (dmgType is HitData.DamageType.Lightning) m_lightning = value;
			if (dmgType is HitData.DamageType.Poison) m_poison = value;
			if (dmgType is HitData.DamageType.Spirit) m_spirit = value;
			if (dmgType is HitData.DamageType.Chop) m_chop = value;
			if (dmgType is HitData.DamageType.Pickaxe) m_pickaxe = value;
		}

		public float GetByType(HitData.DamageType dmgType) {
			if (dmgType is HitData.DamageType.Blunt) return m_blunt;
			if (dmgType is HitData.DamageType.Pierce) return m_pierce;
			if (dmgType is HitData.DamageType.Slash) return m_slash;
			if (dmgType is HitData.DamageType.Fire) return m_fire;
			if (dmgType is HitData.DamageType.Frost) return m_frost;
			if (dmgType is HitData.DamageType.Lightning) return m_lightning;
			if (dmgType is HitData.DamageType.Poison) return m_poison;
			if (dmgType is HitData.DamageType.Spirit) return m_spirit;
			if (dmgType is HitData.DamageType.Chop) return m_chop;
			if (dmgType is HitData.DamageType.Pickaxe) return m_pickaxe;
			return 0;
		}

		override public string ToString() {
			var parts = new List<string>();
			if (m_blunt != 0) parts.Add($"m_blunt={m_blunt}");
			if (m_pierce != 0) parts.Add($"m_pierce={m_pierce}");
			if (m_slash != 0) parts.Add($"m_slash={m_slash}");
			if (m_fire != 0) parts.Add($"m_fire={m_fire}");
			if (m_lightning != 0) parts.Add($"m_lightning={m_lightning}");
			if (m_poison != 0) parts.Add($"m_poison={m_poison}");
			if (m_spirit != 0) parts.Add($"m_spirit={m_spirit}");
			if (m_chop != 0) parts.Add($"m_chop={m_chop}");
			if (m_pickaxe != 0) parts.Add($"m_pickaxe={m_pickaxe}");

			var content = String.Join(", ", parts);
			if (content == "") content = "empty";

			return $"DamageTypeValues({content})";
		}
	}

	[Serializable]
	public class RuneEffect {
		public int duration;
		public string target; 

		public bool healthRegen;
		public bool staminaRegen;

		public bool healthBack;
		public decimal staminaBack;

		public bool healthRecover;
		public bool staminaRecover;

		public string physicalResitance;
		public string elementalResistance;

		public bool damage;
		public DamageMode damage_mode;
		public HitData.DamageType damage_type;

		// public DamageTypeFlags damage = new DamageTypeFlags();
		public DamageTypeFlags resist = new DamageTypeFlags();
		public DamageTypeFlags power = new DamageTypeFlags();

		public bool stealthiness;

		public decimal v1, vx, v100, value;

		public bool stagger;
		public bool pushback;
		public bool pull;
		public bool fear;
		public bool burn;
		public bool slow;
		public bool cripple;
		public bool poison;

		public bool expose;
		public bool movementBonus;
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

		public RuneVFX SetName(string name) {
			this.name = name;
			return this;
		}
	}
}

public enum DamageMode {
	Weapon = 1,
	Shield = 2,
	Skill = 4
}