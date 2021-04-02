using System;
using System.Linq;
using HarmonyLib;
using RunicPower;
using RunicPower.Core;
using RunicPower.Patches;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RuneStones.Patches {

    [HarmonyPatch(typeof(InventoryGui), "Awake")]
    public static class InventoryGui_Awake_Patch {
        public static void Postfix(InventoryGui __instance) {
            SpellsBar.CreateInventoryBar(__instance);
        }
    }

    [HarmonyPatch(typeof(InventoryGui), "UpdateInventory")]
    public static class InventoryGui_UpdateInventory_Patch {
        public static void Postfix(InventoryGui __instance, Player player) {
            player.UpdateSpellBars();
        }
    }

    [HarmonyPatch(typeof(InventoryGui), "OnSelectedItem")]
    public static class InventoryGui_OnSelectedItem_Patch {
        public static bool Prefix(InventoryGui __instance, InventoryGrid grid, ItemDrop.ItemData item, Vector2i pos, InventoryGrid.Modifier mod) {
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

    [HarmonyPatch(typeof(InventoryGui), "UpdateRecipe")]
    public static class InventoryGui_UpdateRecipe_Patch {

        static float m_craftDuration_original = 0f;
        static float m_craftDuration_runes = 0.5f;

        public static void Prefix(InventoryGui __instance, Player player, float dt) {
            if (m_craftDuration_original == 0) m_craftDuration_original = __instance.m_craftDuration;
            var recipe = __instance.m_selectedRecipe.Key;
            if (recipe == null) return;
            if (recipe.m_item == null) return;
            var item = recipe.m_item.m_itemData;
            if (item.GetRuneData() == null) return;
            __instance.m_craftDuration = m_craftDuration_runes;
            player.ExtendedPlayer().craftingRuneItem = item;
        }

        public static void Postfix(InventoryGui __instance, Player player, float dt) {
            __instance.m_craftDuration = m_craftDuration_original;
            player.ExtendedPlayer().craftingRuneItem = null;
        }
    }
}