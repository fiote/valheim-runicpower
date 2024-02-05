using RunicPower.Core;
using System.Collections.Generic;
using UnityEngine;

namespace RunicPower.Patches {
	public class ZNetView_Extended : MonoBehaviour {
		
		public Dictionary<int, string> hashCodes = new Dictionary<int, string>();
		public ZNetView zview;


		public int CreateHashCode(string hashValue) {
			RunicPower.Log($"ZNetView_Extended:CreateHashCode {hashValue}");
			var hashCode = StringExtensionMethods.GetStableHashCode(hashValue);
			SetHashCode(hashCode, hashValue);
			zview.InvokeRPC("RPC_SetHashCode", hashCode, hashValue);
			return hashCode;
		}

		public void SetHashCode(int hashCode, string hashValue) {
			RunicPower.Log($"ZNetView_Extended:SetHashCode {hashCode} -> {hashValue}");
			hashCodes[hashCode] = hashValue;
		}

		public string GetHashCode(int hashCode) {
			if (hashCodes.ContainsKey(hashCode)) {
				var hashValue = hashCodes[hashCode];
				RunicPower.Log($"ZNetView_Extended:GetHashCode {hashCode} -> {hashValue}");
				return hashValue;
			}
			return "";
		}

	}
}

