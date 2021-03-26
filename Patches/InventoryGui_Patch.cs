using System;
using System.Linq;
using HarmonyLib;
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
            SpellsBar.CreateGameObject(ref SpellsBar.spellsBarGrid, __instance, parent, name, position, true, size);
        }
    }

    [HarmonyPatch(typeof(InventoryGui), "UpdateInventory")]
    public static class InventoryGui_UpdateInventory_Patch {
        public static void Postfix(InventoryGui __instance, Player player) {
            player.UpdateSpellBars();
        }
    }
}