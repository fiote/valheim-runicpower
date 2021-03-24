using Common;
using HarmonyLib;
using RunicPower.Core;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RunicPower.Patches {
	public class ExtendedItemDropData {
		public Rune rune;
	}

    [HarmonyPatch(typeof(ItemDrop.ItemData), "GetTooltip", typeof(ItemDrop.ItemData), typeof(int), typeof(bool))]
    public static class ItemData_GetTooltip_Patch {

        private static bool Prefix(ref string __result, ItemDrop.ItemData item, int qualityLevel, bool crafting) {
            var rune = item.GetRune();
            if (rune == null) return true;
            __result = rune.GetTooltip(item);
            return false;
        }
    }

    public static class ItemDropExtensions {

        public static Dictionary<string, ExtendedItemDropData> mapping = new Dictionary<string, ExtendedItemDropData>();

        public static void SetRuneByKey(string key, Rune rune) {
            if (key == null) return;
            // getting the current extendedData
            var ext = mapping.ContainsKey(key) ? mapping[key] : null;
            // if it does not exist
            if (ext == null) {
                // create a new one
                mapping[key] = ext = new ExtendedItemDropData();
                // and store a 'clone' version if needed
                if (!key.Contains("(Clone)")) mapping[key + "(Clone)"] = ext;
            }
            // then we set the rune
            ext.rune = rune;
        }

        public static void SetRune(this ItemDrop itemDrop, Rune rune) {
            var key = itemDrop?.m_itemData.m_shared.m_name;
            SetRuneByKey(key, rune);
        }

        public static void SetRune(this ItemDrop.ItemData itemData, Rune rune) {
            var key = itemData?.m_shared?.m_name;
            SetRuneByKey(itemData?.m_dropPrefab?.name, rune);
        }

        public static Rune GetRuneByKey(string key) {
            var ext = (key != null && mapping.ContainsKey(key)) ? mapping[key] : null;
            return ext?.rune;
        }

        public static Rune GetRune(this ItemDrop itemDrop) {
            var key = itemDrop?.m_itemData.m_shared.m_name;
            return GetRuneByKey(key);
        }

        public static Rune GetRune(this ItemDrop.ItemData itemData) {
            var key = itemData?.m_shared?.m_name;
            return GetRuneByKey(key);
        }
    }
}
