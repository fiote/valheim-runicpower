using Common;
using HarmonyLib;
using RunicPower.Core;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RunicPower.Patches {

    [HarmonyPatch(typeof(ItemDrop.ItemData), "GetTooltip", typeof(ItemDrop.ItemData), typeof(int), typeof(bool))]
    public static class ItemData_GetTooltip_Patch {
        private static bool Prefix(ref string __result, ItemDrop.ItemData item, int qualityLevel, bool crafting) {
            var rune = item.GetRune();
            if (rune == null) return true;
            __result = rune.GetTooltip(item);
            return false;
        }
    }
}
