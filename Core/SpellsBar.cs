using BepInEx;
using BepInEx.Configuration;
using RunicPower.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

		public static int slotCount = 10;
        public static string barName = "SpellsBarGrid";
        public static readonly Dictionary<int, SpellShortcut> shortcuts = new Dictionary<int, SpellShortcut>();

        public static void RegisterKeybinds(ConfigFile config) {
            for (var i = 0; i < slotCount; i++) {
                var knumber = "Alpha" + (i == slotCount - 1 ? 0 : i + 1);
                var key = (KeyCode)System.Enum.Parse(typeof(KeyCode), knumber.ToString());
                Debug.Log("knumber " + knumber + " key  " + key);
                shortcuts[i] = new SpellShortcut(KeyCode.LeftControl, key);
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
                Debug.Log("USE SPELL #" + index);
                var item = player.GetSpellsBarItem(index);
                if (item != null) player.UseItem(null, item, false);
            }

            /*
                var item = player.GetQuickSlotItem(index);
                if (item != null) {
                    player.UseItem(null, item, false);
                }
            */

                /**
                if (keyCode != null && Input.GetKeyDown(keyCode)) {
                    var item = player.GetQuickSlotItem(index);
                    if (item != null) {
                        player.UseItem(null, item, false);
                    }
                }
                */
            }

    }
}
