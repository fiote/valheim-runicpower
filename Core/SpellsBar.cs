using BepInEx;
using BepInEx.Configuration;
using RuneStones.Patches;
using RunicPower.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RunicPower.Core {

    public class SpellShortcut {
        public KeyCode? modifier;
        public KeyCode key;

        public SpellShortcut(KeyCode modifier, KeyCode key) {
            this.modifier = modifier;
            this.key = key;
		}
    }

	public class SpellsBar {

		public const int slotCount = 10;
        public static string spellsBarGridName = "SpellsBarGrid";
        public static string spellsBarHotkeysName = "SpellsBarHotkeys";
        public static readonly Dictionary<int, SpellShortcut> shortcuts = new Dictionary<int, SpellShortcut>();

        public static InventoryGrid spellsBarGrid;
        public static InventoryGrid spellsBarHotkeys;

        public static void RegisterKeybinds(ConfigFile config) {
            for (var i = 0; i < slotCount; i++) {
                var knumber = "Alpha" + (i == slotCount - 1 ? 0 : i + 1);
                var key = (KeyCode)System.Enum.Parse(typeof(KeyCode), knumber.ToString());
                shortcuts[i] = new SpellShortcut(KeyCode.LeftShift, key);
            }

            for (var i = 0; i < slotCount; i++) {
                var label = GetBindingLabel(i);
                Debug.Log(label);
            }
        }

        public static void CheckInputs() {
            var player = Player.m_localPlayer;

            for (int i = 0; i < slotCount; ++i) {
                CheckQuickUseInput(player, i);
            }
        }

        public static string GetBindingLabel(int index) {
            var shortcut = shortcuts[index];
            if (shortcut == null) return "??";

            var mod = shortcut.modifier?.ToString();
            var key = shortcut.key.ToString();

            if (mod == "LeftControl") mod = "Ctrl";
            if (mod == "LeftShift") mod = "Shift";
            if (mod == "LeftAlt") mod = "Alt";

            key = key.Replace("Alpha", "");

            return (mod != null) ? mod+"+"+key: key;
        }

        public static void CheckQuickUseInput(Player player, int index) {
            var shortcut = shortcuts[index];
            if (shortcut == null) return;

            var modOk = shortcut.modifier == null || Input.GetKey((KeyCode)shortcut.modifier);
            var keyOk = Input.GetKeyDown(shortcut.key);

            if (modOk && keyOk) {
                var item = player.GetSpellsBarItem(index);
                if (item != null) player.UseItem(null, item, false);
            }
        }

        public static void CreateGameObject(ref InventoryGrid grid, InventoryGui inventoryGui, GameObject parent, string name, Vector2 position, bool isInventory = false, Vector2 size = new Vector2()) {
            // go
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);

            // rect
            var goRect = go.transform as RectTransform;
            goRect.anchoredPosition = position;

            // hightlight
            var highlight = new GameObject("SelectedFrame", typeof(RectTransform));

            if (isInventory) {
                highlight.transform.SetParent(go.transform, false);
                // highlight.AddComponent<Image>().color = Color.yellow;
                var highlightRT = highlight.transform as RectTransform;
                highlightRT.anchoredPosition = new Vector2(0, 0);
                highlightRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x + 2);
                highlightRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y + 2);
                highlightRT.localScale = new Vector3(1, 1, 1);
            }

            // background
            if (isInventory) {
                var bkg = inventoryGui.m_player.Find("Bkg").gameObject;
                var background = Object.Instantiate(bkg, go.transform);
                background.name = name + "Bkg";
                var backgroundRT = background.transform as RectTransform;
                backgroundRT.anchoredPosition = new Vector2(0, 0);
                backgroundRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
                backgroundRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
                backgroundRT.localScale = new Vector3(1, 1, 1);
            }

            // grid
            grid = go.AddComponent<InventoryGrid>();
            grid.name = name + "Grid";
            var root = new GameObject("Root", typeof(RectTransform));
            root.transform.SetParent(go.transform, false);
            grid.m_elementPrefab = inventoryGui.m_playerGrid.m_elementPrefab;
            grid.m_gridRoot = root.transform as RectTransform;
            grid.m_elementSpace = inventoryGui.m_playerGrid.m_elementSpace;
            grid.ResetView();

            if (isInventory) {
                grid.m_onSelected = null;
                grid.m_onRightClick = null;
            } else {
                grid.m_onSelected += OnSelected(inventoryGui);
                grid.m_onRightClick += OnRightClicked(inventoryGui);
            }

            grid.m_uiGroup = grid.gameObject.AddComponent<UIGroupHandler>();
            grid.m_uiGroup.m_groupPriority = 1;
            grid.m_uiGroup.m_active = true;
            grid.m_uiGroup.m_enableWhenActiveAndGamepad = highlight;

            // list
            var list = inventoryGui.m_uiGroups.ToList();
            list.Insert(2, grid.m_uiGroup);
            inventoryGui.m_uiGroups = list.ToArray();
        }
        public static Action<InventoryGrid, ItemDrop.ItemData, Vector2i, InventoryGrid.Modifier> OnSelected(InventoryGui inventoryGui) {
            return (InventoryGrid inventoryGrid, ItemDrop.ItemData item, Vector2i pos, InventoryGrid.Modifier mod) => {
                var current = (item != null) ? item : inventoryGui.m_dragItem;
                var rune = current?.GetRune();
                if (rune != null) {
                    inventoryGui.OnSelectedItem(inventoryGrid, item, pos, mod);
                } else {
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, "You can't put a non-rune item on this slot.");
                }
            };
        }

        public static Action<InventoryGrid, ItemDrop.ItemData, Vector2i> OnRightClicked(InventoryGui inventoryGui) {
            return (InventoryGrid inventoryGrid, ItemDrop.ItemData item, Vector2i pos) => {
                var player = Player.m_localPlayer;
                if (item == null || player == null) return;
                // if (player.ConsumeItem(player.m_inventory, item))
                player.UseItem(player.m_inventory, item, true);
            };
        }

    }
}
