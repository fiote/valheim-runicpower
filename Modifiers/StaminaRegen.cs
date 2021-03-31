using HarmonyLib;
using RunicPower.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RunicPower.Modifiers {

    [HarmonyPatch(typeof(SEMan), "ModifyStaminaRegen")]
    public static class SEMan_ModifyStaminaRegen_Patch {
        public static void Postfix(SEMan __instance, ref float staminaMultiplier) {
            var runes = __instance.GetRunes();
            var player = __instance.m_character as Player;
            foreach (var rune in runes) rune.ModifyStaminaRegen(ref staminaMultiplier);
            Debug.Log(__instance.m_nview.name+" staminaMultiplier " + staminaMultiplier);
        }
    }
}