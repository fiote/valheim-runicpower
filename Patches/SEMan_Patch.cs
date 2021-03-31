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

    [HarmonyPatch(typeof(SEMan), "Internal_AddStatusEffect")]
    public static class Character_Internal_AddStatusEffect_Patch {
        static bool Prefix(SEMan __instance, string name, bool resetTime) {
            var parts = name.Split('|');
            if (parts[0] != "RUNICPOWER") return true;

            var effectName = parts[1];
            var effectCaster = Player.GetAllPlayers().Find(p => p.GetZDOID().ToString() == parts[2]);
            var effectBuffs = parts[3];
            __instance.AddRunicEffect(effectName, effectCaster, effectBuffs, resetTime);

            return false;
        }
    }




}
