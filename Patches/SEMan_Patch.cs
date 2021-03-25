using Common;
using HarmonyLib;
using RunicPower.Core;
using RunicPower;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RunicPower.Patches {
    public static class SEManExtensions {

        public static List<Rune> GetRunes(this SEMan __instance) {
            var list = new List<Rune>();

            foreach (var statusEffect in __instance.m_statusEffects) {
                var rune = statusEffect.GetRune();
                if (rune == null) continue;
                list.Add(rune);
            }

            return list;
        }
    }

    [HarmonyPatch(typeof(SEMan), "OnDamaged")]
    public static class Character_OnDamaged_Patch {
        static void Prefix(SEMan __instance, ref HitData hit, Character attacker) {
            if (hit == null) return;
            if (attacker == null) return;
            var runes = attacker?.m_seman?.GetRunes();
            if (runes == null) return;
            foreach (var rune in runes) rune.ApplyHealthSteal(hit, attacker);
        }
    }

}
