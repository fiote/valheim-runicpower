using System;
using System.Linq;
using HarmonyLib;
using RunicPower;
using RunicPower.Core;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RuneStones.Patches {

    [HarmonyPatch(typeof(InventoryGrid), "GetElement", typeof(int), typeof(int), typeof(int))]
    public static class InventoryGrid_GetElement_Patch {
        private static bool Prefix(InventoryGrid __instance, ref InventoryGrid.Element __result, int x, int y, int width) {
            if (!__instance.IsRunic()) return true;
            var index = y * width + x;
            __result = (index < 0 || index >= __instance.m_elements.Count) ? null : __instance.m_elements[index];
            return false;
        }
    }

    [HarmonyPatch(typeof(InventoryGrid), "UpdateGui", typeof(Player), typeof(ItemDrop.ItemData))]
    public static class InventoryGrid_UpdateGui_Patch {
        private static void Postfix(InventoryGrid __instance) {
            if (!__instance.IsRunic()) return;

            for (var i = 0; i < SpellsBar.slotCount; ++i) {
                var element = __instance.m_elements[i];
                var bindingText = element.m_go.transform.Find("binding").GetComponent<Text>();
                bindingText.enabled = true;
                bindingText.horizontalOverflow = HorizontalWrapMode.Overflow;
                bindingText.text = SpellsBar.GetBindingLabel(i);
                bindingText.fontSize = 15;
            }
        }
    }
}