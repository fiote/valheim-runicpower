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

			Debug.Log("currentVersion: "+spellsBarInventory.currentVersion);
			Debug.Log("m_inventory.Count: "+spellsBarInventory.m_inventory.Count);
			foreach (ItemDrop.ItemData item in spellsBarInventory.m_inventory) {
				if (item.m_dropPrefab == null) {
					Debug.Log("Item missing prefab " + item.m_shared.m_name);
					Debug.Log("");
				} else {
					Debug.Log("Prefab name "+item.m_dropPrefab.name);
				}
				Debug.Log("m_stack: " + item.m_stack);
				Debug.Log("m_durability: " + item.m_durability);
				Debug.Log("m_gridPos: " + item.m_gridPos);
				Debug.Log("m_equiped: " + item.m_equiped);
				Debug.Log("m_quality: " + item.m_quality);
				Debug.Log("m_variant: " + item.m_variant);
				Debug.Log("m_crafterID: " + item.m_crafterID);
				Debug.Log("m_crafterName: " + item.m_crafterName);
			}

			SaveValue(_player, nameof(spellsBarInventory), pkg.GetBase64());
			Debug.Log("ExtendedPlayerData SAVED!");
		}

		public void Load() {
			Debug.Log("ExtendedPlayerData LOADING...");

			LoadValue(_player, "ExtendedPlayerData", out var init);

			if (LoadValue(_player, nameof(spellsBarInventory), out var quickSlotData)) {
				var pkg = new ZPackage(quickSlotData);
				_isLoading = true;

				int num = pkg.ReadInt();
				int num2 = pkg.ReadInt();
				spellsBarInventory.m_inventory.Clear();
				for (int i = 0; i < num2; i++) {
					string text = pkg.ReadString();
					int stack = pkg.ReadInt();
					float durability = pkg.ReadSingle();
					Vector2i pos = pkg.ReadVector2i();
					bool equiped = pkg.ReadBool();
					int quality = 1;
					if (num >= 101) {
						quality = pkg.ReadInt();
					}
					int variant = 0;
					if (num >= 102) {
						variant = pkg.ReadInt();
					}
					long crafterID = 0L;
					string crafterName = "";
					if (num >= 103) {
						crafterID = pkg.ReadLong();
						crafterName = pkg.ReadString();
					}
					if (text != "") {
						Debug.Log("text=" + text + ", stack=" + stack + ", durability=" + durability + ", pos=" + pos + ", equiped=" + equiped + ", quality=" + quality + ", variant=" + variant + ", crafterID=" + crafterID + ", crafterName=" + crafterName);
					}
				}

				pkg = new ZPackage(quickSlotData);
				spellsBarInventory.Load(pkg);
				// _player.m_inventory.MoveAll(spellsBarInventory);

				pkg = new ZPackage(quickSlotData);
				spellsBarInventory.Save(pkg);
				SaveValue(_player, nameof(spellsBarInventory), pkg.GetBase64());

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

	[HarmonyPatch(typeof(Player), "Awake")]
	public static class Player_Awake_Patch {
		public static void Postfix(Player __instance) {
		}
	}

	[HarmonyPatch(typeof(Player), "Load")]
	public static class Player_Load_Patch {
		public static void Postfix(Player __instance) {
			Debug.Log("Player_Load_Postfix");
			__instance.GetExtendedData().Load();
			__instance.EquipIventoryItems();
		}
	}
 
	[HarmonyPatch(typeof(Player), "Save")]
	public static class Player_Save_Patch {
		public static void Prefix(Player __instance) {
			Debug.Log("Player_Save_Prefix");
			__instance.GetExtendedData().Save();
		}
	}

	public static class PlayerExtensions {

		public static Dictionary<string, ExtendedPlayerData> mapping = new Dictionary<string, ExtendedPlayerData>();
		public static ExtendedPlayerData GetExtendedData(this Player __instance) {
			var key = __instance.GetInstanceID().ToString();
			if (key == null) return null;
			// getting the current extendedData
			var ext = mapping.ContainsKey(key) ? mapping[key] : null;
			// if it does not exist, create one
			if (ext == null) mapping[key] = ext = new ExtendedPlayerData(__instance);
			// and return it
			return ext;
		}

		public static Inventory GetSpellsBarInventory(this Player __instance) {
			var ext = __instance.GetExtendedData();
			return ext?.spellsBarInventory;
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
