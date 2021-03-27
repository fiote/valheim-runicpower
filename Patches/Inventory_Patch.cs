using System;
using System.Linq;
using HarmonyLib;
using RunicPower;
using RunicPower.Core;
using RunicPower.Patches;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RuneStones.Patches {


	[HarmonyPatch(typeof(Inventory), "HaveEmptySlot")]
	public static class Inventory_HaveEmptySlot_Patch {
		public static bool Prefix(Inventory __instance, ref bool __result) {
			// if we're not crating a rune, return true so it goes as normal
			var item = InventoryGui_Extended.isCraftingRune;
			if (item == null) return true;
			// getting hte spell inventory
			var inv = SpellsBar.invBarGrid.m_inventory;
			// checking if this item already exists in the inventory (with free stack space)
			ItemDrop.ItemData itemData = inv.FindFreeStackItem(item.m_shared.m_name, item.m_quality);
			// if it does
			if (itemData != null) {
				// get the free space
				var freeStack = itemData.m_shared.m_maxStackSize - itemData.m_stack;
				// if it's enough for this recipe, then we have an 'empty' slot!
				if (freeStack >= item.m_stack) {
					__result = true;
					return false;
				}
			}
			// if the item does not exist, check for a empty slot
			Vector2i invPos = inv.FindEmptySlot(inv.TopFirst(item));
			if (invPos.x >= 0) {
				// if ther is a empty slot
				__result = true;
				return false;
			}
			// otherwise return true and let native code runs
			return true;
		}
	}
	 

	[HarmonyPatch(typeof(Inventory), "AddItem", typeof(ItemDrop.ItemData))] 
    public static class Inventory_AddItem_Patch {
        public static bool Prefix(Inventory __instance, ref ItemDrop.ItemData item, ref bool __result) {
			var rune = item.GetRune();
            if (rune == null) return true;
			var inv = SpellsBar.invBarGrid.m_inventory;
			while (item.m_stack > 0) {
				// checking if this item already exists in the inventory (with free stack space)
				ItemDrop.ItemData itemData = inv.FindFreeStackItem(item.m_shared.m_name, item.m_quality);
				// if it does
				if (itemData != null) {
					// get the free space
					var freeStack = itemData.m_shared.m_maxStackSize - itemData.m_stack;  
					// get how much can we add to it 
					var toAdd = (item.m_stack >= freeStack) ? freeStack : item.m_stack; 
					// add that to it 
					itemData.m_stack += toAdd; 
					// subs that much of the item stack
					item.m_stack -= toAdd;
					// iv was changed
					inv.Changed(); 
				} else {
					// if the item does not exist, check for a empty slot
					Vector2i invPos = inv.FindEmptySlot(inv.TopFirst(item));
					// if there is one
					if (invPos.x >= 0) {
						// add the item to it
						item.m_gridPos = invPos;
						inv.m_inventory.Add(item);
						// iv was changed
						inv.Changed();
						// we're done, stop here
						__result = true;
						return false;
					} else {
						// if there is no empty slot on the spellsbar, let's return true so it goes the normal flow trying to add it to the base inventory
						return true;
					}
				}
			} 
			__result = true;
			return false;
        }
    }
}