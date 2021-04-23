using RunicPower.Core;
using UnityEngine;

namespace RunicPower.Patches {
	public class Player_Extended : MonoBehaviour {

		public DamageTypeValues powerModifiers = new DamageTypeValues();
		public Inventory spellsBarInventory = new Inventory(nameof(spellsBarInventory), null, SpellsBar.slotCount, 1);
		public Inventory spellsBarHotKeys = new Inventory(nameof(spellsBarHotKeys), null, SpellsBar.slotCount, 1);

		private Player player;
		private bool _isLoading;

		public ItemDrop.ItemData craftingRuneItem = null;
		public ItemDrop.ItemData lootingRuneItem = null;
		public ItemDrop.ItemData lootedRuneItem = null;
		public ItemDrop.ItemData selectingRuneItem = null;

		public const string Sentinel = "<|>";

		public void SetPlayer(Player player) {
			this.player = player;
			spellsBarInventory.m_onChanged += OnInventoryChanged;
		}

		public void SetLootingRuneItem(ItemDrop.ItemData item) {
			lootingRuneItem = item;
			lootedRuneItem = null;
		}

		public void SetCraftingRuneItem(ItemDrop.ItemData item) {
			craftingRuneItem = item;
		}

		public void SetSelectingRuneItem(ItemDrop.ItemData item) {
			selectingRuneItem = item;
		}

		public void Save() {
			var pkg = new ZPackage();
			spellsBarInventory.Save(pkg);
			SaveValue(player, nameof(spellsBarInventory), pkg.GetBase64());
		}

		public void Load() {
			LoadValue(player, "ExtendedPlayerData", out var init);
			if (LoadValue(player, nameof(spellsBarInventory), out var quickSlotData)) {
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
