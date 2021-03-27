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
			_player = player;
			spellsBarInventory.m_onChanged += OnInventoryChanged;
		}

		public void Save() {
			var pkg = new ZPackage();
			spellsBarInventory.Save(pkg);
			SaveValue(_player, nameof(spellsBarInventory), pkg.GetBase64());
		}

		public void Load() {
			LoadValue(_player, "ExtendedPlayerData", out var init);
			if (LoadValue(_player, nameof(spellsBarInventory), out var quickSlotData)) {
				var pkg = new ZPackage(quickSlotData);
				_isLoading = true;
				spellsBarInventory.Load(pkg);
				_isLoading = false;
			}
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
}
