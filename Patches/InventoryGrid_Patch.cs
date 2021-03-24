using System;
using System.Linq;
using HarmonyLib;
using RunicPower.Core;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RuneStones.Patches {

    public static class InventoryGrid_Patch {

        //public InventoryGrid.Element GetElement(int x, int y, int width) => this.m_elements[y * width + x];
        [HarmonyPatch(typeof(InventoryGrid), "GetElement", typeof(int), typeof(int), typeof(int))]
        public static class InventoryGrid_GetElement_Patch {
            private static bool Prefix(InventoryGrid __instance, ref InventoryGrid.Element __result, int x, int y, int width) {
                var index = y * width + x;
                if (index < 0 || index >= __instance.m_elements.Count) {
                    Debug.LogError($"Tried to get element for item ({x}, {y}) in inventory ({__instance.m_inventory.m_name}) but that element is outside the bounds!");
                    __result = null;
                } else {
                    __result = __instance.m_elements[index];
                }

                return false;
            }
        }

        //private void UpdateGui(Player player, ItemDrop.ItemData dragItem)
        [HarmonyPatch(typeof(InventoryGrid), "UpdateGui", typeof(Player), typeof(ItemDrop.ItemData))]
        public static class InventoryGrid_UpdateGui_Patch {
            private static void Postfix(InventoryGrid __instance) {
                if (__instance.name == SpellsBar.barName) {
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

    }
}