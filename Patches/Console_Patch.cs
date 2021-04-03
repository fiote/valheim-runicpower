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

		public class ConsoleValue {
			public string value;
			public float floatvalue;
			public int intvalue;
			public bool boolvalue;

			public ConsoleValue(string value) {
				this.value = value.ToLower(); 
				var floated = float.TryParse(value, out floatvalue);
				intvalue = floated ? Mathf.RoundToInt(floatvalue) : 0;
				boolvalue = (value == "1" || value == "yes" || value == "on");
			}
		}

		public static Dictionary<string, ConsoleValue> vars = new Dictionary<string, ConsoleValue>() {
			{  "x" , new ConsoleValue("0") },
			{  "y" , new ConsoleValue("-60") },
			{  "w" , new ConsoleValue("-150") },
			{  "h" , new ConsoleValue("-10") },
			{  "s" , new ConsoleValue("20") }
		};

		static bool Prefix(Console __instance) {
			RunicPower.Debug("Console_InputText_Patch Prefix");
			string text = __instance.m_input.text;
			var parts = text.Split('=');

			if (parts.Length == 2) {
				var key = parts[0];
				
				var value = parts[1];
				var cvalue = new ConsoleValue(value);


				var keyparts = key.Split('.');

				if (keyparts[0] != "rp") {
					vars[key] = cvalue;
					RunicPower.CreateCraftAllButton(null);
					return true;
				}
				
				var cmd = keyparts[1];

				if (cmd == "hotkey") {
					RunicPower.configHotkeysEnabled.Value = cvalue.boolvalue;
					RunicPower.Log("HOTKEY.ENABLED config changed to "+ cvalue.boolvalue);
				}
				if (cmd == "scale") {
					RunicPower.configHotkeysScale.Value = cvalue.intvalue;
					RunicPower.Log("HOTKEY.SCALE config changed to " + cvalue.intvalue);
				}
				if (cmd == "x") {
					RunicPower.configHotkeysOffsetX.Value = cvalue.intvalue;
					RunicPower.Log("HOTKEY.OFFSETX config changed to " + cvalue.intvalue);
				}
				if (cmd == "y") {
					RunicPower.configHotkeysOffsetY.Value = cvalue.intvalue;
					RunicPower.Log("HOTKEY.OFFSETY config changed to " + cvalue.intvalue);
				}
				if (cmd == "pvp") {
					RunicPower.configPvpEnabled.Value = cvalue.boolvalue;
					RunicPower.Log("PVP.ENABLED config changed to " + cvalue.boolvalue);
				}

				if (cmd == "debug") {
					RunicPower.debug = cvalue.boolvalue;
					RunicPower.Log("DEBUG config changed to " + cvalue.boolvalue);
				}

				if (cmd == "message") {
					RunicPower.CastingMessage message;
					if (cvalue.value == "global") message = RunicPower.CastingMessage.GLOBAL;
					else if (cvalue.value == "none") message = RunicPower.CastingMessage.NONE;
					else if (cvalue.value == "normal") message = RunicPower.CastingMessage.NORMAL;
					else if (cvalue.value == "self") message = RunicPower.CastingMessage.SELF;
					else {
						RunicPower.Log("CASTING.MESSAGE failed to change. Acceptable values are: global, none, normal, self");
						return true;
					}
					RunicPower.configCastingMessage.Value = message;
					RunicPower.Log("CASTING.MESSAGE config changed to " + message);
				}

				if (cmd == "modifier") {
					RunicPower.KeyModifiers mod;
					if (cvalue.value == "shift") mod = RunicPower.KeyModifiers.SHIFT;
					else if (cvalue.value == "control" || cvalue.value == "ctrl" || cvalue.value == "ctr") mod = RunicPower.KeyModifiers.CTRL;
					else if (cvalue.value == "alt") mod = RunicPower.KeyModifiers.ALT;
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