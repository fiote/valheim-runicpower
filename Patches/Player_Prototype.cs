using Common;
using HarmonyLib;
using RunicPower.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RunicPower.Patches {

	public static class Player_Prototype {

		public static Dictionary<string, ExtendedPlayerData> mapping = new Dictionary<string, ExtendedPlayerData>();
		public static ExtendedPlayerData ExtendedPlayer(this Player __instance) {
			var key = __instance.GetInstanceID().ToString();
			if (key == null) return null;
			var ext = mapping.ContainsKey(key) ? mapping[key] : null;
			if (ext == null) mapping[key] = ext = new ExtendedPlayerData(__instance);
			// and return it
			return ext;
		}

		public static Inventory GetSpellsBarInventory(this Player __instance) {
			var ext = __instance.ExtendedPlayer();
			return ext?.spellsBarInventory;
		}

		public static void UpdateSpellBars(this Player __instance) {
			var inv = __instance?.GetSpellsBarInventory();
			var invGui = InventoryGui.instance;
			if (inv != null && invGui != null) {
				try {
					SpellsBar.invBarGrid?.UpdateInventory(inv, __instance, invGui?.m_dragItem);
				} catch (Exception e) {
					Debug.Log("SpellsBar.invBarGrid failed " + e.Message);
				}
				try {
					SpellsBar.hotkeysGrid?.UpdateInventory(inv, __instance, invGui?.m_dragItem);
				} catch (Exception e) {
					Debug.Log("SpellsBar.hotkeysGrid failed " + e.Message);
				}
			}
		}


		public static ItemDrop.ItemData GetSpellsBarItem(this Player __instance, int index) {
			if (index < 0 || index > SpellsBar.slotCount) return null;
			var spellsBarInventory = __instance.GetSpellsBarInventory();
			return spellsBarInventory?.GetItemAt(index, 0);
		}

		public static void UseRuneFromSpellBar(this Player __instance, ItemDrop.ItemData item) {
			if (item == null) return;
			var inv = __instance.GetSpellsBarInventory();
			__instance.UseItem(inv, item, true);
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
	}
}
