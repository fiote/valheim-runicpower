namespace RunicPower {
	public static class HitData_Prototype {
		public static string GetTotals(this HitData __instance) {
			return __instance.GetTotalDamage() + " (P: " + __instance.GetTotalPhysicalDamage() + ", E: " + __instance.GetTotalElementalDamage() + ")";
		}
	}
}