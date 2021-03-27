using BepInEx;
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

namespace RunicPower {
	[BepInPlugin("fiote.mods.runicpower", "RunicPower", "1.0.0")]
	[BepInDependency("com.pipakin.SkillInjectorMod")]
	[BepInDependency("randyknapp.mods.extendeditemdataframework")]

	public class RunicPower : BaseUnityPlugin {
		private Harmony _harmony;

		public static RunesConfig runesConfig;
		public static List<Rune> runes = new List<Rune>();
		public static List<ClassSkill> cskills = new List<ClassSkill>();

		private void Awake() {
			LoadRunes();
			LoadClasses();
			SpellsBar.RegisterKeybinds(Config);
		}

		private void LoadRunes() {
			runesConfig = PrefabCreator.LoadJsonFile<RunesConfig>("runes.json");
			var assetBundle = PrefabCreator.LoadAssetBundle("runeassets");
			if (runesConfig != null && assetBundle != null) {
				foreach (var rune in runesConfig.runes) {
					if (!rune.implemented) continue;
					if (assetBundle.Contains(rune.recipe.item)) {
						rune.prefab = assetBundle.LoadAsset<GameObject>(rune.recipe.item);
						runes.Add(rune);
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
					cskills.Add(cskill);
					Debug.Log("ClassSkill " + cskill.id + " " + cskill.name);
					SkillInjector.RegisterNewSkill(cskill.id, cskill.name, cskill.description, 1.0f, PrefabCreator.LoadCustomTexture(cskill.icon), Skills.SkillType.Unarmed);
				}
			}
		}

		private void OnDestroy() {
			_harmony?.UnpatchAll();
			foreach (var rune in runes) Destroy(rune.prefab);
			runes.Clear();
		}

		public static void TryRegisterPrefabs(ZNetScene zNetScene) {
			if (zNetScene == null) return;
			foreach (var rune in runes) zNetScene.m_prefabs.Add(rune.prefab);
		}

		public static void TryRegisterItems() {
			if (ObjectDB.instance == null || ObjectDB.instance.m_items.Count == 0) return;

			foreach (var rune in runes) {
				rune.itemDrop = rune.prefab.GetComponent<ItemDrop>();
				if (rune.itemDrop == null) continue;
				if (ObjectDB.instance.GetItemPrefab(rune.prefab.name.GetStableHashCode()) != null) continue;
				RegisterRune(rune);
			}
		}

		private static void RegisterRune(Rune rune) {
			var itemDrop = rune.itemDrop;
			itemDrop.SetRune(rune);
			ObjectDB.instance.m_items.Add(rune.prefab);

		}

		public static void TryRegisterRecipes() {
			if (ObjectDB.instance == null || ObjectDB.instance.m_items.Count == 0) return;
			PrefabCreator.Reset();
			foreach (var rune in runes) {
				if (rune.recipe.amount == 0) rune.recipe.amount = runesConfig.defRecipes.amount;
				if (rune.recipe.craftingStation == "") rune.recipe.craftingStation = runesConfig.defRecipes.craftingStation;
				if (rune.recipe.minStationLevel == 0) rune.recipe.minStationLevel = runesConfig.defRecipes.minStationLevel;
				if (rune.recipe.repairStation == "") rune.recipe.repairStation = runesConfig.defRecipes.repairStation;

				rune.recipe.enabled = true;
				rune.itemDrop.m_itemData.m_shared.m_name = rune.name;
				rune.itemDrop.m_itemData.m_shared.m_description = rune.description;
				rune.itemDrop.m_itemData.m_shared.m_maxStackSize = 100;
				rune.itemDrop.m_itemData.m_shared.m_weight = 0.1f;

				PrefabCreator.AddNewRuneRecipe(rune);
			}
		}

		private void Update() {
			var player = Player.m_localPlayer;
			if (player != null && player.TakeInput()) {
				SpellsBar.CheckInputs();
			}
		}
	}
}