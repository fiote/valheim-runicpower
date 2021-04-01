using BepInEx;
using Common;
using HarmonyLib;
using LitJson;
using Pipakin.SkillInjectorMod;
using RuneStones.Core;
using RunicPower.Core;
using RunicPower.Patches;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace RunicPower {
	[BepInPlugin("fiote.mods.runicpower", "RunicPower", "1.0.2")]
	[BepInDependency("com.pipakin.SkillInjectorMod")]
	[BepInDependency("randyknapp.mods.extendeditemdataframework")]

	public class RunicPower : BaseUnityPlugin {
		private Harmony _harmony;

		public static RunesConfig runesConfig;
		public static List<Rune> runes = new List<Rune>();
		public static List<RuneData> runesData = new List<RuneData>();
		public static List<ClassSkill> listofCSkills = new List<ClassSkill>();

		private void Awake() {
			LoadRunes();
			LoadClasses();
			SpellsBar.RegisterKeybinds(Config);
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
			if (ObjectDB.instance == null || ObjectDB.instance.m_items.Count == 0) return;

			foreach (var runeConfig in runesData) {
				runeConfig.itemDrop = runeConfig.prefab.GetComponent<ItemDrop>();
				if (runeConfig.itemDrop == null) continue;
				if (ObjectDB.instance.GetItemPrefab(runeConfig.prefab.name.GetStableHashCode()) != null) continue;
				RegisterRuneConfig(runeConfig);
			}
		}

		private static void RegisterRuneConfig(RuneData data) {
			var itemDrop = data.itemDrop;
			itemDrop.SetRuneData(data);
			ObjectDB.instance.m_items.Add(data.prefab);

		}

		public static void TryRegisterRecipes() {
			if (ObjectDB.instance == null || ObjectDB.instance.m_items.Count == 0) return;
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

		public static StatusEffect CreateStatusEffect(string name, Player caster, string dsbuffs) {
			var data = runesData.Find(r => r.recipe.item == name);
			if (data == null) return null;

			var rune = new Rune(data, caster);
			rune.ParseBuffs(dsbuffs);
			rune.CreateEffect();

			return rune.statusEffect;
		}

		private void Update() {
			var player = Player.m_localPlayer;
			if (player != null && player.TakeInput()) {
				SpellsBar.CheckInputs();
			}
		}
	}
}