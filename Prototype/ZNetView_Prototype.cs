using RunicPower.Core;
using System;
using static Unity.Collections.LowLevel.Unsafe.BurstRuntime;

namespace RunicPower.Patches {

	public static class ZNetView_Prototype {

		public static ZNetView_Extended ExtendedZNetView(this ZNetView __instance) {
			var ext = __instance.gameObject.GetComponent<ZNetView_Extended>();
			if (ext == null) {
				ext = __instance.gameObject.AddComponent<ZNetView_Extended>();
				ext.zview = __instance;
			}
			return ext;
		}

		public static int CreateHashCode(this ZNetView __instance, string hashValue) {
			return __instance.ExtendedZNetView().CreateHashCode(hashValue);
		}

		public static void SetHashCode(this ZNetView __instance, int hashCode, string hashValue) {
			__instance.ExtendedZNetView().SetHashCode(hashCode, hashValue);
		}

		public static string GetHashCode(this ZNetView __instance, int hashCode) {
			return __instance.ExtendedZNetView().GetHashCode(hashCode);
		}

	}
}
