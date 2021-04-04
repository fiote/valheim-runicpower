using System;
using System.Linq;
using HarmonyLib;
using RunicPower;
using RunicPower.Core;
using RunicPower.Patches;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RunicPower.Patches {

    [HarmonyPatch(typeof(InventoryGui), "Awake")]
    public static class InventoryGui_Awake_Patch {
        public static void Postfix(InventoryGui __instance) {
            SpellsBar.CreateInventoryBar(__instance);
            RunicPower.CreateCraftAllButton(__instance);
        }
    }

    [HarmonyPatch(typeof(InventoryGui), "Show")]
    public static class InventoryGui_Show_Patch {
        public static void Prefix(InventoryGui __instance, Container container) {
            if (!__instance.m_animator.GetBool("visible")) {
                SpellsBar.UpdateVisibility();
            }
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

    [HarmonyPatch(typeof(InventoryGui), "DoCrafting")]
    public static class InventoryGui_DoCrafting_Patch {
        public static bool Prefix(InventoryGui __instance, Player player) {
            // if it's not a rune, do the normal flow
            var data = __instance.m_craftRecipe?.m_item?.m_itemData?.GetRuneData();
            if (data == null) {
                RunicPower.StopCraftingAll(false);
                return true;
            }
            // if the player does not have the requeriments, do the normal flow
            var qualityLevel = 1;
            if (!player.HaveRequirements(__instance.m_craftRecipe, discover: false, qualityLevel) && !player.NoCostCheat()) {
                RunicPower.StopCraftingAll(false);
                return true;
            }
            // getting hte spell inventory
            var inv = SpellsBar.invBarGrid.m_inventory;
            // if there is not an 'empty' slot, do the normal flow
            if (!inv.HaveEmptySlot()) {
                RunicPower.StopCraftingAll(false);
                return true;
            }

            var craftItem = __instance.m_craftRecipe.m_item;
            GameObject go = Object.Instantiate(craftItem.gameObject);
            ItemDrop item = go.GetComponent<ItemDrop>();
            item.m_itemData.m_stack = __instance.m_craftRecipe.m_amount;
            item.m_itemData.m_quality = qualityLevel;
            item.m_itemData.m_variant = __instance.m_craftVariant;
            item.m_itemData.m_durability = item.m_itemData.GetMaxDurability();
            item.m_itemData.m_crafterID = player.GetPlayerID();
            item.m_itemData.m_crafterName = player.GetPlayerName();

            var crafted = inv.AddItem(item.m_itemData);

            if (crafted) {
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

    [HarmonyPatch(typeof(InventoryGui), "UpdateRecipe")]
    public static class InventoryGui_UpdateRecipe_Patch {
        public static void Postfix(InventoryGui __instance, Player player, float dt) {
            if (__instance.m_craftTimer == -1f && __instance.m_craftRecipe != null) {
                __instance.m_craftTimer = -0.5f;
                RunicPower.TryCraftingMore();
			}
        }
    }

    [HarmonyPatch(typeof(InventoryGui), "OnCraftCancelPressed")]
    public static class InventoryGui_OnCraftCancelPressed_Patch {
        public static void Prefix(InventoryGui __instance) {
            RunicPower.StopCraftingAll(false);
        }
    }

    [HarmonyPatch(typeof(InventoryGui), "OnCraftPressed")]
    public static class InventoryGui_OnCraftPressed_Patch {
        public static void Postfix(InventoryGui __instance) {
            if (__instance.m_craftRecipe == null) RunicPower.StopCraftingAll(false);
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
            var ext = Player.m_localPlayer?.ExtendedPlayer();

            if (rune == null) {
                ext?.SetCraftingRuneItem(null);
                __instance.m_craftDuration = m_craftDuration_original;
                return;
            }

            ext?.SetCraftingRuneItem(item);
            __instance.m_craftDuration = m_craftDuration_runes;
        }
    }

    [HarmonyPatch(typeof(InventoryGui), "OnSelectedItem")]
    public static class InventoryGui_OnSelectedItem_Patch {
        public static bool Prefix(InventoryGui __instance, InventoryGrid grid, ItemDrop.ItemData item, Vector2i pos, InventoryGrid.Modifier mod) {
            // if we're not moving from the spellbars, do nothing (normal flow)
            if (__instance.m_dragInventory?.m_name != "spellsBarInventory") return true;
            // if we moving TO the spellsbars, do nothing (normal flow)
            if (grid.m_inventory?.m_name == "spellsBarInventory") return true;
            // so if we're moving from the spellsbar to another inventory
            ItemDrop.ItemData itemAt = grid.GetInventory().GetItemAt(pos.x, pos.y);
            if (itemAt != null) {
                // and there is an item at the destination
                var runeData = itemAt.GetRuneData();
                // and its not a rune
                if (runeData == null) {
                    // we cant swap that!
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, "You can't swap a rune for a non-rune item.");
                    return false;
				}
			}
            return true;
        }
    }
}