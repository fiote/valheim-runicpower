using HarmonyLib;

namespace RunicPower {

	[HarmonyPatch(typeof(ObjectDB), "CopyOtherDB")]
	public static class ObjectDB_CopyOtherDB_Patch {
		public static void Postfix() {
			RunicPower.TryRegisterItems();
			RunicPower.TryRegisterRecipes();
		}
	}

	[HarmonyPatch(typeof(ObjectDB), "Awake")]
	public static class ObjectDB_Awake_Patch {
		public static void Postfix() {
			RunicPower.TryRegisterItems();
			RunicPower.TryRegisterRecipes();
		}
	}
}
