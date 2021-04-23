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

namespace RunicPower.Core {

	[Serializable]
	public class RunesConfig {
		public DefaultRecipeConfig defRecipes;
		public List<RuneData> runes = new List<RuneData>();
	}

	[Serializable]
	public class RuneData {
		public string name;
		public string core;

		public string type;
		public string archetype;
		public string description;
		public bool implemented;

		public int rank;

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


		public string fxcustom;
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

		public RuneVFX SetName(string name) {
			this.name = name;
			return this;
		}
	}
}
