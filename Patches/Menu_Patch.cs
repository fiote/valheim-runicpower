using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RunicPower {

	[HarmonyPatch(typeof(GuiScaler), "SetScale")]

	public static class GuiScaler_SetScale_Patch {

		public static float lastScale = -100f;

		public static void Postfix(GuiScaler __instance, float scale) {
			if (scale == lastScale) return;
			lastScale = scale;
			RunicPower.Recreate();
		}
	}
}
