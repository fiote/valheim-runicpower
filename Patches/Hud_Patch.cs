using HarmonyLib;
using RunicPower.Core;
using RunicPower.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RuneStones.Patches {

    [HarmonyPatch(typeof(Hud), "Awake")]
    public static class Hud_Awake_Patch {
        public static void Postfix(Hud __instance) {
            var hotkeyBar = __instance.GetComponentInChildren<HotkeyBar>();
            var inventoryGui = InventoryGui.instance;
            var parent = hotkeyBar.gameObject;
            SpellsBar.CreateGameObject(ref SpellsBar.spellsBarHotkeys, inventoryGui, parent, SpellsBar.spellsBarHotkeysName, new Vector2(1002, -4), false);
        }
    }
}