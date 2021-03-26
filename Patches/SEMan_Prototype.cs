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

}
