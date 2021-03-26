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
		public static ExtendedPlayerData GetExtendedData(this Player __instance) {
			var key = __instance.GetInstanceID().ToString();
			if (key == null) return null;
			var ext = mapping.ContainsKey(key) ? mapping[key] : null;
			if (ext == null) mapping[key] = ext = new ExtendedPlayerData(__instance);
			// and return it
			return ext;
		}

		public static Inventory GetSpellsBarInventory(this Player __instance) {
			var ext = __instance.GetExtendedData();
			return ext?.spellsBarInventory;
		}

		public static bool UpdateSpellBars(this Player __instance) {
			if (__instance == null) return false;
			var inv = __instance.GetSpellsBarInventory();
			var invGui = InventoryGui.instance;
			if (inv != null && invGui != null) {
				SpellsBar.invBarGrid.UpdateInventory(inv, __instance, invGui?.m_dragItem);
				SpellsBar.hotkeysGrid.UpdateInventory(inv, __instance, invGui?.m_dragItem);
				return true;
			}
			return false;
		}

		public static ItemDrop.ItemData GetSpellsBarItem(this Player __instance, int index) {
			if (index < 0 || index > SpellsBar.slotCount) return null;
			var spellsBarInventory = __instance.GetSpellsBarInventory();
			return spellsBarInventory?.GetItemAt(index, 0);
		}
	}
}
