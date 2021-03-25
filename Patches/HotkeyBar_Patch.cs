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
    class HotkeyBar_Patch {

        public static RectTransform goRect;


        [HarmonyPatch(typeof(HotkeyBar), "UpdateIcons")]
        public static class UpdateIcons_Patch {
            public static void Postfix(HotkeyBar __instance, Player player) {
                var updated = player?.UpdateSpellBars();
                // if (updated == true) SpellsBar.spellsBarHotkeys.gameObject.SetActive(__instance.gameObject.activeSelf);
            }
        }

        [HarmonyPatch(typeof(Hud), "Awake")]
        public static class Hud_Awake_Patch {
            public static void Postfix(Hud __instance) {
                Debug.Log("HUD AWAKE " + __instance.name);

                var inventoryGui = InventoryGui.instance;
                var name = SpellsBar.spellsBarHotkeysName;
                var size = new Vector2((74 * SpellsBar.slotCount) + 10, 90);
                
                var hotkeyBar = __instance.GetComponentInChildren<HotkeyBar>();
                Debug.Log("hotkeyBar = " + hotkeyBar.name);
                var go = new GameObject(name, typeof(RectTransform));

                go.transform.SetParent(hotkeyBar.gameObject.transform, false);

                goRect = go.transform as RectTransform;
                goRect.anchoredPosition = new Vector2(1002, -4);

                SpellsBar.spellsBarHotkeys = go.AddComponent<InventoryGrid>();
                var grid = SpellsBar.spellsBarHotkeys;

                grid.name = name + "Grid";
                var root = new GameObject("Root", typeof(RectTransform));
                root.transform.SetParent(go.transform, false);

                grid.m_elementPrefab = inventoryGui.m_playerGrid.m_elementPrefab;
                grid.m_gridRoot = root.transform as RectTransform;
                grid.m_elementSpace = inventoryGui.m_playerGrid.m_elementSpace;
                grid.ResetView();

                grid.m_onSelected = null;
                grid.m_onRightClick = null;

                grid.m_uiGroup = grid.gameObject.AddComponent<UIGroupHandler>();
                grid.m_uiGroup.m_groupPriority = 1;
                grid.m_uiGroup.m_active = true;

                var list = inventoryGui.m_uiGroups.ToList();
                list.Insert(2, grid.m_uiGroup);
                inventoryGui.m_uiGroups = list.ToArray();

                Debug.Log("hotbar created i guess?");

            }
        }

    }
}