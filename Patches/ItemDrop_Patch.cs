using HarmonyLib;

namespace RunicPower.Patches {

	[HarmonyPatch(typeof(ItemDrop.ItemData), "GetTooltip", typeof(ItemDrop.ItemData), typeof(int), typeof(bool))]
	public static class ItemData_GetTooltip_Patch {
		private static bool Prefix(ref string __result, ItemDrop.ItemData item, int qualityLevel, bool crafting) {
			var data = item.GetRuneData();
			if (data == null) return true;

			var rune = RunicPower.GetStaticRune(data);
			__result = rune.GetTooltip(item);

			return false;
		}
	}
}
