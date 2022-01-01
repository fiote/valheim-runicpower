using HarmonyLib;
using RunicPower.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RunicPower.Patches {

	[HarmonyPatch(typeof(InventoryGui), "Awake")]
	public static class InventoryGui_Awake_Patch {
		public static void Postfix(InventoryGui __instance) {
			SpellsBar.CreateInventoryBar(__instance);
			RunicPower.CreateRankTabs(__instance);
		}
	}

	[HarmonyPatch(typeof(InventoryGui), "Show")]
	public static class InventoryGui_Show_Patch {
		public static void Prefix(InventoryGui __instance, Container container) {
			if (!__instance.m_animator.GetBool("visible")) {
				SpellsBar.UpdateVisibility();
			}
			SpellsBar.UpdateInventory();
		}
	}

	[HarmonyPatch(typeof(InventoryGui), "Hide")]
	public static class InventoryGui_Hide_Patch {
		public static void Prefix(InventoryGui __instance) {
			if (__instance.m_animator.GetBool("visible")) {
				SpellsBar.UpdateVisibility();
			}
		}
	}


	[HarmonyPatch(typeof(InventoryGui), "OnCraftPressed")]
	public static class InventoryGui_OnCraftPressed_Patch {
		public static bool Prefix(InventoryGui __instance) {
			// getting the selected recipe
			var recipe = __instance.m_selectedRecipe.Key;
			// checking if it'a rune recipe
			var runeData = recipe?.m_item?.m_itemData?.GetRuneData();
			// if it is
			if (runeData != null) {
				// and we're not resting
				if (!RunicPower.IsResting()) return RunicPower.ShowMessage(MsgKey.ONLY_WHEN_RESTING, false);
				// if the rune we're crafting already exists on the spellsbar and its fullstack
				var exists = SpellsBar.FindAnother(runeData, true);
				if (exists != null) return RunicPower.ShowMessage(MsgKey.SAME_RUNE_MULTIPLE, false);
			}
			// otherwise do the normal flow
			return true;
		}
	}

	[HarmonyPatch(typeof(InventoryGui), "DoCrafting")]
	public static class InventoryGui_DoCrafting_Patch {
		public static bool Prefix(InventoryGui __instance, Player player) {
			// if it's not a rune, do the normal flow
			var recipe = __instance.m_craftRecipe;
			var data = recipe?.m_item?.m_itemData?.GetRuneData();
			if (data == null) return true;
			// if the player does not have the requeriments, do the normal flow			
			var qualityLevel = 1;
			if (!player.HaveRequirements(__instance.m_craftRecipe, discover: false, qualityLevel) && !player.NoCostCheat()) return true;

			// getting hte spell inventory
			var inv = SpellsBar.invBarGrid.m_inventory;
			
			// if there is not an 'empty' slot, do the normal flow
			if (!inv.HaveEmptySlot()) return true;

			var craftItem = recipe.m_item;
			var crafted = inv.AddItem(craftItem.gameObject.name, recipe.m_amount, qualityLevel, __instance.m_craftVariant, player.GetPlayerID(), player.GetPlayerName());

			if (crafted != null) {
				if (!player.NoCostCheat()) {
					player.ConsumeResources(__instance.m_craftRecipe.m_resources, qualityLevel);
				}
				__instance.UpdateCraftingPanel();
			}

			// displaying some effects
			CraftingStation currentCraftingStation = Player.m_localPlayer.GetCurrentCraftingStation();
			var effs = (currentCraftingStation != null) ? currentCraftingStation.m_craftItemDoneEffects : __instance.m_craftItemDoneEffects;
			effs.Create(player.transform.position, Quaternion.identity);

			Game.instance.GetPlayerProfile().m_playerStats.m_crafts++;
			Gogan.LogEvent("Game", "Crafted", __instance.m_craftRecipe.m_item.m_itemData.m_shared.m_name, qualityLevel);

			return false;
		}
	}

	[HarmonyPatch(typeof(InventoryGui), "SetRecipe")]
	public static class InventoryGui_SetRecipe_Patch {

		static float m_craftDuration_original = 2f;
		static float m_craftDuration_runes = 0.5f;

		public static void Prefix(InventoryGui __instance, int index, bool center) {
			RunicPower.ClearCache();
		}

		public static void Postfix(InventoryGui __instance, int index, bool center) {
			var item = __instance?.m_selectedRecipe.Key?.m_item?.m_itemData;
			var rune = item?.GetRuneData();
			var ext = Player.m_localPlayer?.ExtendedPlayer(false);

			if (rune == null) {
				ext?.SetCraftingRuneItem(null);
				__instance.m_craftDuration = m_craftDuration_original;
				return;
			}

			ext?.SetCraftingRuneItem(item);
			__instance.m_craftDuration = m_craftDuration_runes;
		}
	}

	[HarmonyPatch(typeof(InventoryGui), "UpdateContainer")]
	public static class InventoryGui_UpdateContainer_Patch {

		static void PrefixUpdateContainer(InventoryGui __instance, Player player) {
			if (!__instance.m_animator.GetBool("visible")) {
				return;
			}
			if (__instance.m_currentContainer != null && __instance.m_currentContainer.IsOwner()) {
				__instance.m_currentContainer.SetInUse(inUse: true);
				__instance.m_container.gameObject.SetActive(true);
				__instance.m_containerGrid.UpdateInventory(__instance.m_currentContainer.GetInventory(), null, __instance.m_dragItem);
				__instance.m_containerName.text = Localization.instance.Localize(__instance.m_currentContainer.GetInventory().GetName());
				if (__instance.m_firstContainerUpdate) {
					__instance.m_containerGrid.ResetView();
					__instance.m_firstContainerUpdate = false;
				}
				if (Vector3.Distance(__instance.m_currentContainer.transform.position, player.transform.position) > __instance.m_autoCloseDistance) {
					__instance.CloseContainer();
				}
			} else {
				__instance.m_container.gameObject.SetActive(false);
				if (__instance.m_dragInventory != null && __instance.m_dragInventory != Player.m_localPlayer.GetInventory() && __instance.m_dragInventory.m_name != RunicPower.invName) {
					__instance.SetupDragItem(null, null, 1);
				}
			}
		}

		public static bool Prefix(InventoryGui __instance, Player player) {
			PrefixUpdateContainer(__instance, player);
			return false;
		}
	}

	[HarmonyPatch(typeof(InventoryGui), "OnSelectedItem")]
	public static class InventoryGui_OnSelectedItem_Patch {
		public static bool Prefix(InventoryGui __instance, InventoryGrid grid, ItemDrop.ItemData item, Vector2i pos, InventoryGrid.Modifier mod) {
			string invDrag = "", invDrop = "";
			ItemDrop.ItemData itemDrag = null, itemDrop = null;
			RuneData runeDrag = null, runeDrop = null;

			if (__instance.m_dragInventory == null) {
				invDrag = grid?.m_inventory?.m_name;
				
				itemDrag = item;
			} else {
				invDrag = __instance.m_dragInventory?.m_name;
				invDrop = grid?.m_inventory?.m_name;

				itemDrag = __instance.m_dragItem;
				itemDrop = grid.m_inventory.GetItemAt(pos.x, pos.y);
			}

			runeDrag = itemDrag?.GetRuneData();
			runeDrop = itemDrop?.GetRuneData();

			var isRuneDrag = runeDrag != null;
			var isRuneDrop = runeDrop != null;

			var spellsbarInvolved = (invDrag == RunicPower.invName || invDrop == RunicPower.invName);
			var spellsbarOnly = (invDrag == RunicPower.invName && invDrop == RunicPower.invName);
			
			// if this has nothing to do with the spellsbar, simply return true
			if (!spellsbarInvolved) return true;

			// if we're not resting, we cant manage the spellsbar
			if (!RunicPower.IsResting()) return RunicPower.ShowMessage(MsgKey.ONLY_WHEN_RESTING, false);

			// if we're still on the drag part, return true
			if (invDrop == "") return true;

			// if we're simply dropping what we dragged, return true
			if (itemDrag == itemDrop) return true;

			// if we're dropping a rune on the spellsbar
			if (invDrop == RunicPower.invName) {
				// check if that rune is already on the spellsbar
				var exists = SpellsBar.FindAnother(runeDrag, false);
				// if there is one
				if (exists != null) {
					// but it's where we're dropping it (stacking up), return true
					if (exists == itemDrop) return true;
					// if we're moving the entire stack, return true
					if (exists == itemDrag && itemDrag.m_stack == __instance.m_dragAmount) return true;
					// otherwise, prevent it
					return RunicPower.ShowMessage(MsgKey.SAME_RUNE_MULTIPLE, false);
				}
			}
			// if this is happening all inside the spellsbar, simply return true
			if (spellsbarOnly) return true;
			// if we're trying to swap a rune for a non-rune, stop here
			if (itemDrop != null && isRuneDrag != isRuneDrop) return RunicPower.ShowMessage(MsgKey.CANT_SWAP_THOSE, false);
			return true;
		}
	}

	[HarmonyPatch(typeof(InventoryGui), "OnTabCraftPressed")]
	public static class InventoryGui_OnTabCraftPressed_Patch {
		public static void Prefix(InventoryGui __instance) {
			if (!RunicPower.configRanksTabEnabled.Value) return;
			RunicPower.onTabPressed(0, false);
		}
	}

	[HarmonyPatch(typeof(InventoryGui), "SetupCrafting")]
	public static class InventoryGui_SetupCrafting_Patch {
		public static void Prefix(InventoryGui __instance) {
			if (!RunicPower.configRanksTabEnabled.Value) return;
			RunicPower.onTabPressed(0, false);
			RunicPower.UpdateVisibilityRankTabs();
		}
	}

	[HarmonyPatch(typeof(InventoryGui), "UpdateRecipeList")]
	public static class InventoryGui_UpdateRecipeList_Patch {
		public static void Prefix(InventoryGui __instance, ref List<Recipe> recipes) {
			if (!RunicPower.configRanksTabEnabled.Value) return;
			var station = Player.m_localPlayer.GetCurrentCraftingStation();

			if (station == null && RunicPower.craftRank > 0) {
				recipes = recipes.FindAll(r => {
					var rune = r.m_item?.m_itemData?.GetRuneData();
					return rune?.rank == RunicPower.craftRank;
				});
			} else {
				recipes = recipes.FindAll(r => {
					var rune = r.m_item?.m_itemData?.GetRuneData();
					return rune == null;
				});
			}
		}
		public static void Postfix(InventoryGui __instance, List<Recipe> recipes) {
			if (!RunicPower.configRanksTabEnabled.Value) return;

			var station = Player.m_localPlayer.GetCurrentCraftingStation();
			if (station == null && RunicPower.craftRank > 0) ((Selectable)__instance.m_tabCraft).interactable = true;
		}
	}

}