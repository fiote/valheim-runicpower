using HarmonyLib;
using RunicPower.Core;
using RunicPower.Patches;
using UnityEngine;

namespace RunicPower {

	[HarmonyPatch(typeof(Humanoid), "UseItem")]
	public static class Humanoid_UseItem_Patch {
		static bool Prefix(Humanoid __instance, Inventory inventory, ItemDrop.ItemData item, bool fromInventoryGui) {
			var data = item.GetRuneData();
			if (data == null) return true;
			if (inventory == null) inventory = __instance.m_inventory;

			if (RunicPower.IsOnCooldown(data)) {
				return RunicPower.ShowMessage(MsgKey.STILL_ON_COOLDOWN, data.name, false);
			}

			if (inventory.m_name != RunicPower.invName) {
				return RunicPower.ShowMessage(MsgKey.CAST_ONLY_SPELLBAR, false);
			}

			var player = __instance as Player;
			var rune = new Rune(data, player);
						
			if (rune.RequiresWeapon() && !rune.GotWeapon()) {
				return RunicPower.ShowMessage(MsgKey.WEAPON_REQUIRED, data.name, false);
			}
			
			if (rune.RequiresShield() && !rune.GotShield()) {
				return RunicPower.ShowMessage(MsgKey.SHIELD_REQUIRED, data.name, false);
			}

			if (rune.CustomEffect() && !rune.CanCastCustom(true)) {
				return false;
			}

			if (!__instance.ConsumeItem(inventory, item)) return true;

			rune.Cast();

			return false;
		}
	}

	[HarmonyPatch(typeof(Humanoid), "SetupEquipment")]
	public static class Humanoid_SetupEquipment_Patch {
		static void Postfix(Humanoid __instance) {
			if (!__instance.IsPlayer()) return;
			RunicPower.ClearCache();
			SpellsBar.UpdateInventory();
		}
	}

	[HarmonyPatch(typeof(Humanoid), "Pickup")]
	public static class Humanoid_Pickup_Patch {
		static void Prefix(Humanoid __instance, GameObject go) {
			if (!__instance.IsPlayer()) return;

			var itemDrop = go.GetComponent<ItemDrop>();
			var runeData = itemDrop?.m_itemData?.GetRuneData();
			if (runeData == null) return;

			Player.m_localPlayer?.ExtendedPlayer(true)?.SetLootingRuneItem(itemDrop.m_itemData);
		}

		static void Postfix(Humanoid __instance, GameObject go) {
			if (!__instance.IsPlayer()) return;
			Player.m_localPlayer?.ExtendedPlayer(true)?.SetLootingRuneItem(null);
		}
	}

}