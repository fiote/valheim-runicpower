using RunicPower.Core;
using UnityEngine;

namespace RunicPower.Patches {
	public class Projectile_Extended : MonoBehaviour {

		public Rune rune;

		public void SetRune(Rune rune) {
			this.rune = rune;
		}

	}
}
