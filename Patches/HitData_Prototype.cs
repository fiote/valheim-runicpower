using HarmonyLib;
using RunicPower.Core;
using RunicPower.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RunicPower {
	public static class HitData_Prototype {
		public static string GetTotals(this HitData __instance) {
			return __instance.GetTotalDamage() + " (P: " + __instance.GetTotalPhysicalDamage() + ", E: " + __instance.GetTotalElementalDamage() + ")";
		}
	}
}