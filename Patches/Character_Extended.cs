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
	public class Character_Extended : MonoBehaviour {
		public Character character;

		public List<Rune> runes = new List<Rune>();

		public float runicMoveBonus = 0;
		public float runicInvisibilityRange = 0;
		public float runicLifeSteal = 0;
		public bool runicIgnoreFallDamage = false;
		public DamageTypeValues runicResistModifier = new DamageTypeValues();
		public DamageTypeValues runicPowerModifier = new DamageTypeValues();

		public void SetCharacter(Character character) {
			this.character = character;
		}

		private void UpdateValues() {
			StoreRuneEffects();
			if (character?.IsPlayer() == true) RunicPower.ClearCache();
		}

		void StoreRuneEffects() {
			Log("StoreRuneEffects");
			runicMoveBonus = 0f;
			runicInvisibilityRange = 0f;
			runicLifeSteal = 0f;
			runicIgnoreFallDamage = false;
			runicResistModifier.Reset();
			runicPowerModifier.Reset();

			runes.ForEach(rune => {
				rune.ModifyEquipmentMovement(ref runicMoveBonus);
				rune.ModifyInvisibilityRange(ref runicInvisibilityRange);
				rune.ModifyHealthSteal(ref runicLifeSteal);
				rune.ModifyIgnoreFallDamage(ref runicIgnoreFallDamage);
				rune.ModifyResist(ref runicResistModifier);
				rune.ModifyPower(ref runicPowerModifier);
			});
		}

		public void ApplyResistModifiersToHit(ref HitData hit) {
			Rune.ApplyModifierToHit(runicResistModifier, ref hit);
		}

		public void ApplyPowerModifiersToHit(ref HitData hit) {
			Rune.ApplyModifierToHit(runicPowerModifier, ref hit);
		}

		public void AddRune(Rune rune) {
			if (rune == null) return;
			Log("AddRune " + rune?.data.name);
			runes.Add(rune);
			UpdateValues();
		}

		public void RemoveRune(Rune rune) {
			if (rune == null) return;
			Log("RemoveRune " + rune?.data.name);
			if (runes.Contains(rune)) {
				Log("rune removed.");
				runes.Remove(rune);
				UpdateValues();
			} else {
				Log("cant remove rune, its not on the stack!");
			}
		}

		void Log(string message) {
			RunicPower.Debug(this.character.name + " " + message);
		}
	}
}