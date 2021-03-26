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

	public static class Projectile_Prototype {

		public static Dictionary<string, Projectile_Extended> mapping = new Dictionary<string, Projectile_Extended>();
		public static Projectile_Extended GetExtendedData(this Projectile self) {
			var key = self.GetInstanceID().ToString();
			if (key == null) return null;
			var ext = mapping.ContainsKey(key) ? mapping[key] : null;
			if (ext == null) mapping[key] = ext = new Projectile_Extended();
			return ext;
		}

		public static Rune GetRune(this Projectile self) {
			return self.GetExtendedData().rune;
		}

		public static void SetRune(this Projectile self, Rune rune) {
			self.GetExtendedData().rune = rune;
		}
	}
}
