using HarmonyLib;
using RunicPower.Core;
using RunicPower.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RunicPower.Patches {

    [HarmonyPatch(typeof(HotkeyBar), "UpdateIcons")]
    public static class HotkeyBar_UpdateIcons_Patch {
        public static void Postfix(HotkeyBar __instance, Player player) {
			RunicPower.Debug("HotkeyBar_UpdateIcons_Patch Postfix");
            player?.UpdateSpellBars();
        }
    }
}