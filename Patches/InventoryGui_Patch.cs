using System;
using System.Linq;
using HarmonyLib;
using RunicPower;
using RunicPower.Core;
using RunicPower.Patches;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RunicPower.Patches {

    [HarmonyPatch(typeof(InventoryGui), "Awake")]
    public static class InventoryGui_Awake_Patch {
        public static void Postfix(InventoryGui __instance) {
            RunicPower.Debug("InventoryGui_Awake_Patch Postfix");
            SpellsBar.CreateInventoryBar(__instance);
        }
    }

    [HarmonyPatch(typeof(InventoryGui), "DoCrafting")]
    public static class InventoryGui_DoCrafting_Patch {
        public static bool Prefix(InventoryGui __instance, Player player) {
            // if it's not a rune, do the normal flow
            var data = __instance.m_craftRecipe?.m_item?.m_itemData?.GetRuneData();
            if (data == null) return true;
            // if the player does not have the requeriments, do the normal flow
            var qualityLevel = 1;
            if (!player.HaveRequirements(__instance.m_craftRecipe, discover: false, qualityLevel) && !player.NoCostCheat()) return true;
            // getting hte spell inventory
            var inv = SpellsBar.invBarGrid.m_inventory;
            // if there is not an 'empty' slot, do the normal flow
            if (!inv.HaveEmptySlot()) {
                return true;
            }
            // trying to craft the item
            long playerID = player.GetPlayerID();
            string playerName = player.GetPlayerName();
            var crafted = inv.AddItem(__instance.m_craftRecipe.m_item.gameObject.name, __instance.m_craftRecipe.m_amount, qualityLevel, __instance.m_craftVariant, playerID, playerName);
            if (crafted != null) {
                if (!player.NoCostCheat()) {
                    player.ConsumeResources(__instance.m_craftRecipe.m_resources, qualityLevel);
                }
                __instance.UpdateCraftingPanel();
            }
            // displaying some effects
            CraftingStation currentCraftingStation = Player.m_localPlayer.GetCurrentCraftingStation();
            var effs = (currentCraftingStation != null) ? currentCraftingStation.m_craftItemDoneEffects : __instance.m_craftItemDoneEffects;
            effs.Create(player.transform.position, Quaternion.identity);

            Game.instance.GetPlayerProfile().m_playerStats.m_crafts++;
            Gogan.LogEvent("Game", "Crafted", __instance.m_craftRecipe.m_item.m_itemData.m_shared.m_name, qualityLevel);

            return false;
        }
    }



    [HarmonyPatch(typeof(InventoryGui), "SetRecipe")]
    public static class InventoryGui_SetRecipe_Patch {

        static float m_craftDuration_original = 2f;
        static float m_craftDuration_runes = 0.5f;

        public static void Prefix(InventoryGui __instance, int index, bool center) {
            RunicPower.Debug("InventoryGui_SetRecipe_Patch Prefix");
            RunicPower.ClearCache();
        }

        public static void Postfix(InventoryGui __instance, int index, bool center) {
            RunicPower.Debug("InventoryGui_SetRecipe_Patch Postfix");
            var item = __instance?.m_selectedRecipe.Key?.m_item?.m_itemData;
            var rune = item?.GetRuneData();
            var ext = Player.m_localPlayer?.ExtendedPlayer();

            if (rune == null) {
                ext?.SetCraftingRuneItem(null);
                __instance.m_craftDuration = m_craftDuration_original;
                return;
            }

            ext?.SetCraftingRuneItem(item);
            __instance.m_craftDuration = m_craftDuration_runes;
        }
    }

    [HarmonyPatch(typeof(InventoryGui), "OnSelectedItem")]
    public static class InventoryGui_OnSelectedItem_Patch {
        public static bool Prefix(InventoryGui __instance, InventoryGrid grid, ItemDrop.ItemData item, Vector2i pos, InventoryGrid.Modifier mod) {
            RunicPower.Debug("InventoryGui_OnSelectedItem_Patch Prefix");
            // if we're not moving from the spellbars, do nothing (normal flow)
            if (__instance.m_dragInventory?.m_name != "spellsBarInventory") return true;
            // if we moving TO the spellsbars, do nothing (normal flow)
            if (grid.m_inventory?.m_name == "spellsBarInventory") return true;
            // so if we're moving from the spellsbar to another inventory
            ItemDrop.ItemData itemAt = grid.GetInventory().GetItemAt(pos.x, pos.y);
            if (itemAt != null) {
                // and there is an item at the destination
                var runeData = itemAt.GetRuneData();
                // and its not a rune
                if (runeData == null) {
                    // we cant swap that!
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, "You can't swap a rune for a non-rune item.");
                    return false;
				}
			}
            return true;
        }
    }
}