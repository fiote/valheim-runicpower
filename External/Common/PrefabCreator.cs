using BepInEx;
using LitJson;
using RunicPower.Core;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Common {
    public static class PrefabCreator {
        public static Dictionary<string, CraftingStation> CraftingStations;
        private static Dictionary<string, Texture2D> cachedTextures = new Dictionary<string, Texture2D>();

        public static T RequireComponent<T>(GameObject go) where T : Component {
            var c = go.GetComponent<T>();
            if (c == null) {
                c = go.AddComponent<T>();
            }

            return c;
        }

        public static void Reset() {
            CraftingStations = null;
        }

        private static void InitCraftingStations() {
            if (CraftingStations == null) {
                CraftingStations = new Dictionary<string, CraftingStation>();
                foreach (var recipe in ObjectDB.instance.m_recipes) {
                    if (recipe.m_craftingStation != null && !CraftingStations.ContainsKey(recipe.m_craftingStation.name)) {
                        CraftingStations.Add(recipe.m_craftingStation.name, recipe.m_craftingStation);
                    }
                }
            }
        }

        public static T LoadJsonFile<T>(string filename) where T : class {
            var jsonFileName = GetAssetPath(filename);
            if (!string.IsNullOrEmpty(jsonFileName)) {
                var jsonFile = File.ReadAllText(jsonFileName);
                return JsonMapper.ToObject<T>(jsonFile);
            }

            return null;
        }

        public static string GetAssetPath(string assetName) {
            var assetFileName = Path.Combine(Paths.PluginPath, "RunicPower", assetName);
            if (!File.Exists(assetFileName)) {
                Assembly assembly = typeof(RunicPower.RunicPower).Assembly;
                assetFileName = Path.Combine(Path.GetDirectoryName(assembly.Location), assetName);
                if (!File.Exists(assetFileName)) {
                    Debug.LogError($"Could not find asset ({assetName})");
                    return null;
                }
            }

            return assetFileName;
        }

        public static AssetBundle LoadAssetBundle(string filename) {
            var assetBundlePath = GetAssetPath(filename);
            if (!string.IsNullOrEmpty(assetBundlePath)) {
                return AssetBundle.LoadFromFile(assetBundlePath);
            }

            return null;
        }

        public static Sprite LoadCustomTexture(string name) {
            string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filepath = Path.Combine(directoryName, name+".png");
            if (File.Exists(filepath)) {
                Texture2D texture2D = LoadTexture(filepath);
                return Sprite.Create(texture2D, new Rect(0f, 0f, 32f, 32f), Vector2.zero);
            } else {
                Debug.LogError("Unable to load skill icon! Make sure you place the "+name+".png file in the plugins directory!");
                return null;
            }
        }

        private static Texture2D LoadTexture(string filepath) {
            if (cachedTextures.ContainsKey(filepath)) {
                return cachedTextures[filepath];
            }
            Texture2D texture2D = new Texture2D(0, 0);
            ImageConversion.LoadImage(texture2D, File.ReadAllBytes(filepath));
            return texture2D;
        }

        public static Recipe CreateRecipe(string name, string itemId, RecipeConfig recipeConfig) {
            InitCraftingStations();

            var itemPrefab = ObjectDB.instance.GetItemPrefab(itemId);
            if (itemPrefab == null) {
                Debug.LogWarning($"[PrefabCreator] Could not find item prefab ({itemId})");
                return null;
            }

            var newRecipe = ScriptableObject.CreateInstance<Recipe>();
            newRecipe.name = name;
            newRecipe.m_amount = recipeConfig.amount;
            newRecipe.m_minStationLevel = recipeConfig.minStationLevel;
            newRecipe.m_item = itemPrefab.GetComponent<ItemDrop>();
            newRecipe.m_enabled = recipeConfig.enabled;

            if (!string.IsNullOrEmpty(recipeConfig.craftingStation)) {
                var craftingStationExists = CraftingStations.ContainsKey(recipeConfig.craftingStation);
                if (!craftingStationExists) {
                    Debug.LogWarning($"[PrefabCreator] Could not find crafting station ({itemId}): {recipeConfig.craftingStation}");
                    var stationList = string.Join(", ", CraftingStations.Keys);
                    Debug.Log($"[PrefabCreator] Available Stations: {stationList}");
                } else {
                    newRecipe.m_craftingStation = CraftingStations[recipeConfig.craftingStation];
                }
            }

            if (!string.IsNullOrEmpty(recipeConfig.repairStation)) {
                var repairStationExists = CraftingStations.ContainsKey(recipeConfig.repairStation);
                if (!repairStationExists) {
                    Debug.LogWarning($"[PrefabCreator] Could not find repair station ({itemId}): {recipeConfig.repairStation}");
                    var stationList = string.Join(", ", CraftingStations.Keys);
                    Debug.Log($"[PrefabCreator] Available Stations: {stationList}");
                } else {
                    newRecipe.m_repairStation = CraftingStations[recipeConfig.repairStation];
                }
            }

            var reqs = new List<Piece.Requirement>();
            foreach (var requirement in recipeConfig.resources) {
                var reqPrefab = ObjectDB.instance.GetItemPrefab(requirement.item);
                if (reqPrefab == null) {
                    Debug.LogError($"[PrefabCreator] Could not find requirement item ({itemId}): {requirement.item}");
                    continue;
                }

                reqs.Add(new Piece.Requirement() {
                    m_amount = requirement.amount,
                    m_resItem = reqPrefab.GetComponent<ItemDrop>()
                });
            }
            newRecipe.m_resources = reqs.ToArray();

            return newRecipe;
        }

        public static Recipe AddNewRecipe(string name, string itemId, RecipeConfig recipeConfig) {
            var recipe = CreateRecipe(name, itemId, recipeConfig);
            if (recipe == null) {
                Debug.LogError($"[PrefabCreator] Failed to create recipe ({name})");
                return null;
            }
            return AddNewRecipe(recipe);
        }

        public static Recipe AddNewRecipe(Recipe recipe) {
            var removed = ObjectDB.instance.m_recipes.RemoveAll(x => x.name == recipe.name);
            if (removed > 0) Debug.Log($"[PrefabCreator] Removed recipe ({recipe.name}): {removed}");

            ObjectDB.instance.m_recipes.Add(recipe);
            Debug.Log($"[PrefabCreator] Added recipe: {recipe.name}");

            return recipe;
        }

        public static Recipe AddNewRuneRecipe(Rune rune) {
            return AddNewRecipe(rune.recipe.name, rune.recipe.item, rune.recipe);
		}
    }
}