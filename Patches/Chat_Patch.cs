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

		public static Dictionary<string, float> vars = new Dictionary<string, float>();

		static void Postfix(Chat __instance) {
			string text = __instance.m_input.text;

			var parts = text.Split('=');
			if (parts.Count() == 2) {
				var key = parts[0];
				var value = float.Parse(parts[1]);
				vars[key] = value;
			}
			
			if (text.Contains("vfx") || text.Contains("sfx")) {
				Debug.Log("=========================================");
				Debug.Log(text);

				var parts2 = Regex.Matches(text, @"[\""].+?[\""]|[^ ]+").Cast<Match>().Select(m => m.Value).ToList();

				var vfxPrefab = ZNetScene.instance?.GetPrefab(parts2[0]);
				if (vfxPrefab != null) {
					vfxPrefab.SetActive(false);

					var caster = Player.m_localPlayer;
					var vfxGo = UnityEngine.Object.Instantiate(vfxPrefab, caster.gameObject.transform.position, vfxPrefab.transform.rotation);

					Component[] components = vfxGo.GetComponentsInChildren<Component>(true);
					foreach (var c in components) {
						var enabled = true;
						foreach (var p in parts2) if (p.StartsWith("-") && c.name.Contains(p.Substring(1))) enabled = false;
						c.gameObject.SetActive(enabled);
					}

					vfxGo.SetActive(true);
					vfxPrefab.SetActive(true);
				}
			}
		}
	}
}