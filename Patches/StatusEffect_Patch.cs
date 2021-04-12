using Common;
using HarmonyLib;
using RunicPower.Core;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RunicPower.Patches {

    [HarmonyPatch(typeof(StatusEffect), "GetTooltipString")]
    public static class ItemData_GetTooltipString_Patch {
        private static bool Prefix(StatusEffect __instance, ref string __result) {
            var rune = __instance.GetRune();
            if (rune == null) return true;
            __result = rune.GetEffectTooltip();
            return false;
        }
    }

    [HarmonyPatch(typeof(StatusEffect), "ModifyStaminaRegen")]
    public static class StatusEffect_ModifyStaminaRegen_Patch {
        public static bool Prefix(StatusEffect __instance, ref float staminaRegen) {
            var rune = __instance.GetRune();
            if (rune == null) return true;
            rune.ModifyStaminaRegen(ref staminaRegen);
            return false;
        }
    }

    [HarmonyPatch(typeof(StatusEffect), "ModifyHealthRegen")]
    public static class StatusEffect_ModifyHealthRegen_Patch {
        public static bool Prefix(StatusEffect __instance, ref float regenMultiplier) {
            var rune = __instance.GetRune();
            if (rune == null) return true;
            rune.ModifyHealthRegen(ref regenMultiplier);
            return false;
        }
    }

    [HarmonyPatch(typeof(StatusEffect), "RemoveStartEffects")]
    public static class StatusEffect_RemoveStartEffects_Patch {
        public static void Prefix(StatusEffect __instance) {
            var rune = __instance.GetRune();
            __instance.m_character?.ExtendedCharacter()?.RemoveRune(rune);
        }
    }
}
