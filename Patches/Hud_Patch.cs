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
            var position = new Vector2(0, 0);
            SpellsBar.hotkeysRect = SpellsBar.CreateGameObject(ref SpellsBar.hotkeysGrid, inventoryGui, parent, SpellsBar.spellsBarHotkeysName, position, "hotkeys", SpellsBar.barSize);
        }
    }

    [HarmonyPatch(typeof(Hud), "Update")]
    public static class Hud_Update_Patch {

        public static void Postfix(Hud __instance) {
            var bar = SpellsBar.invBarRect;
            if (bar != null) {
                var vars = Chat_InputText_Patch.vars;
                var x = bar.position.x;
                var y = bar.position.y;
                if (vars.ContainsKey("x")) x = vars["x"];
                if (vars.ContainsKey("y")) y = vars["y"];
                bar.position = new Vector2(x, y);
            }

            var hot = SpellsBar.hotkeysRect;
            if (hot != null) {
                hot.position = new Vector2(950, 70);
            }
        }
    }
}