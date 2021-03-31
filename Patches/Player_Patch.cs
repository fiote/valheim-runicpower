using Common;
using HarmonyLib;
using RunicPower.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RunicPower.Patches {

	[HarmonyPatch(typeof(Player), "Load")]
	public static class Player_Load_Patch {
		public static void Postfix(Player __instance) {
			__instance.ExtendedPlayer().Load();
		}
	}

	[HarmonyPatch(typeof(Player), "Save")]
	public static class Player_Save_Patch {
		public static void Prefix(Player __instance) {
			__instance.ExtendedPlayer().Save();
		}
	}

	[HarmonyPatch(typeof(Player), "Awake")]
	public static class Player_Awake_Patch {
		public static void Postfix(Player __instance) {
			__instance.UpdateSpellBars();
		}
	}

	[HarmonyPatch(typeof(Player), "UseHotbarItem")]
	public static class Player_UseHotbarItem_Patch {
		public static bool Prefix(Player __instance, int index) {
			var item = SpellsBar.GetSpellHotKeyItem(__instance, index-1, true);
			return (item == null);
		}
	}

	[HarmonyPatch(typeof(Player), "UpdateMovementModifier")]
	public static class Player_UpdateMovementModifier_Patch {
		public static void Postfix(Player __instance) {
			var runes = __instance?.m_seman.GetRunes();
			if (runes == null) return;
			foreach (var rune in runes) {
				rune.ModifyEquipmentMovement(ref __instance.m_equipmentMovementModifier);
			}
		}
	}

	[HarmonyPatch(typeof(Player), "ApplyArmorDamageMods")]
	public static class Player_ApplyArmorDamageMods_Patch {
		public static void Postfix(Player __instance, ref HitData.DamageModifiers mods) {
			var damageMods = new List<HitData.DamageModPair>();
			var runes = __instance.m_seman.GetRunes();
			foreach (var rune in runes) {
				var rmods = rune.GetResistanceModifiers();
				foreach (var rmod in rmods) damageMods.Add(rmod);
			}
			mods.Apply(damageMods);
		}
	}

	[HarmonyPatch(typeof(Player), "IsPlayer")]
	public static class Player_IsPlayer_Patch {
		public static bool Prefix(Player __instance, ref bool __result) {
			var ext = __instance.ExtendedCharacter();
			if (ext.isNotAPlayerRightNow) {
				__result = false;
				return false;
			}
			return true;
		}
	}
}
