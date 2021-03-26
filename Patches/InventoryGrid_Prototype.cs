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
	public static class InventoryGrid_Prototype {
        public static bool IsRunic(this InventoryGrid grid) {
            var runic = false;
            if (grid.name == SpellsBar.spellsBarGridName + "Grid") runic = true;
            if (grid.name == SpellsBar.spellsBarHotkeysName + "Grid") runic = true;
            return runic;
        }
    }
}