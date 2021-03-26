using Common;
using HarmonyLib;
using RuneStones.Patches;
using RunicPower.Core;
using RunicPower.Patches;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace RunicPower {

	[HarmonyPatch(typeof(Chat), "InputText")]
	public static class Chat_InputText_Patch {
		static void Postfix(Chat __instance) {
			string text = __instance.m_input.text;

			if (text.StartsWith("x=") || text.StartsWith("y=")) {

				/*
				var gridRT = HotkeyBar_Patch.goRect;				
				var oldposition = gridRT.localPosition;
				var newposition = gridRT.localPosition;

				var parts = text.Split('=');
				int value = int.Parse(parts[1]);
				if (parts[0] == "x") newposition = new Vector2(value, oldposition.y);
				if (parts[0] == "y") newposition = new Vector2(oldposition.x, value);

				Debug.Log("Changing rect position from " + oldposition+ " to " + newposition);
				gridRT.localPosition = newposition;
				*/
			}
			
			if (text.Contains("vfx") || text.Contains("sfx")) {
				Debug.Log("=========================================");
				Debug.Log(text);

				var parts = Regex.Matches(text, @"[\""].+?[\""]|[^ ]+").Cast<Match>().Select(m => m.Value).ToList();

				var vfxPrefab = ZNetScene.instance?.GetPrefab(parts[0]);
				if (vfxPrefab != null) {
					vfxPrefab.SetActive(false);

					var caster = Player.m_localPlayer;
					var vfxGo = UnityEngine.Object.Instantiate(vfxPrefab, caster.gameObject.transform.position, vfxPrefab.transform.rotation);

					Component[] components = vfxGo.GetComponentsInChildren<Component>(true);
					foreach (var c in components) {
						var enabled = true;
						foreach (var p in parts) if (p.StartsWith("-") && c.name.Contains(p.Substring(1))) enabled = false;
						c.gameObject.SetActive(enabled);
					}

					vfxGo.SetActive(true);
					vfxPrefab.SetActive(true);
				}
			}
		}
	}
}