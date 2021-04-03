using Common;
using HarmonyLib;
using RunicPower.Core;
using RunicPower.Patches;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace RunicPower {

	[HarmonyPatch(typeof(Console), "InputText")]
	public static class Console_InputText_Patch {

		static bool Prefix(Console __instance) {
			RunicPower.Debug("Console_InputText_Patch Prefix");
			string text = __instance.m_input.text;
			var parts = text.Split('=');

			if (parts.Length == 2) {
				var key = parts[0];

				var keyparts = key.Split('.');
				if (keyparts[0] != "rp") return true;

				var value = parts[1];
				var lower = value.ToLower();
				var floated = float.TryParse(parts[1], out float floatvalue);

				var cmd = keyparts[1];
				var intvalue = floated ? Mathf.RoundToInt(floatvalue) : 0;
				var boolvalue = (value == "1" || value == "yes" || value == "on");

				if (cmd == "hotkey") {
					RunicPower.configHotkeysEnabled.Value = boolvalue;
					RunicPower.Log("HOTKEY.ENABLED config changed to "+boolvalue);
				}
				if (cmd == "scale") {
					RunicPower.configHotkeysScale.Value = intvalue;
					RunicPower.Log("HOTKEY.SCALE config changed to " + intvalue);
				}
				if (cmd == "x") {
					RunicPower.configHotkeysOffsetX.Value = intvalue;
					RunicPower.Log("HOTKEY.OFFSETX config changed to " + intvalue);
				}
				if (cmd == "y") {
					RunicPower.configHotkeysOffsetY.Value = intvalue;
					RunicPower.Log("HOTKEY.OFFSETY config changed to " + intvalue);
				}
				if (cmd == "pvp") {
					RunicPower.configPvpEnabled.Value = boolvalue;
					RunicPower.Log("PVP.ENABLED config changed to " + boolvalue);
				}

				if (cmd == "debug") {
					RunicPower.debug = boolvalue;
					RunicPower.Log("DEBUG config changed to " + boolvalue);
				}

				if (cmd == "message") {
					RunicPower.CastingMessage message;
					if (lower == "global") message = RunicPower.CastingMessage.GLOBAL;
					else if (lower == "none") message = RunicPower.CastingMessage.NONE;
					else if (lower == "normal") message = RunicPower.CastingMessage.NORMAL;
					else if (lower == "self") message = RunicPower.CastingMessage.SELF;
					else {
						RunicPower.Log("CASTING.MESSAGE failed to change. Acceptable values are: global, none, normal, self");
						return true;
					}
					RunicPower.configCastingMessage.Value = message;
					RunicPower.Log("CASTING.MESSAGE config changed to " + message);
				}

				if (cmd == "modifier") {
					RunicPower.KeyModifiers mod;
					if (lower == "shift") mod = RunicPower.KeyModifiers.SHIFT;
					else if (lower == "control" || lower == "ctrl" || lower == "ctr") mod = RunicPower.KeyModifiers.CTRL;
					else if (lower == "alt") mod = RunicPower.KeyModifiers.ALT;
					else {
						RunicPower.Log("HOTKEY.MODIFIER failed to change. Acceptable values are: shift, control, alt");
						return true;
					}
					RunicPower.configHotkeysModifier.Value = mod;
					RunicPower.Log("HOTKEY.MODIFIER config changed to " + mod);
				}

				RunicPower.configFile.Save();
				SpellsBar.RegisterKeybinds();
				SpellsBar.CreateHotkeysBar(null);
				return false;
			}

			return true;
		}
	}
}