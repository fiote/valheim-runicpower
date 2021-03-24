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
	public class ExtendedPlayerData {
		public DamageTypeValues powerModifiers = new DamageTypeValues();
	}

	[HarmonyPatch(typeof(Player), "Awake")]
	public static class Player_Awake_Patch {
		public static void Prefix(Player __instance) {
			Debug.Log("============================================");
			Debug.Log("============================================");
		}
	}

	public static class PlayerExtensions {

		public static Dictionary<string, ExtendedPlayerData> mapping = new Dictionary<string, ExtendedPlayerData>();
		public static ExtendedPlayerData GetExtendedData(this Player __instance) {
			var key = __instance.GetInstanceID().ToString();
			if (key == null) return null;
			// getting the current extendedData
			var ext = mapping.ContainsKey(key) ? mapping[key] : null;
			// if it does not exist, create one
			if (ext == null) mapping[key] = ext = new ExtendedPlayerData();
			// and return it
			return ext;
		}
    }
}
