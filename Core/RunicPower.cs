﻿using BepInEx;
using BepInEx.Configuration;
using Common;
using HarmonyLib;
using LitJson;
using Pipakin.SkillInjectorMod;
using RunicPower.Core;
using RunicPower.Patches;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Description = System.ComponentModel.DescriptionAttribute;
using Object = UnityEngine.Object;

/* [1.x.x]
 * - Fixing bug when picking up runes from the ground.
 * - Adding a "Craft All" button below the "Craft" button.
 * - You should now be able to craft runes if the spellsbar is 'full' but with free stacks.
 * - Fixed bug where crafting could grant extra runes when stacks were at 99.
 * - Adding CONFIG to configure where the inventory spellsbar should appear.
*/


// TODO: check why fireball isnt doing the basic damage. maybe is resist?
// TODO: add cooldown to rune-casting (mainly the spells)
// TODO: check integration with equip wheel.
// TODO: CONFLICT? "crafting with containers" characters run on the spot like gliding over the terrain

// TODO: CONFLICT? check hotkey bar not updating when using runes
// TODO: CONFLICT? check if ghost mode is really broken.
// TODO: CONFLICT? check looted-items message not showing

// MAYBE: change how crafting works. Instead of different items, just use a single 'currency' that would be the result of desenchanting items or something like that.
// MAYBE: change how crafting works. Rune material would increase as the rune get stronger.
// MAYBE: change how casting works. Instead of consuming runes, use of kind of MANA resource.
// MAYBE: ranks for recall rune. Better recalls allow to teleport with better ores.

namespace RunicPower {
	[BepInPlugin("fiote.mods.runicpower", "RunicPower", "1.1")]
	[BepInDependency("com.pipakin.SkillInjectorMod")]
	[BepInDependency("randyknapp.mods.extendeditemdataframework")]

	public class RunicPower : BaseUnityPlugin {
		private Harmony _harmony;
		public static bool debug = false;

		public static RunesConfig runesConfig;
		public static List<Rune> runes = new List<Rune>();
		public static List<RuneData> runesData = new List<RuneData>();
		public static List<ClassSkill> listofCSkills = new List<ClassSkill>();

		public static ConfigFile configFile;

		private void Awake() {
			LoadRunes();
			LoadClasses();
			SetupConfig();
			configFile = Config;
			SpellsBar.RegisterKeybinds();
		}

		private void LoadRunes() {
			runesConfig = PrefabCreator.LoadJsonFile<RunesConfig>("runes.json");
			var assetBundle = PrefabCreator.LoadAssetBundle("runeassets");
			if (runesConfig != null && assetBundle != null) {
				foreach (var data in runesConfig.runes) {
					if (!data.implemented) continue;
					if (assetBundle.Contains(data.recipe.item)) {
						data.prefab = assetBundle.LoadAsset<GameObject>(data.recipe.item);
						runesData.Add(data);
					}
				}
			}
			assetBundle?.Unload(false);
			_harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "fiote.mods.runicpower");
		}

		private void LoadClasses() {
			var classesConfig = PrefabCreator.LoadJsonFile<ClassesConfig>("classes.json");
			foreach (var cskill in classesConfig.classes) {
				if (cskill.implemented) {
					listofCSkills.Add(cskill);
					SkillInjector.RegisterNewSkill(cskill.id, cskill.name, cskill.description, 1.0f, PrefabCreator.LoadCustomTexture(cskill.icon), Skills.SkillType.Unarmed);
				}
			}
		}

		public enum KeyModifiers {
			SHIFT,
			CTRL,
			ALT
		}

		public enum InvBarPosition {
			TOP,
			BOTTOM,
			AUTO
		}

		public enum CastingMessage {
			GLOBAL,
			NORMAL,
			SELF,
			NONE
		}

		public static ConfigEntry<CastingMessage> configCastingMessage;
		public static ConfigEntry<bool> configPvpEnabled;
		public static ConfigEntry<bool> configHotkeysEnabled;
		public static ConfigEntry<int> configHotkeysScale;
		public static ConfigEntry<int> configHotkeysOffsetX;
		public static ConfigEntry<int> configHotkeysOffsetY;
		public static ConfigEntry<KeyModifiers> configHotkeysModifier;
		public static ConfigEntry<InvBarPosition> configInvBarPosition;
		private void SetupConfig() {
			Config.Bind("General", "NexusID", 840, "NexusMods ID for updates.");

			configCastingMessage = Config.Bind("Casting", "Message", CastingMessage.NORMAL, "Define where the casting message should appear.");

			configPvpEnabled = Config.Bind("PVP", "Enabled", true, "If enabled, this will count pvp-flagged players as enemies.");

			configInvBarPosition = Config.Bind("SpellsBar", "Position", InvBarPosition.AUTO, "Defines where the inventory spells' bar should appear.");

			configHotkeysEnabled = Config.Bind("HotkeysBar", "Enabled", true, "Enables the hotkey's bar (the one the bottom of the screen).");
			configHotkeysScale = Config.Bind("HotkeysBar", "Scale", 100, "Adjusts the hotkey's bar size.");
			configHotkeysOffsetX = Config.Bind("HotkeysBar", "OffsetX", 0, "Adjust the hotkey's bar horizontal position (left/right).");
			configHotkeysOffsetY = Config.Bind("HotkeysBar", "OffsetY", 0, "Adjust the hotkey's bar vertical position (down/up).");
			configHotkeysModifier = Config.Bind("HotkeysBar", "Modifier", KeyModifiers.SHIFT, "Key modifier to use the runes.");
		}

		private void OnDestroy() {
			_harmony?.UnpatchAll();
			foreach (var rune in runesData) Destroy(rune.prefab);
			runesData.Clear();
		}

		public static void TryRegisterPrefabs(ZNetScene zNetScene) {
			if (zNetScene == null) return;
			foreach (var rune in runesData) zNetScene.m_prefabs.Add(rune.prefab);
		}

		public static void TryRegisterItems() {
			foreach (var data in runesData) {
				data.itemDrop = data.prefab.GetComponent<ItemDrop>();
				if (data.itemDrop == null) {
					Log("Failed to register item " + data.name + ". ItemDrop not found.");
					continue;
				}
				if (ObjectDB.instance.GetItemPrefab(data.prefab.name.GetStableHashCode()) != null) {
					Log("Failed to register item " + data.name + ". Prefab already exists.");
					continue;
				}

				var itemDrop = data.itemDrop;
				itemDrop.SetRuneData(data);
				ObjectDB.instance.m_items.Add(data.prefab);
			}
		}

		static bool tryAgain = false;
		static float tryAgainTime = 0f;
		static float tryAgainDuration = 0.25f;

		public static void TryRegisterRecipes() {
			if (ObjectDB.instance == null) return;

			var wrongTime = (ObjectDB.instance?.m_items?.Count == 0);

			if (!wrongTime) Log("TryRegisterRecipes (" + ObjectDB.instance?.m_items?.Count+" items in the database).");

			var resources = new List<string>();
			foreach (var data in runesData) {
				foreach (var req in data.recipe.resources) {
					if (!resources.Contains(req.item)) resources.Add(req.item);
				}
			}

			var missing = new List<string>();

			foreach (var item in resources) {
				var pref = ObjectDB.instance.GetItemPrefab(item);
				if (pref == null) missing.Add(item);
			}

			if (missing.Count > 0) {
				if (!wrongTime) Log("Some requeriments are not ready yet ("+string.Join(", ",missing)+"). Let's try again in few miliseconds...");
				tryAgain = true;
				tryAgainTime = 0f;
				return;
			} else {
				Log("All requeriments are ready!");
			}

			TryRegisterItems();

			PrefabCreator.Reset();

			foreach (var data in runesData) {
				if (data.recipe.amount == 0) data.recipe.amount = runesConfig.defRecipes.amount;
				if (data.recipe.craftingStation == "") data.recipe.craftingStation = runesConfig.defRecipes.craftingStation;
				if (data.recipe.minStationLevel == 0) data.recipe.minStationLevel = runesConfig.defRecipes.minStationLevel;
				if (data.recipe.repairStation == "") data.recipe.repairStation = runesConfig.defRecipes.repairStation;

				data.recipe.enabled = true;
				data.itemDrop.m_itemData.m_shared.m_name = data.name;
				data.itemDrop.m_itemData.m_shared.m_description = data.description;
				data.itemDrop.m_itemData.m_shared.m_maxStackSize = 100;
				data.itemDrop.m_itemData.m_shared.m_weight = 0.1f;

				PrefabCreator.AddNewRuneRecipe(data);
				var rune = new Rune(data, null);
				runes.Add(rune);
			}
		}
		public static Rune GetStaticRune(RuneData data) {
			var rune = runes.Find(r => r.data.name == data.name);
			rune.SetCaster(Player.m_localPlayer);
			return rune;
		}

		public static void ClearCache() {
			runes.ForEach(rune => rune.ClearCache());
		}

		public static StatusEffect CreateStatusEffect(string name, Player caster, string dsbuffs) {
			var data = runesData.Find(r => r.recipe.item == name);
			if (data == null) return null;

			var rune = new Rune(data, caster);
			rune.ParseBuffs(dsbuffs);
			rune.CreateEffect();

			return rune.statusEffect;
		}

		private void Update() {
			if (tryAgain) {
				tryAgainTime += Time.deltaTime;
				if (tryAgainTime >= tryAgainDuration) {
					tryAgain = false;
					TryRegisterRecipes();
				}
			}
			if (Player.m_localPlayer?.TakeInput() == true) SpellsBar.CheckInputs();
			// SpellsBar.UpdateVisibility();
		}

		public static void Log(string message) {
			UnityEngine.Debug.Log("[RunicPower] "+message);
		}

		public static void Debug(string message) {
			if (debug) Log(message);
		}

		public static GameObject craftAllgo;
		public static Button craftAllButton;
		public static Text craftAllText;
		public static bool isCraftingAll = false;

		public static void CreateCraftAllButton(InventoryGui gui) {
			if (gui == null) gui = InventoryGui.instance;
			var craftButton = gui.m_craftButton.gameObject;
			var name = "runicPowerCraftAllButton";

			if (craftAllgo != null) {
				Destroy(craftAllgo);
			}

			// var comps2 = craftButton.GetComponentsInChildren(typeof(Component));
			// foreach (var comp in comps2) RunicPower.Debug("children.comp -> " + comp);

			craftAllgo = Instantiate(craftButton);
			craftAllgo.transform.SetParent(craftButton.transform.parent, false);
			craftAllgo.name = name;

			var vars = Console_InputText_Patch.vars;

			var position = craftAllgo.transform.position;
			position.x += 0f;
			position.y += -60f;
			craftAllgo.transform.position = position;

			var rect = craftAllgo.GetComponent<RectTransform>();
			var size = rect.sizeDelta;
			size.x += -150;
			size.y += -10;
			rect.sizeDelta = size;

			craftAllButton = craftAllgo.GetComponentInChildren<Button>();
			craftAllButton.interactable = true;
			craftAllButton.onClick.AddListener(OnClickCraftButton);

			craftAllText = craftAllgo.GetComponentInChildren<Text>();
			craftAllText.text = "Craft All";
			craftAllText.resizeTextForBestFit = false;
			craftAllText.fontSize = 20;

			craftAllgo.GetComponent<UITooltip>().m_text = "";
		}

		public static void OnClickCraftButton() {
			RunicPower.Debug("OnClickCraftButton");
			if (isCraftingAll) {
				StopCraftingAll(true);
			} else {
				StartCraftingAll();
			}
		}

		public static void StartCraftingAll() {
			isCraftingAll = true;
			craftAllText.text = "Stop Crafting";
			InventoryGui.instance.OnCraftPressed();
		}

		public static void StopCraftingAll(bool triggerCancel) {
			isCraftingAll = false;
			craftAllText.text = "Craft All";
			if (triggerCancel) InventoryGui.instance.OnCraftCancelPressed();
		}

		public static void TryCraftingMore() {
			RunicPower.Debug("TryCraftingMore");
			if (!isCraftingAll) return;
			InventoryGui.instance.OnCraftPressed();
		}
	}
}