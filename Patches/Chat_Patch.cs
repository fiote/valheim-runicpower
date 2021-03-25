using Common;
using HarmonyLib;
using RuneStones.Patches;
using RunicPower.Core;
using RunicPower.Patches;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace RunicPower {

	// sfx_OpenPortal

	// vfx_corpse_destruction_small (good allround effect)
	// vfx_dragon_death
	// vfx_corpse_destruction_medium
	// vfx_corpse_destruction_large
	// vfx_GodExplosion
	// vfx_HealthUpgrade
	// vfx_StaminaUpgrade
	// vfx_stonegolem_wakeup
	// vfx_WishbonePing
	// sfx_wraith_attack

	// sfx_Potion_frostresist_Start
	// sfx_Potion_health_Start
	// sfx_Potion_stamina_Start


	// vfx_damaged_cart (stealth cast)
	// vfx_crow_death
	// vfx_Damaged_Raft
	// vfx_ghost_death

	// vfx_GoblinShield (shield?)
	// vfx_perfectblock
	// sfx_gdking_stomp
	// sfx_perfectblock

	// vfx_sledge_iron_hit (frost cast)
	// vfx_ColdBall_Hit
	// vfx_ColdBall_launch
	// vfx_dragon_coldbreath 
	// vfx_dragon_ice_hit
	// vfx_frostarrow_hit
	// vfx_ice_destroyed	
	// vfx_iceblocker_destroyed
	// sfx_dragon_coldball_launch
	// sfx_Frost_Start

	// vfx_HearthAddFuel (fire cast)
	// vfx_FireballHit
	// vfx_bonfire_AddFuel 
	// sfx_bowl_AddItem
	// sfx_GoblinShaman_fireball_launch

	// vfx_bowl_AddItem (nice visual for buff)
	// vfx_fermenter_add
	// vfx_firlogdestroyed_half
	// vfx_Potion_stamina_medium
	// vfx_Potion_health_medium
	// vfx_seagull_death
	// sfx_stonegolem_alerted

	// vfx_BloodDeath (blood rune)

	// vfx_barnacle_destroyed -pieces (poisonous shiv)
	// vfx_ProjectileHit
	// vfx_blob_attack (poison?)
	// vfx_greydwarf_shaman_pray
	// vfx_GuckSackDestroyed
	// vfx_poisonarrow_hit
	// sfx_barnacle_destroyed
	// sfx_GuckSackDestroyed

	// vfx_boar_love (charm)
	// vfx_spawn

	// vfx_gdking_stomp (earch/stone?)
	// vfx_lox_groundslam
	// vfx_RockDestroyed_large
	// vfx_stone_floor_destroyed
	// sfx_build_hammer_stone
	// sfx_build_hammer_metal
	// sfx_sledge_hit

	// vfx_sledge_hit (weapon)
	// sfx_battleaxe_swing_wosh
	// sfx_metal_blocked
	// sfx_sledge_swing

	// vfx_serpent_attack_trigger (water)

	// vfx_bush_destroyed (healing / nature?)
	// vfx_firetree_regrow
	// vfx_offering
	// vfx_shrub_2_destroyed
	// vfx_shrub_2_heath_destroyed
	// sfx_greydwarf_shaman_heal



	[HarmonyPatch(typeof(Chat), "InputText")]
	public static class Chat_InputText_Patch {
		static void Postfix(Chat __instance) {
			string text = __instance.m_input.text;

			if (text.StartsWith("x=") || text.StartsWith("y=")) {
				var gridRT = HotkeyBar_Patch.goRect;
				
				var oldposition = gridRT.localPosition;
				var newposition = gridRT.localPosition;

				var parts = text.Split('=');
				int value = int.Parse(parts[1]);
				if (parts[0] == "x") newposition = new Vector2(value, oldposition.y);
				if (parts[0] == "y") newposition = new Vector2(oldposition.x, value);

				Debug.Log("Changing rect position from " + oldposition+ " to " + newposition);
				gridRT.localPosition = newposition;
			}

			
			if (text.Contains("vfx") || text.Contains("sfx")) {
				Debug.Log("=========================================");
				Debug.Log(text);

				var parts = Regex.Matches(text, @"[\""].+?[\""]|[^ ]+").Cast<Match>().Select(m => m.Value).ToList();

				var vfxPrefab = ZNetScene.instance?.GetPrefab(parts[0]);
				if (vfxPrefab != null) {
					vfxPrefab.SetActive(false);

					var caster = Player.m_localPlayer;
					var vfxGo = UnityEngine.Object.Instantiate(vfxPrefab, caster.gameObject.transform.position, vfxPrefab.transform.rotation);


					Component[] components = vfxGo.GetComponentsInChildren<Component>(true);
					foreach (var c in components) {
						var enabled = true;
						foreach (var p in parts) if (p.StartsWith("-") && c.name.Contains(p.Substring(1))) enabled = false;
						c.gameObject.SetActive(enabled);
						// Debug.Log("component["+ (enabled ? "enabled" : "disabled") +"]: " + c.name + " " + c.GetType());
					}

					vfxGo.SetActive(true);
					vfxPrefab.SetActive(true);
				}
			}
		}
	}
}