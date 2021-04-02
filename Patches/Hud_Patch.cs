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

namespace RunicPower.Patches {

    [HarmonyPatch(typeof(Hud), "Awake")]
    public static class Hud_Awake_Patch {
        public static void Postfix(Hud __instance) {
            RunicPower.Debug("Hud_Awake_Patch Postfix");
            SpellsBar.CreateHotkeysBar(__instance);
        }
    }

    [HarmonyPatch(typeof(Hud), "SetVisible")]
    public static class Hud_SetVisible_Patch {

        public static void Postfix(Hud __instance) {
            RunicPower.Debug("Hud_SetVisible_Patch Postfix");
            SpellsBar.UpdateVisibility();
        }
    }

    [HarmonyPatch(typeof(Hud), "UpdateBuild")]
    public static class Hud_UpdateBuild_Patch {

        public static void Postfix(Hud __instance) {
            RunicPower.Debug("Hud_UpdateBuild_Patch Postfix");
            SpellsBar.UpdateVisibility();            
        }
    }
}