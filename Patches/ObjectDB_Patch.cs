using HarmonyLib;

namespace RunicPower {

	[HarmonyPatch(typeof(ObjectDB), "CopyOtherDB")]
	public static class ObjectDB_CopyOtherDB_Patch {
		public static void Postfix() {
			RunicPower.Debug("ObjectDB_CopyOtherDB_Patch Postfix");
			RunicPower.TryRegisterRecipes();
		}
	}

	[HarmonyPatch(typeof(ObjectDB), "Awake")]
	public static class ObjectDB_Awake_Patch {
		public static void Postfix() {
			RunicPower.Debug("ObjectDB_Awake_Patch Postfix");
			RunicPower.TryRegisterRecipes();
		}
	}
}
