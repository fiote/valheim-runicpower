using BepInEx;
using BepInEx.Configuration;
using Common;
using HarmonyLib;
using Pipakin.SkillInjectorMod;
using RunicPower.Core;
using RunicPower.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

/* [1.2]
 * - Fixed bug when picking up runes from the ground.
 * - Added "Craft All" button below the "Craft" button. When you click this, it'll start crafting the rune while there are materials available. Click again to stop.
 * - Added CONFIG to remove the "Craft All" button in case it's messing with your UI/other mods.
 * - You should now be able to craft runes if the spellsbar is 'full' but with free stacks.
 * - You should now be able to cast runes using the right-modifier as well (right-alt, right-ctrl, right-shift).
 * - Fixed bug where crafting could grant extra runes when stacks were at 99.
 * - Added CONFIG to configure where the inventory spellsbar should appear.
 * - Fixed elemental damage not being applied right away with runes.
 * - Most runes now have cooldowns. The cooldown reduces a bit as you level the class.
 * -> Offensive Spells: 15 seconds (Bladestorm, Fireball, etc).
 * -> Recovery Spells: 30 seconds (Inspiring Shout, Healing Circle).
 * -> Battle (De)Buffs: 120 seconds (Expose Weakness, Stone Rune, Mind Rune, etc).
 * -> Normal Buffs: 5 seconds (Swift Rune, Light Rune, etc) (just to avoid casting it twice my mistake).
 * -> Recall: 300 seconds. *
 * - Added CONFIG to disable cooldowns if you don't like them (I won't judge!).
*/

/* [1.2.2]
 * - Removing use of AUTO for SpellsBar.position (not using that)
*/

/* [1.2.3]
 * - Inventory Spellsbar position now defaulted to bottom.
 * - Improving visibility updates so the hotkey's bar won't appear alongside the building panel.
 * - Runic buffs should now correctly be removed when the rune expires.
 * - Making sure the crafting works even if the hotkey's bar is disabled.
 * - Fixing error related to hotkeys-bar disabled.
 * - Fixing errors related to craft-all disabled.
 */

/* [1.3]
 * - Changed how extended data is stored to avoid using unnecessary memory.
 * - Added CONFIG "class control" to set the max level of alt class skills.
*/

/* [1.4]
 * - Implementing "Ranks" for spells.
 * - Adding rank tabs.
 * - Fixing tooltip position when it's on the lower part of the screen.
 * - Adding CONGIFs for ranktab, rankx, ranky.
 * - Recreating the mod's game objects when the game's scale changes.
*/

/* [1.4.1]
 * - Disabling debug mode (lol sorry).
*/

/* [1.4.2]
 * - Fixing CreateRankTab exception when relogging ingame.
 * - Fixing bug that would allow players to stack multiple buffs for ranks of the same rune.
 * - Changed how extended data is stored to avoid using unnecessary memory.
 * - Fixing a very weird bug that was spawning runes somewhere in your world whenever you crafted a rune.
 * - Fixing the bug that would duplicate runes when you dropped/picked them up.
 * - Adding some debug.logging so you can know exactly what rune effects are on your character (good for testing stuff, if you're into that kind of thing).
*/

/* [2.0.0]
 * - Removing "Craft All" button below "Craft" button (it's now its own mod).
 * - Reduced class runes stack size to 10 (was 100).
 * - Reduced recall runes stack size to 1 (was 100).
 * - Runes now have "RPG" icons to make it easier to identify them.
 * - Higher rank runes will now grant a lot of experience to help you 'catch up' with its level.
 * - You can now only craft runes while resting.
 * - You can now only manage your spellsbar while resting.
 * - You can now only cast runes from your spellsbar.
 * - You can no longer have duplicated rune stacks on your spellsbar.
 *
 * [Generic]
 * - "Recall" rune renamed to "Recall (Meadows)". It now the same restrictions of a portal (i.e: no ore/metals allowed).
 * - New rune: "Recall (Black Forest)". Allows you to teleport even with tin/copper/bronze in your inventory.
 * - New rune: "Recall (Swamp)". Allows you to teleport even with iron in your inventory.
 * - New rune: "Recall (Mountains)". Allows you to teleport even with silver in your inventory.
 * - New rune: "Recall (Plains)". Allows you to teleport even with black metal in your inventory.
 * - New rune: "Enchant Axe". Increase your chopping damage.
 * - New rune: "Enchant Pickaxe". Increase your mining damage.
 *
 * [Warrior]
 * - "Blade Storm" now requires a weapon. Its damage is now a percentage of your weapon (from 150% to 300%) instead of a flat amount.
 * - "Inspiring Shout" now recovers a percentage of your max stamina instead of a flat amount.
 * - "Blood Rune" and "Stone Rune" buffs tunned down a bit.
 *
 * [Rogue]
 * - "Poisonous Shiv" now requires a weapon. Its damage is now a percentage of your weapon (from 150% to 200%) instead of a flat amount.
 * - "Expose Weakness" should now works alongside all attacks, not only those from runes.
 * - "Night Rune" now increases your stealthness (just like the Troll gear set) instead of trying to mess with other stuff.
 * - "Swift Rune" no longer removes fall damage (it was too OP while also dunking some player's FPS).
 *
 * [Cleric]
 * - "Shield Slam" now requires a shield. Its damage is now a percentage of your block power (from 150% to 300%) instead of a flat amount.
 * - "Healing Circle" now recovers a percentage of your max health instead of a flat amount.
 * - "Light Rune" and "Vigor Rune" buffs tunned down a bit.
 *
 * [Wizard]
 * - "Fireball" damage no longer scales with weapon, depending only of your wizard level.
 * - "Ice Shard" damage no longer scales with weapon, depending only of your wizard level.
 * - "Ice Shard" slow effect now scales with your wizard level, from 10% (1) to 80% (100).
 * - "Ward Rune" buff tunned down a bit, but should work more properly.
 * - "Mind Rune" buff tunned up. It now should works alongside all attacks, not only those from runes.
*/


/* [2.0.1]
 * Removing forced min level and reanabling burn effect.
 * Trying to make it work better with craft all mod.
 * Fixing "v2" on poisonous shiv data (it's v100).
 */

// TODO: make cooldowns appear on the inventory itself.
// TODO: INTEGRATION? equip wheel considering runes as consumables (which they are)
// MAYBE: change how crafting works. Instead of different items, just use a single 'currency' that would be the result of desenchanting items or something like that.
// MAYBE: change how casting works. Instead of consuming runes, use of kind of MANA resource.

namespace RunicPower {
	[BepInPlugin("fiote.mods.runicpower", "RunicPower", "2.0.1")]
	[BepInDependency("com.pipakin.SkillInjectorMod")]
	[BepInDependency("randyknapp.mods.extendeditemdataframework")]

	public class RunicPower : BaseUnityPlugin {
		// core stuff
		private Harmony _harmony;
		public static bool debug = false;
		public static ConfigFile configFile;
		public static List<Rune> runes = new List<Rune>();
		public static List<RuneData> runesData = new List<RuneData>();
		public static List<ClassSkill> listofCSkills = new List<ClassSkill>();

		public static RunesConfig runesConfig;
		public static AssetBundle assetBundle;

		// mod stuff
		public static Dictionary<int, Button> rankButtons;
		public static Dictionary<string, int> activeCooldowns;
		static bool tryAgain;
		static float tryAgainTime;
		static float tryAgainDuration;

		public static string invName = "spellsBarInventory";

		public static int craftRank;
		public static RunicPower _this;

		float tickCooldown;

		public static Dictionary<int, string> rank2rank = new Dictionary<int, string> {
			{1, "I"},
			{2, "II"},
			{3, "III"},
			{4, "IV"},
			{5, "V"},
		};

		public static Dictionary<MsgKey, string> texts = new Dictionary<MsgKey, string> {
			{ MsgKey.ONLY_WHEN_RESTING, "You can only craft runes while resting." },
			{ MsgKey.SAME_RUNE_MULTIPLE, "You already have this rune on your spellsbar." },
			{ MsgKey.CANT_SWAP_THOSE, "You can't swap a rune for a non-rune item." },
			{ MsgKey.CANT_PLACE_THAT, "You can't put a non-rune item on your spellsbar." },
			{ MsgKey.ITEM_PREVENTS_RECALL, "An item in your inventory prevents you from recalling." },
			{ MsgKey.CAST_ONLY_SPELLBAR, "You can only cast runes from your spellsbar." },
			{ MsgKey.STILL_ON_COOLDOWN, "[$param] is still on cooldown." },
			{ MsgKey.WEAPON_REQUIRED, "You need a weapon equipped to cast [$param]." },
			{ MsgKey.SHIELD_REQUIRED, "You need a shield equipped to cast [$param]." },
		};

		private void Awake() {
			_this = this;
			UnsetMostThings();
			LoadRunes();
			LoadClasses();
			SetupConfig();
			configFile = Config;
			SpellsBar.RegisterKeybinds();
		}


		public static void UnsetMostThings() {
			activeCooldowns = new Dictionary<string, int>();
			rankButtons = new Dictionary<int, Button>();
			tryAgain = false;
			tryAgainTime = 0f;
			tryAgainDuration = 0.25f;
			craftRank = 0;
			if (_this != null) _this.tickCooldown = 0f;
			SpellsBar.UnsetMostThings();
		}

		void ReloadAssets() {
			UnloadAssets();
			runesConfig = PrefabCreator.LoadJsonFile<RunesConfig>("runes.json");

			foreach (var data in runesConfig.runes) {
				if (data.reSet == "s1") data.resources = runesConfig.reSets.s1;
				if (data.reSet == "s2") data.resources = runesConfig.reSets.s2;
				if (data.reSet == "s3") data.resources = runesConfig.reSets.s3;
				if (data.reSet == "s4") data.resources = runesConfig.reSets.s4;
			}

			assetBundle = PrefabCreator.LoadAssetBundle("runeassets");
		}

		void UnloadAssets() {
			if (assetBundle != null) {
				assetBundle.Unload(false);
				assetBundle = null;
			}
		}

		List<RuneData> GetRanked() {
			ReloadAssets();
			return runesConfig.runes.FindAll(x => x.implemented && x.ranked);
		}

		List<RuneData> GetSimple() {
			ReloadAssets();
			return runesConfig.runes.FindAll(x => x.implemented && !x.ranked);
		}

		private void LoadRunes() {
			var simple = GetSimple();

			foreach (var data in simple) {
				if (data.recipe.prefab != "" && data.recipe.prefab != default) data.recipe.item = data.recipe.prefab;

				ReloadAssets();
				if (assetBundle.Contains(data.recipe.item)) {
					data.prefab = assetBundle.LoadAsset<GameObject>(data.recipe.item);
					data.core = data.recipe.item;
					data.prefab.name += data.rank;
					data.recipe.name += data.rank;
					data.recipe.item += data.rank;
					runesData.Add(data);
				} else {
					Log("SimpleRune " + data.recipe.item + " not found.");
					Log("extra? "+assetBundle.Contains(data.recipe.prefab));
				}
			}

			for (var i = 1; i <= 5; i++) {
				var ranked = GetRanked();
				foreach (var data in ranked) {
					if (runesConfig != null && assetBundle != null) {
						if (assetBundle.Contains(data.recipe.item)) {
							data.prefab = assetBundle.LoadAsset<GameObject>(data.recipe.item);
							data.rank = i;
							data.core = data.recipe.item;
							data.name += " " + rank2rank[data.rank];
							if (data.rank > 1) {
								data.prefab.name += data.rank;
								data.recipe.name += data.rank;
								data.recipe.item += data.rank;
							}
							runesData.Add(data);
						} else {
							Log("RankedRune "+data.recipe.item + " not found.");
						}
					}
				}
			}

			UnloadAssets();

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

		private void OnDestroy() {
			_harmony?.UnpatchAll();
			foreach (var rune in runesData) Destroy(rune.prefab);
			UnsetMostThings();
			runesData.Clear();
		}

		public static void TryRegisterPrefabs(ZNetScene zNetScene) {
			if (zNetScene == null) return;
			foreach (var rune in runesData) zNetScene.m_prefabs.Add(rune.prefab);
		}

		public static void TryRegisterItems() {
			Log("TryRegisterItems()");
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

		public static void TryRegisterRecipes() {
			Log("TryRegisterRecipes()");
			if (ObjectDB.instance == null) return;

			var wrongTime = (ObjectDB.instance?.m_items?.Count == 0);

			if (!wrongTime) Log("TryRegisterRecipes (" + ObjectDB.instance?.m_items?.Count + " items in the database).");

			var resources = new List<string>();
			foreach (var data in runesData) {
				foreach (var item in data.resources) {
					if (!resources.Contains(item)) resources.Add(item);
				}
			}

			var missing = new List<string>();

			foreach (var value in resources) {
				var parts = value.Split(':');
				var item = parts[0];
				var pref = ObjectDB.instance.GetItemPrefab(item);
				if (pref == null) missing.Add(item);
			}

			if (missing.Count > 0) {
				if (!wrongTime) Log("Some requeriments are not ready yet (" + string.Join(", ", missing) + "). Let's try again in few miliseconds...");
				tryAgain = true;
				tryAgainTime = 0f;
				return;
			} else {
				Log("All requeriments are ready!");
			}

			TryRegisterItems();

			PrefabCreator.Reset();

			foreach (var data in runesData) {

				var min = data.rank - 3;
				if (min < 0) min = 0;

				var resList = new List<string>();

				if (data.ranked) {
					for (var i = min; i < data.rank; i++) {
						var value = data.resources[i];
						resList.Add(value);
					}
				} else {
					resList = data.resources;
				}

				var mats = new List<RecipeRequirementConfig>();

				foreach (var value in resList) {
					var parts = value.Split(':');
					var item = parts[0];
					var amount = 1;
					if (parts.Length == 2) amount = int.Parse(parts[1]);
					mats.Add(new RecipeRequirementConfig { item = item, amount = amount });
				}

				data.recipe.craftingStation = null;

				if (data.recipe.amount == 0) data.recipe.amount = runesConfig.defRecipes.amount;
				if (data.recipe.minStationLevel == 0) data.recipe.minStationLevel = runesConfig.defRecipes.minStationLevel;
				if (data.recipe.repairStation == "") data.recipe.repairStation = runesConfig.defRecipes.repairStation;

				data.recipe.resources = mats;
				data.recipe.enabled = true;

				data.itemDrop.m_itemData.m_shared.m_name = data.name;
				data.itemDrop.m_itemData.m_shared.m_description = data.description;
				data.itemDrop.m_itemData.m_shared.m_maxStackSize = data.maxstack != default ? data.maxstack : 10;
				data.itemDrop.m_itemData.m_shared.m_weight = 0.1f;

				PrefabCreator.AddNewRuneRecipe(data);
				var rune = new Rune(data, null);
				runes.Add(rune);
			}
		}

		public enum KeyModifiers {
			SHIFT,
			CTRL,
			ALT
		}

		public enum InvBarPosition {
			TOP,
			BOTTOM
		}

		public enum CastingMessage {
			GLOBAL,
			NORMAL,
			SELF,
			NONE
		}

		// CASTING
		public static ConfigEntry<CastingMessage> configCastingMessage;
		public static ConfigEntry<bool> configCooldownsEnabled;
		public static ConfigEntry<int> configClassControl;
		// PVP
		public static ConfigEntry<bool> configPvpEnabled;
		// SPELLSBAR
		public static ConfigEntry<InvBarPosition> configInvBarPosition;
		// HOTKEYS BAR
		public static ConfigEntry<bool> configHotkeysEnabled;
		public static ConfigEntry<int> configHotkeysScale;
		public static ConfigEntry<int> configHotkeysOffsetX;
		public static ConfigEntry<int> configHotkeysOffsetY;
		public static ConfigEntry<KeyModifiers> configHotkeysModifier;
		// INTERFACE
		public static ConfigEntry<bool> configRanksTabEnabled;
		public static ConfigEntry<int> configRanksOffsetX;
		public static ConfigEntry<int> configRanksOffsetY;

		private void SetupConfig() {
			Config.Bind("General", "NexusID", 840, "NexusMods ID for updates.");

			// CASTING
			configCastingMessage = Config.Bind("Casting", "Message", CastingMessage.NORMAL, "Define where the casting message should appear.");
			configCooldownsEnabled = Config.Bind("Casting", "Cooldowns", true, "Enables cooldowns when rune-casting.");
			configClassControl = Config.Bind("Casting", "Class Control", 20, "Defines the max level that your alt classes can raise up to.");
			// PVP
			configPvpEnabled = Config.Bind("PVP", "Enabled", true, "If enabled, this will count pvp-flagged players as enemies.");
			// SPELLSBAR
			configInvBarPosition = Config.Bind("SpellsBar", "Position", InvBarPosition.BOTTOM, "Defines where the inventory spells' bar should appear.");
			// HOTKEYS BAR
			configHotkeysEnabled = Config.Bind("HotkeysBar", "Enabled", true, "Enables the hotkey's bar (the one the bottom of the screen).");
			configHotkeysScale = Config.Bind("HotkeysBar", "Scale", 100, "Adjusts the hotkey's bar size.");
			configHotkeysOffsetX = Config.Bind("HotkeysBar", "OffsetX", 0, "Adjust the hotkey's bar horizontal position (left/right).");
			configHotkeysOffsetY = Config.Bind("HotkeysBar", "OffsetY", 0, "Adjust the hotkey's bar vertical position (down/up).");
			configHotkeysModifier = Config.Bind("HotkeysBar", "Modifier", KeyModifiers.SHIFT, "Key modifier to use the runes.");
			// INTERFACE
			configRanksTabEnabled = Config.Bind("Interface", "Rank Tabs", true, "Enables the 'Rank Tab's on your crafting panel.");
			configRanksOffsetX = Config.Bind("Interface", "RanksX", 0, "Adjust the rank's buttons horizontal position (left/right).");
			configRanksOffsetY = Config.Bind("Interface", "RanksY", 0, "Adjust the rank's buttons vertical position (down/up).");
		}

		public static Rune GetStaticRune(RuneData data) {
			var rune = runes.Find(r => r.data.name == data.name);
			rune.SetCaster(Player.m_localPlayer);
			return rune;
		}

		public static void ClearCache() {
			runes.ForEach(rune => rune.ClearCache());
		}

		public static RuneData GetRuneData(string name) {
			return runesData.Find(r => r.recipe.item == name);
		}

		public static Rune CreateRunicEffect(string name, Player caster, string dsbuffs) {
			var data = runesData.Find(r => r.recipe.item == name);
			if (data == null) return null;

			var rune = new Rune(data, caster);
			rune.ParseBuffs(dsbuffs);
			rune.CreateEffect();

			return rune;
		}

		public static void CreateRankTabs(InventoryGui gui) {
			if (gui == null) gui = InventoryGui.instance;

			var parent = gui?.m_tabUpgrade?.transform?.parent;
			if (parent == null) return;

			var enabled = configRanksTabEnabled.Value;

			for (var rank = 1; rank <= 5; rank++) {
				if (enabled) {
					CreateRankTab(gui, rank);
				} else {
					DeleteRankTab(gui, rank);
				}
			}

			onTabPressed(0, true);
		}

		public static void DeleteRankTab(InventoryGui gui, int rank) {
			if (rankButtons.ContainsKey(rank)) {
				var button = rankButtons[rank];
				Destroy(button.gameObject);
				rankButtons.Remove(rank);
			}
		}

		public static void CreateRankTab(InventoryGui gui, int rank) {
			Button button;

			var offsetx = configRanksOffsetX.Value;
			var offsety = configRanksOffsetY.Value;

			if (gui?.m_tabUpgrade == null) return;
			var posbase = gui.m_tabUpgrade.GetComponent<RectTransform>().anchoredPosition;


			if (rankButtons.ContainsKey(rank)) {
				button = rankButtons[rank];
				var go = button.gameObject;
			} else {
				button = Instantiate(gui.m_tabUpgrade, gui.m_tabUpgrade.transform.parent, true);
				rankButtons[rank] = button;

				var go = button.gameObject;

				go.name = "craftRank" + rank;
				go.GetComponentInChildren<Text>().text = rank2rank[rank];

				go.transform.SetSiblingIndex(gui.m_tabUpgrade.transform.parent.childCount - 2);

				button.onClick = new Button.ButtonClickedEvent();
				button.onClick.AddListener(button.GetComponent<ButtonSfx>().OnClick);
				button.onClick.AddListener(() => onTabPressed(rank, true));
			}

			var posx = 140;
			var width = 35;
			var padleft = 0;

			var rect = button.gameObject.GetComponent<RectTransform>();
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

			var basex = (width + padleft);
			rect.anchoredPosition = posbase + new Vector2(posx + basex * (rank + 1) + offsetx, 0 + offsety);
		}

		public static void onTabPressed(int selected, Boolean doUpgrade) {
			if (!configRanksTabEnabled.Value) return;

			var gui = InventoryGui.instance;
			craftRank = (selected == craftRank) ? 0 : selected;

			for (var rank = 1; rank <= 5; rank++) {
				if (rankButtons.ContainsKey(rank)) {
					var button = rankButtons[rank];
					((Selectable)button).interactable = (rank != craftRank);
				}
			}

			if (doUpgrade) {
				try {
					gui.UpdateCraftingPanel(true);
				} catch {
				};
			}
		}

		public static void UpdateVisibilityRankTabs() {
			if (!configRanksTabEnabled.Value) return;

			var active = (Player.m_localPlayer.GetCurrentCraftingStation() == null);
			for (var rank = 1; rank <= 5; rank++) {
				var button = rankButtons[rank];
				button.gameObject.SetActive(active);
			}
		}

		public static void Recreate() {
			if (!configCooldownsEnabled.Value) activeCooldowns.Clear();
			SpellsBar.RegisterKeybinds();
			CreateRankTabs(null);
			SpellsBar.ClearBindings();
			SpellsBar.CreateHotkeysBar(null);
			SpellsBar.CreateInventoryBar(null);
			SpellsBar.UpdateInventory();
			SpellsBar.UpdateVisibility();
		}

		public static void AddCooldown(string name, int cooldown) {
			if (!configCooldownsEnabled.Value) return;
			activeCooldowns.Add(name, cooldown);
			SpellsBar.SetExtraTexts(name, cooldown);
		}

		public static int GetCooldown(RuneData data) {
			var got = activeCooldowns.TryGetValue(data.name, out int cooldown);
			return (got) ? cooldown : 0;
		}

		public static bool IsOnCooldown(RuneData data) {
			return GetCooldown(data) > 0;
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

			tickCooldown += Time.deltaTime;

			if (tickCooldown >= 1f) {
				tickCooldown -= 1f;

				var keys = activeCooldowns.Keys;
				var newCooldowns = new Dictionary<string, int>();

				foreach (var key in keys) {
					var cd = activeCooldowns[key] - 1;

					if (cd > 0) {
						newCooldowns[key] = cd;
						SpellsBar.SetExtraTexts(key, cd);
					} else {
						SpellsBar.SetExtraTexts(key, 0);
					}
				}

				activeCooldowns = newCooldowns;
			}
		}

		public static bool ShowMessage(string message, bool flag = true) {
			Player.m_localPlayer.Message(MessageHud.MessageType.Center, message);
			return flag;
		}

		public static bool ShowMessage(MsgKey key, string param, bool flag = true) {
			Player.m_localPlayer.Message(MessageHud.MessageType.Center, texts[key].Replace("$param", param));
			return flag;
		}

		public static bool ShowMessage(MsgKey key, bool flag = true) {
			Player.m_localPlayer.Message(MessageHud.MessageType.Center, texts[key]);
			return flag;
		}

		public static bool IsResting() {
			return Player.m_localPlayer.m_seman.HaveStatusEffect("Resting");
		}

		public static void Log(string message) {
			UnityEngine.Debug.Log("[RunicPower] " + message);
		}

		public static void Bar() {
			Log("=============================================================");
		}

		public static void Line() {
			Log("---------------------------");
		}

		public static void Debug(string message) {
			if (debug) Log(message);
		}

	}
}

public enum MsgKey {
	ONLY_WHEN_RESTING,
	SAME_RUNE_MULTIPLE,
	CANT_SWAP_THOSE,
	CANT_PLACE_THAT,
	ITEM_PREVENTS_RECALL,
	CAST_ONLY_SPELLBAR,
	STILL_ON_COOLDOWN,
	WEAPON_REQUIRED,
	SHIELD_REQUIRED,
}