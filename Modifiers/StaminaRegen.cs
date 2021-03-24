using HarmonyLib;
using RunicPower.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunicPower.Modifiers {

    //public void ModifyHealthRegen(ref float regenMultiplier)
    [HarmonyPatch(typeof(SEMan), "ModifyStaminaRegen")]
    public static class SEMan_ModifyStaminaRegen_Patch {
        public static void Postfix(SEMan __instance, ref float staminaMultiplier) {
            var runes = __instance.GetRunes();
            var player = __instance.m_character as Player;
            foreach (var rune in runes) rune.ModifyStaminaRegen(player, ref staminaMultiplier);
        }
    }
}