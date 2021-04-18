using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RunicPower {

	[HarmonyPatch(typeof(Utils), "ClampUIToScreen")]
	public static class Utils_ClampUIToScreen_Patch {
		public static bool Prefix(RectTransform transform) {
			if (transform != UITooltip.m_tooltip?.transform) return true;

			Vector3[] array = (Vector3[])(object)new Vector3[4];
			transform.GetWorldCorners(array);

			if (!((Object)(object)Utils.GetMainCamera() == (Object)null)) {
				float num = 0f;
				float num2 = 0f;

				var t = Utils.FindChild(transform, "Text");
				var h = t.gameObject.GetComponent<Text>().preferredHeight;
				var w = t.gameObject.GetComponent<Text>().preferredWidth;

				array[2].x = array[0].x + w;
				array[2].y = array[0].y - h;

				if (array[2].x > (float)Screen.width) {
					num -= array[2].x - (float)Screen.width;
				}
				if (array[0].x < 0f) {
					num -= array[0].x;
				}
				if (array[2].y > (float)Screen.height) {
					num2 -= array[2].y - (float)Screen.height;
				}
				if (array[0].y < 0f) {
					num2 -= array[0].y;
				}
				if (array[2].y < 0f) {
					var scale = GuiScaler.m_largeGuiScale*100;
					var addy = (scale - 50) * 4.5f;
					num2 -= array[2].y - addy;
				}
				Vector3 position = ((Transform)transform).position;
				position.x += num;
				position.y += num2;
				((Transform)transform).position = position;
			}

			return false;
		}
	}
}
