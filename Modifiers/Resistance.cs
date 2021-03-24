using System.Collections.Generic;
using HarmonyLib;
using RunicPower.Patches;
using UnityEngine;

namespace RunicPower.Modifiers{
    

    [HarmonyPatch(typeof(Player), "ApplyArmorDamageMods")]
    public static class Player_ApplyArmorDamageMods_Patch {
        public static void Postfix(Player __instance, ref HitData.DamageModifiers mods) {
            var damageMods = new List<HitData.DamageModPair>();
            var runes = __instance.m_seman.GetRunes();
            foreach (var rune in runes) {
                var rmods = rune.GetResistanceModifiers();
                foreach (var rmod in rmods) damageMods.Add(rmod);
            }
            mods.Apply(damageMods);
        }
    }
}