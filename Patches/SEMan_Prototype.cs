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
    public static class SEMan_Prototype {

        public static void AddRunicEffect(this SEMan __instance, string name, Player caster, string dsbuffs, bool resetTime) {
            // if the seman already have this effect
            StatusEffect statusEffect = __instance.GetStatusEffect(name);
            if (statusEffect != null) __instance.RemoveStatusEffect(statusEffect, true);
            // otherwise let's crete a new effect and add it to the target
            StatusEffect statusEffect2 = RunicPower.CreateStatusEffect(name, caster, dsbuffs);
            if (statusEffect2 != null) __instance.AddStatusEffect(statusEffect2);
        }
    }
}
