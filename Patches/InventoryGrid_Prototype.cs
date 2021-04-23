using RunicPower.Core;

namespace RunicPower {
	public static class InventoryGrid_Prototype {
		public static bool IsRunic(this InventoryGrid grid) {
			var runic = false;
			if (grid.name == SpellsBar.spellsBarGridName + "Grid") runic = true;
			if (grid.name == SpellsBar.spellsBarHotkeysName + "Grid") runic = true;
			return runic;
		}
	}
}