using RunicPower.Core;
using System;

namespace RunicPower.Patches {

	public static class Player_Prototype {
		public static Player_Extended ExtendedPlayer(this Player __instance, Boolean create) {
			var ext = __instance.gameObject.GetComponent<Player_Extended>();
			if (ext == null && create) {
				ext = __instance.gameObject.AddComponent<Player_Extended>();
				ext.SetPlayer(__instance);
			}
			return ext;
		}

		public static Inventory GetSpellsBarInventory(this Player __instance) {
			return __instance.ExtendedPlayer(true)?.spellsBarInventory;
		}

		public static ItemDrop.ItemData GetSpellsBarItem(this Player __instance, int index) {
			if (index < 0 || index > SpellsBar.slotCount) return null;
			return __instance.GetSpellsBarInventory()?.GetItemAt(index, 0);
		}

		public static void UseRuneItem(this Player __instance, ItemDrop.ItemData item, bool fromInventoryGui) {
			if (item == null) return;
			var inv = __instance.GetSpellsBarInventory();
			__instance.UseItem(inv, item, fromInventoryGui);
		}

		public static bool CanHarmWithRunes(this Player __instance, Character other) {
			// if the other is a monster of a boss, it CAN be harmed
			if (other.IsMonsterFaction() || other.IsBoss()) return true;
			// if the player is not flagged as pvp, it CAN NOT harm others players
			if (!__instance.IsPVPEnabled()) return false;
			// if the pvp config is not enabled, it CAN NOT harm others players
			if (!RunicPower.configPvpEnabled.Value) return false;
			// then the other player can only be harmed if its pvp is enabled
			return other.IsPVPEnabled();
		}

		public static bool CanHelpWithRunes(this Player __instance, Character other) {
			return !__instance.CanHarmWithRunes(other);
		}
		public static void LowerSkill(this Player __instance, Skills.SkillType skill, float value = 1f) {
			__instance.m_skills.LowerSkill(skill, value);
		}
	}
}
