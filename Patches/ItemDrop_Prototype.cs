using Common;
using HarmonyLib;
using RunicPower.Core;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RunicPower.Patches {
    public static class ItemDrop_Prototype {

        public static Dictionary<string, ItemDropData_Extended> mapping = new Dictionary<string, ItemDropData_Extended>();

        public static void SetRuneByKey(string key, Rune rune) {
            if (key == null) return;
            var ext = mapping.ContainsKey(key) ? mapping[key] : null;
            if (ext == null) {
                mapping[key] = ext = new ItemDropData_Extended();
                if (!key.Contains("(Clone)")) mapping[key + "(Clone)"] = ext;
            }
            ext.rune = rune;
        }

        public static void SetRune(this ItemDrop itemDrop, Rune rune) {
            var key = itemDrop?.m_itemData.m_shared.m_name;
            SetRuneByKey(key, rune);
        }

        public static Rune GetRuneByKey(string key) {
            var ext = (key != null && mapping.ContainsKey(key)) ? mapping[key] : null;
            return ext?.rune;
        }

        public static Rune GetRune(this ItemDrop.ItemData itemData) {
            var key = itemData?.m_shared?.m_name;
            return GetRuneByKey(key);
        }
    }
}
