using RunicPower.Core;

namespace RunicPower.Patches {
	public static class SEMan_Prototype {

		static StatusEffect se;
		static Rune seRune;

		public static void SetTempRune(StatusEffect statusEffect, Rune rune) {
			se = statusEffect;
			seRune = rune;
		}

		public static void UnsetTemp() {
			SetTempRune(null, null);
		}

		public static Rune GetTempRune(StatusEffect statusEffect) {
			return (se?.GetInstanceID() == statusEffect.GetInstanceID()) ? seRune : null;
		}

		public static void AddRunicEffect(this SEMan __instance, string name, Player caster, string dsbuffs, bool resetTime) {
			// getting the rune data of this runic effect
			var data = RunicPower.GetRuneData(name);
			// if there is none, stop here
			if (data == null) return;
			// checking if the target already has this effect
			var currentEffect = __instance.m_statusEffects.Find(se => se.GetRune()?.data.core == data.core);
			// if it does
			if (currentEffect != null) {
				var currentRune = currentEffect.GetRune();
				// and its rank is greater than the one we're trying to apply
				if (currentRune.data.rank > data.rank) {
					// stop here. we won't replace the buff with a worse version of it!
					return;
				} else {
					// if the current rune is of equal or lower rank, let's remove it so it can be reapplied
					__instance.RemoveStatusEffect(currentEffect, true);
				}
			}
			// preparing a new effect
			var rune = RunicPower.CreateRunicEffect(name, caster, dsbuffs);
			var effect = rune?.statusEffect;

			// and adding it to the player
			if (effect != null) {
				SetTempRune(effect, rune);
				__instance.AddStatusEffect(effect);
				UnsetTemp();
			}
		}
	}
}
