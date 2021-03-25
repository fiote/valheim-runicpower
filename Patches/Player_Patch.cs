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
	public class ExtendedPlayerData {
		public DamageTypeValues powerModifiers = new DamageTypeValues();
		public Inventory spellsBarInventory = new Inventory(nameof(spellsBarInventory), null, SpellsBar.slotCount, 1);
		public Inventory spellsBarHotKeys = new Inventory(nameof(spellsBarHotKeys), null, SpellsBar.slotCount, 1);

		private Player _player;
		private bool _isLoading;
		public const string Sentinel = "<|>";

		public ExtendedPlayerData(Player player) {
			Debug.Log("ExtendedPlayerData CREATED");
			_player = player;
			spellsBarInventory.m_onChanged += OnInventoryChanged;
		}

		public void Save() {
			Debug.Log("ExtendedPlayerData SAVING...");
			var pkg = new ZPackage();
			spellsBarInventory.Save(pkg);
			SaveValue(_player, nameof(spellsBarInventory), pkg.GetBase64());
			Debug.Log("ExtendedPlayerData SAVED!");
		}

		public void Load() {
			Debug.Log("ExtendedPlayerData LOADING...");
			LoadValue(_player, "ExtendedPlayerData", out var init);
			if (LoadValue(_player, nameof(spellsBarInventory), out var quickSlotData)) {
				var pkg = new ZPackage(quickSlotData);
				_isLoading = true;
				spellsBarInventory.Load(pkg);
				_isLoading = false;
			}
			Debug.Log("ExtendedPlayerData LOADED!");
		}

		private void OnInventoryChanged() {
			if (_isLoading) return;
			Save();
		}

		private static void SaveValue(Player player, string key, string value) {
			if (player.m_knownTexts.ContainsKey(key)) {
				player.m_knownTexts.Remove(key);
			}

			key = Sentinel + key;
			if (player.m_knownTexts.ContainsKey(key)) {
				player.m_knownTexts[key] = value;
			} else {
				player.m_knownTexts.Add(key, value);
			}
		}

		private static bool LoadValue(Player player, string key, out string value) {
			if (!player.m_knownTexts.TryGetValue(key, out value)) key = Sentinel + key;
			return player.m_knownTexts.TryGetValue(key, out value);
		}
	}

	[HarmonyPatch(typeof(Player), "Load")]
	public static class Player_Load_Patch {
		public static void Postfix(Player __instance) {
			Debug.Log("Player_Load_Postfix");
			__instance.GetExtendedData().Load();
		}
	}

	[HarmonyPatch(typeof(Player), "Save")]
	public static class Player_Save_Patch {
		public static void Prefix(Player __instance) {
			Debug.Log("Player_Save_Prefix");
			__instance.GetExtendedData().Save();
		}
	}

	[HarmonyPatch(typeof(Player), "Awake")]
	public static class Player_Awake_Patch {
		public static void Postfix(Player __instance) {
			Debug.Log("Player_Awake_Patch");
			__instance.UpdateSpellBars();
		}
	}

	public static class PlayerExtensions {

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
				SpellsBar.spellsBarGrid.UpdateInventory(inv, __instance, invGui?.m_dragItem);
				SpellsBar.spellsBarHotkeys.UpdateInventory(inv, __instance, invGui?.m_dragItem);
				return true;
			}
			return false;
		}

		public static ItemDrop.ItemData GetSpellsBarItem(this Player __instance, int index) {
			if (index < 0 || index > SpellsBar.slotCount) return null;
			var spellsBarInventory = __instance.GetSpellsBarInventory();
			return spellsBarInventory?.GetItemAt(index, 0);
		}

		public static List<Inventory> GetAllInventories(this Player __instance) {
			var result = new List<Inventory>();
			result.Add(__instance.m_inventory);
			var spellsBarInventory = __instance.GetSpellsBarInventory();
			if (spellsBarInventory != null) result.Add(spellsBarInventory);
			return result;
		}
	}
}
