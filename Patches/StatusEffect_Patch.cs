using Common;
using HarmonyLib;
using RunicPower.Core;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RunicPower.Patches {

    [HarmonyPatch(typeof(StatusEffect), "GetTooltipString")]
    public static class ItemData_GetTooltipString_Patch {
        private static bool Prefix(StatusEffect __instance, ref string __result) {
            var rune = __instance.GetRune();
            if (rune == null) return true;
            __result = rune.GetEffectTooltip();
            return false;
        }
    }
}
