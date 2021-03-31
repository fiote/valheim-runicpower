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
            var parent = __instance.m_player.gameObject;
            var name = SpellsBar.spellsBarGridName;
            var position = new Vector2(1000, 103);
            var size = new Vector2((74 * SpellsBar.slotCount) + 10, 90);
            SpellsBar.invBarRect = SpellsBar.CreateGameObject(ref SpellsBar.invBarGrid, __instance, parent, name, position, "inventory", size);
        }
    }

    [HarmonyPatch(typeof(InventoryGui), "UpdateInventory")]
    public static class InventoryGui_UpdateInventory_Patch {
        public static void Postfix(InventoryGui __instance, Player player) {
            player.UpdateSpellBars();
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