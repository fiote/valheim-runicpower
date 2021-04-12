using BepInEx;
using BepInEx.Configuration;
using RunicPower.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RunicPower.Core {

    public class SpellShortcut {
        public KeyCode modifier;
        public KeyCode auxmod;
        public KeyCode key;

        public SpellShortcut(KeyCode modifier, KeyCode auxmod, KeyCode key) {
            this.modifier = modifier;
            this.auxmod = auxmod;
            this.key = key;
        }
    }

	public class SpellsBar {
		public const int slotCount = 10;
        public static Vector2 barSize = new Vector2((74 * slotCount) + 10, 90);

        public static string spellsBarGridName = "SpellsBarGrid";
        public static string spellsBarHotkeysName = "SpellsBarHotkeys";
        public static readonly Dictionary<int, SpellShortcut> shortcuts = new Dictionary<int, SpellShortcut>();

        public static InventoryGrid invBarGrid;
        public static InventoryGrid hotkeysGrid;

        public static RectTransform invBarRect;
        public static RectTransform hotkeysRect;

        public static Dictionary<RunicPower.KeyModifiers, KeyCode> mod2key = new Dictionary<RunicPower.KeyModifiers, KeyCode> {
            { RunicPower.KeyModifiers.ALT, KeyCode.LeftAlt },
            { RunicPower.KeyModifiers.CTRL, KeyCode.LeftControl },
            { RunicPower.KeyModifiers.SHIFT, KeyCode.LeftShift },
        };

        public static Dictionary<RunicPower.KeyModifiers, KeyCode> mod2aux = new Dictionary<RunicPower.KeyModifiers, KeyCode> {
            { RunicPower.KeyModifiers.ALT, KeyCode.RightAlt },
            { RunicPower.KeyModifiers.CTRL, KeyCode.RightControl },
            { RunicPower.KeyModifiers.SHIFT, KeyCode.RightShift },
        };

        public static void RegisterKeybinds() {
            shortcuts.Clear();
            var mod = mod2key[RunicPower.configHotkeysModifier.Value];
            var aux = mod2aux[RunicPower.configHotkeysModifier.Value];
            for (var i = 0; i < slotCount; i++) {
                var knumber = "Alpha" + (i == slotCount - 1 ? 0 : i + 1);
                var key = (KeyCode)System.Enum.Parse(typeof(KeyCode), knumber.ToString());
                shortcuts[i] = new SpellShortcut(mod, aux, key);
            }
        }

        public static void CheckInputs() {
            var player = Player.m_localPlayer;

            for (int i = 0; i < slotCount; ++i) {
                CheckInputHotKey(player, i);
            }
        }

        public static string GetBindingLabel(int index) {
            var shortcut = shortcuts[index];
            if (shortcut == null) return "??";

            var mod = shortcut.modifier.ToString();
            var key = shortcut.key.ToString();

            if (mod == "LeftControl") mod = "Ctrl";
            if (mod == "LeftShift") mod = "Shift";
            if (mod == "LeftAlt") mod = "Alt";

            key = key.Replace("Alpha", "");

            return (mod != null) ? mod+"+"+key: key;
        }

        public static bool IsSpellHotkeyPressed(int index, bool? assumeKeyOk = false) {
            var shortcut = shortcuts[index];
            if (shortcut == null) return false;
            var modOk = Input.GetKey(shortcut.modifier) || Input.GetKey(shortcut.auxmod);
            var keyOk = Input.GetKeyDown(shortcut.key);
            if (assumeKeyOk == true) keyOk = true;
            return modOk && keyOk;
        }

        public static ItemDrop.ItemData GetSpellHotKeyItem(Player player, int index, bool? assumeKeyOk = false) {
            var pressed = IsSpellHotkeyPressed(index, assumeKeyOk);
            return (pressed) ? player.GetSpellsBarItem(index) : null;
        }

        public static void CheckInputHotKey(Player player, int index) {
            var item = GetSpellHotKeyItem(player, index);
            if (item != null) player.UseRuneFromSpellBar(item);
        }

        public static bool isVisible = false;

        public static void UpdateVisibility() {
            if (hotkeysRect == null) return;

            var visible = true;
            if (visible && Hud.instance?.m_buildHud?.activeSelf == true) visible = false;
            if (visible && Hud.instance?.IsVisible() == false) visible = false;

            UpdateVisibility(visible);
        }

        public static void UpdateVisibility(Boolean visible) {
            if (hotkeysRect == null) return;

            if (visible != isVisible) {
                isVisible = visible;
                hotkeysRect.gameObject.SetActive(isVisible);
            }
        }

        public static void UpdateInventory() {
            UpdateGrid(invBarGrid);
            UpdateGrid(hotkeysGrid);
            UpdateCooldowns();
        }
        
        public static void ClearBindings() {
            mapBindingText.Clear();
            mapCooldownText.Clear();
		}

        public static void UpdateCooldowns() {
            for (var i = 0; i < slotCount; ++i) {
                var spell = invBarGrid?.m_inventory?.GetItemAt(i, 0);
                var name = spell?.GetRuneData()?.name;
                var value = 0;
                var got = (name == null) ? false : RunicPower.activeCooldowns?.TryGetValue(name, out value);
                if (got != true) value = 0;
                SetCooldownText(i, value);
            }
        }

        public static Dictionary<string, Text> mapBindingText = new Dictionary<string, Text>();
        public static Dictionary<string, Text> mapCooldownText = new Dictionary<string, Text>();

        public static void SetCooldownText(int index, int value) {
            SetCooldownText(hotkeysGrid, index, value);
            SetCooldownText(invBarGrid, index, value);
        }
        public static void SetCooldownText(InventoryGrid grid, int index, int value) {
            if (grid == null) return;
            var key = grid.name + ":" + index;

            if (!mapCooldownText.ContainsKey(key)) return;
            if (!mapBindingText.ContainsKey(key)) return;

            var cooldownText = mapCooldownText[key];
            var bindingText = mapBindingText[key];
            if (value > 0) {
                var dsvalue = value.ToString();
                var fontSize = 30;

                if (value >= 60) {
                    var mins = Mathf.Floor(value / 60f);
                    var secs = value - mins * 60;
                    var dssecs = secs.ToString();
                    if (secs < 10) dssecs = '0' + dssecs;
                    dsvalue = mins.ToString() + ":" + dssecs;
                    fontSize = 25;
                }

                cooldownText.gameObject.SetActive(true);
                cooldownText.text = dsvalue;
                cooldownText.fontSize = fontSize;

                bindingText.gameObject.SetActive(false);
            } else {
                cooldownText.gameObject.SetActive(false);
                bindingText.gameObject.SetActive(true);
            }
        }

        public static void SetCooldownText(string name, int value) {
            for (var i = 0; i < slotCount; ++i) {
                var spell = invBarGrid?.m_inventory?.GetItemAt(i, 0);
                if (spell?.GetRuneData().name == name) SetCooldownText(i, value);
            }
        }

        public static void UpdateGrid(InventoryGrid grid) {
            var player = Player.m_localPlayer;
            if (player == null) return;

            var inv = player?.GetSpellsBarInventory();
            var invGui = InventoryGui.instance;
            if (inv == null || invGui == null) return;

            try {
                grid?.UpdateInventory(inv, player, invGui?.m_dragItem);
                for (var i = 0; i < slotCount; ++i) {
                    var key = grid.name + ":" + i;

                    Text bindingText;

                    if (mapBindingText.ContainsKey(key)) {
                        bindingText = mapBindingText[key];
                    } else {
                        var bind = grid.m_elements[i].m_go.transform.Find("binding");
                        bindingText = bind.GetComponent<Text>();
                        bindingText.enabled = true;
                        bindingText.horizontalOverflow = HorizontalWrapMode.Overflow;
                        bindingText.fontSize = 15;
                        mapBindingText[key] = bindingText;

                        var go = Object.Instantiate(bind);
                        go.transform.SetParent(bind.parent.transform, false);

                        var vars = Console_InputText_Patch.vars;

                        var cooldownText = go.GetComponent<Text>();
                        cooldownText.text = "";
                        cooldownText.fontSize = 25;
                        cooldownText.alignByGeometry = false;
                        cooldownText.alignment = TextAnchor.MiddleCenter;
                        cooldownText.color = Color.red;

                        var pos = go.transform.position;
                        pos.x += 25;
                        pos.y += -22;
                        go.transform.position = pos;

                        mapCooldownText[key] = cooldownText;
                    }

                    bindingText.text = GetBindingLabel(i);
                }
            } catch (Exception) {

            }
		}

        public static void CreateHotkeysBar(Hud hud) {
            ClearBindings();
            if (hud == null) hud = Hud.instance;
            var parent = hud.m_rootObject;
            var inventoryGui = InventoryGui.instance;
            hotkeysRect = CreateGameObject(ref hotkeysGrid, inventoryGui, parent, spellsBarHotkeysName, GOTypes.HOTKEYS, barSize);
        }

        public static void CreateInventoryBar(InventoryGui gui) {
            ClearBindings();
            if (gui == null) gui = InventoryGui.instance;
            var parent = gui?.m_player?.gameObject;
            var name = spellsBarGridName;
            invBarRect = CreateGameObject(ref invBarGrid, gui, parent, name, GOTypes.INVENTORY, barSize);
        }

        public enum GOTypes {
            INVENTORY,
            HOTKEYS
		}

        public static RectTransform CreateGameObject(ref InventoryGrid grid, InventoryGui inventoryGui, GameObject parent, string name, GOTypes type, Vector2 size) {
            if (grid != null) {
                Object.Destroy(grid.gameObject);
                grid = null;
            }

            if (type == GOTypes.HOTKEYS) {
                if (!RunicPower.configHotkeysEnabled.Value) return null;
			}

            // go
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);

            // hightlight
            var highlight = new GameObject("SelectedFrame", typeof(RectTransform));

            if (type == GOTypes.INVENTORY) {
                highlight.transform.SetParent(go.transform, false);
                // highlight.AddComponent<Image>().color = Color.yellow;
                var highlightRT = highlight.transform as RectTransform;
                highlightRT.anchoredPosition = new Vector2(0, 0);
                highlightRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x + 2);
                highlightRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y + 2);
                highlightRT.localScale = new Vector3(1, 1, 1);

                // background
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

            if (type == GOTypes.INVENTORY) {
                grid.m_onSelected = OnSelected(inventoryGui);
                grid.m_onRightClick = OnRightClicked(inventoryGui);
            } else {
                grid.m_onSelected = null;
                grid.m_onRightClick = null;
            }

            grid.m_uiGroup = grid.gameObject.AddComponent<UIGroupHandler>();
            grid.m_uiGroup.m_groupPriority = 1;
            grid.m_uiGroup.m_active = true;
            grid.m_uiGroup.m_enableWhenActiveAndGamepad = highlight;

            // list
            var list = inventoryGui.m_uiGroups.ToList();
            list.Insert(2, grid.m_uiGroup);
            inventoryGui.m_uiGroups = list.ToArray();

            // rect
            var goRect = go.transform as RectTransform;


            if (type == GOTypes.HOTKEYS) {
                var position = new Vector2(RunicPower.configHotkeysOffsetX.Value, RunicPower.configHotkeysOffsetY.Value);
                var cfgScale = RunicPower.configHotkeysScale.Value / 100f;
                var scale = new Vector3(cfgScale, cfgScale, cfgScale);
                goRect.localScale = scale;

                goRect.anchorMin = new Vector2(0.5f, 0);
                goRect.anchorMax = new Vector2(0.5f, 0);
                goRect.pivot = new Vector2(0.5f, 0);
                goRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
                goRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);

                position.x -= ((size.x - 107) / 2) * cfgScale;
                goRect.anchoredPosition = position;
            } else { 
                if (RunicPower.configInvBarPosition.Value == RunicPower.InvBarPosition.TOP) {
                    var position = new Vector2(1000, 103);
                    goRect.anchoredPosition = position;
                } else { 
                    var cfgScale = RunicPower.configHotkeysScale.Value / 100f;
                    var scale = new Vector3(cfgScale, cfgScale, cfgScale);
                    goRect.localScale = scale;

                    var vars = Console_InputText_Patch.vars;

                    goRect.anchorMin = new Vector2(0.5f, 0f);
                    goRect.anchorMax = new Vector2(0.5f, 0f);
                    goRect.pivot = new Vector2(0.5f, 0f);

                    var sizex = 643f;
                    var sizey = 88f;
                    var guiscale = GuiScaler.m_largeGuiScale;

                    var x = GuiScaler.m_minWidth/guiscale/2 - sizex/2;
                    var y = (GuiScaler.m_minHeight/guiscale + sizey - 410)*-1;

                    var position = new Vector2(x, y);

                    goRect.anchoredPosition = position;
                } 
            }

            return goRect;
        }

        public static Action<InventoryGrid, ItemDrop.ItemData, Vector2i, InventoryGrid.Modifier> OnSelected(InventoryGui inventoryGui) {
            return (InventoryGrid inventoryGrid, ItemDrop.ItemData item, Vector2i pos, InventoryGrid.Modifier mod) => {
                if (mod == InventoryGrid.Modifier.Move) return;

                var ext = Player.m_localPlayer.ExtendedPlayer();
                var ok = true;
                ext.SetSelectingRuneItem(item);
                if (inventoryGui.m_dragItem != null) {
                    var rune = inventoryGui.m_dragItem?.GetRuneData();
                    if (rune == null) {
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "You can't put a non-rune item on this slot.");
                        ok = false;
                    }
                }
                if (ok) inventoryGui.OnSelectedItem(inventoryGrid, item, pos, mod);
                ext.SetSelectingRuneItem(null);
            };
        }

        public static Action<InventoryGrid, ItemDrop.ItemData, Vector2i> OnRightClicked(InventoryGui inventoryGui) {
            return (InventoryGrid inventoryGrid, ItemDrop.ItemData item, Vector2i pos) => {
                var player = Player.m_localPlayer;
                player?.UseRuneFromSpellBar(item);
            };
        }
	}
}
