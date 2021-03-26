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

    [HarmonyPatch(typeof(SEMan), "OnDamaged")]
    public static class Character_OnDamaged_Patch {
        static void Prefix(SEMan __instance, ref HitData hit, Character attacker) {
            if (hit == null) return;
            if (attacker == null) return;
            var runes = attacker.GetRunes();
            foreach (var rune in runes) rune.ApplyHealthSteal(hit, attacker);
        }
    }

}
