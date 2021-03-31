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
using RuneStones.Core;

namespace RunicPower.Patches {
    public static class SEMan_Prototype {

        public static List<Rune> GetRunes(this SEMan __instance) {
            var list = new List<Rune>();

            foreach (var statusEffect in __instance.m_statusEffects) {
                var rune = statusEffect.GetRune();
                if (rune == null) continue;
                list.Add(rune);
            }

            return list;
        }
        public static void AddRunicEffect(this SEMan __instance, string name, Player caster, string dsbuffs, bool resetTime) {
            // if the seman already have this effect
            StatusEffect statusEffect = __instance.GetStatusEffect(name);
            if (statusEffect != null) {
                // we reset its time if needed
                if (resetTime) statusEffect.ResetTime();
                // and be done with it
                return;
            }
            // otherwise let's crete a new effect and add it to the target
            StatusEffect statusEffect2 = RunicPower.CreateStatusEffect(name, caster, dsbuffs);
            if (statusEffect2 != null) __instance.AddStatusEffect(statusEffect2);
        }
    }
}
