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
        public static void Prefix(InventoryGui __instance, Player player, float dt) {
            var item = __instance.m_selectedRecipe.Value;
            var rune = item?.GetRune();
            if (rune == null) return;
            InventoryGui_Extended.isCraftingRune = item;

        }
        public static void Postfix(InventoryGui __instance, Player player, float dt) {
            InventoryGui_Extended.isCraftingRune = null;

        }
    }
}