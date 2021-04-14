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
	public class Projectile_Extended : MonoBehaviour {

		public Rune rune;

		public void SetRune(Rune rune) {
			this.rune = rune;
		}

	}
}
