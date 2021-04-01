using HarmonyLib;
using RunicPower;
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
            var parent = __instance.m_rootObject;
            var inventoryGui = InventoryGui.instance;
            var position = new Vector2(-SpellsBar.barSize.x/2 + 54  , SpellsBar.barSize.y);
            SpellsBar.hotkeysRect = SpellsBar.CreateGameObject(ref SpellsBar.hotkeysGrid, inventoryGui, parent, SpellsBar.spellsBarHotkeysName, position, "hotkeys", SpellsBar.barSize);
        }
    }

    [HarmonyPatch(typeof(Hud), "SetVisible")]
    public static class Hud_SetVisible_Patch {

        public static void Postfix(Hud __instance) {
            SpellsBar.UpdateVisibility();
        }
    }

    [HarmonyPatch(typeof(Hud), "UpdateBuild")]
    public static class Hud_UpdateBuild_Patch {

        public static void Postfix(Hud __instance) {
            SpellsBar.UpdateVisibility();            
        }
    }
}