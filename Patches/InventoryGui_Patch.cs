using System;
using System.Linq;
using HarmonyLib;
using RunicPower.Core;
using RunicPower.Patches;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RuneStones.Patches {


    public static class InventoryGui_Patch {

        public static RectTransform goRect;

        [HarmonyPatch(typeof(InventoryGui), "Awake")]
        public static class InventoryGui_Awake_Patch {
            public static void Postfix(InventoryGui __instance) {
                BuildSpellsBarGrid(__instance);
            }
        }

        private static void BuildSpellsBarGrid(InventoryGui inventoryGui) {
            BuildInventoryGrid(ref SpellsBar.spellsBarGrid, SpellsBar.spellsBarGridName, new Vector2(1000, 103), new Vector2((74 * SpellsBar.slotCount) + 10, 90), inventoryGui);
        }

        private static void BuildInventoryGrid(ref InventoryGrid grid, string name, Vector2 position, Vector2 size, InventoryGui inventoryGui) {
            if (grid != null) {
				Object.Destroy(grid);
                grid = null;
            }

            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(inventoryGui.m_player, false);

            var highlight = new GameObject("SelectedFrame", typeof(RectTransform));
            highlight.transform.SetParent(go.transform, false);
            highlight.AddComponent<Image>().color = Color.yellow;
            var highlightRT = highlight.transform as RectTransform;
            highlightRT.anchoredPosition = new Vector2(0, 0);
            highlightRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x + 2);
            highlightRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y + 2);
            highlightRT.localScale = new Vector3(1, 1, 1);

            var bkg = inventoryGui.m_player.Find("Bkg").gameObject;
            var background = Object.Instantiate(bkg, go.transform);
            background.name = name + "Bkg";
            var backgroundRT = background.transform as RectTransform;
            backgroundRT.anchoredPosition = new Vector2(0, 0);
            backgroundRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            backgroundRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            backgroundRT.localScale = new Vector3(1, 1, 1);

            grid = go.AddComponent<InventoryGrid>();
            grid.name = name + "Grid";
            var root = new GameObject("Root", typeof(RectTransform));
            root.transform.SetParent(go.transform, false);

            var rect = go.transform as RectTransform;
            goRect = rect;
            rect.anchoredPosition = position;

            grid.m_elementPrefab = inventoryGui.m_playerGrid.m_elementPrefab;
            grid.m_gridRoot = root.transform as RectTransform;
            grid.m_elementSpace = inventoryGui.m_playerGrid.m_elementSpace;
            grid.ResetView();

            grid.m_onSelected += OnSelected(inventoryGui);
            grid.m_onRightClick += OnRightClicked(inventoryGui);

            grid.m_uiGroup = grid.gameObject.AddComponent<UIGroupHandler>();
            grid.m_uiGroup.m_groupPriority = 1;
            grid.m_uiGroup.m_active = true;
            grid.m_uiGroup.m_enableWhenActiveAndGamepad = highlight;

            var list = inventoryGui.m_uiGroups.ToList();
            list.Insert(2, grid.m_uiGroup);
            inventoryGui.m_uiGroups = list.ToArray();
        }


        private static Action<InventoryGrid, ItemDrop.ItemData, Vector2i, InventoryGrid.Modifier> OnSelected(InventoryGui inventoryGui) {
            return (InventoryGrid inventoryGrid, ItemDrop.ItemData item, Vector2i pos, InventoryGrid.Modifier mod) => {
                var current = (item != null) ? item : inventoryGui.m_dragItem;
                var rune = current?.GetRune();

                // if (rune != null) {
                inventoryGui.OnSelectedItem(inventoryGrid, item, pos, mod);
                // } else {
                //  Player.m_localPlayer.Message(MessageHud.MessageType.Center, "You can't put a non-rune item on this slot.");
                // }
            };
        }

        private static Action<InventoryGrid, ItemDrop.ItemData, Vector2i> OnRightClicked(InventoryGui inventoryGui) {
            return (InventoryGrid inventoryGrid, ItemDrop.ItemData item, Vector2i pos) => {
                var player = Player.m_localPlayer;
                if (item == null || player == null) return;
                // if (player.ConsumeItem(player.m_inventory, item))
                    player.UseItem(player.m_inventory, item, true);
            };
        }

        [HarmonyPatch(typeof(InventoryGui), "UpdateInventory")]
        public static class InventoryGui_UpdateInventory_Patch {
            public static void Postfix(InventoryGui __instance, Player player) {
                player.UpdateSpellBars();
            }
        }
    }
}