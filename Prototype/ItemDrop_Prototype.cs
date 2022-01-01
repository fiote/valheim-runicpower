using RunicPower.Core;
using System.Collections.Generic;

namespace RunicPower.Patches {
	public static class ItemDrop_Prototype {

		public static Dictionary<string, ItemDropData_Extended> mapping = new Dictionary<string, ItemDropData_Extended>();

		public static void SetRuneByKey(string key, RuneData data) {
			if (key == null) return;
			var ext = mapping.ContainsKey(key) ? mapping[key] : null;
			if (ext == null) {
				mapping[key] = ext = new ItemDropData_Extended();
				if (!key.Contains("(Clone)")) mapping[key + "(Clone)"] = ext;
			}
			ext.rune = data;
		}

		public static void SetRuneData(this ItemDrop itemDrop, RuneData data) {
			var key = itemDrop?.m_itemData.m_shared.m_name;
			SetRuneByKey(key, data);
		}

		public static RuneData GetRuneDataByKey(string key) {
			var ext = (key != null && mapping.ContainsKey(key)) ? mapping[key] : null;
			return ext?.rune;
		}

		public static RuneData GetRuneData(this ItemDrop.ItemData itemData) {
			var key = itemData?.m_shared?.m_name;
			return GetRuneDataByKey(key);
		}
	}
}
