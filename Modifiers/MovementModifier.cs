using HarmonyLib;
using RunicPower.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunicPower.Modifiers {

    //public void ModifyHealthRegen(ref float regenMultiplier)
    [HarmonyPatch(typeof(Player), "UpdateMovementModifier")]
    public static class Player_UpdateMovementModifier_Patch {
        public static void Postfix(Player __instance) {
            var runes = __instance?.m_seman.GetRunes();
            if (runes == null) return;
            foreach (var rune in runes) {
                rune.ModifyEquipmentMovement(__instance, ref __instance.m_equipmentMovementModifier);
            }
        }
    }
}