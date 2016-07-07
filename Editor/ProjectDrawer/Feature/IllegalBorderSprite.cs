using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Unifred
{
	[InitializeOnLoad]
	public class IllegalBorderSprite : ProjectDrawerBase
	{
		protected override int ScaleX { get{return 20;}}

		/// <summary>
		/// Raises the GUI event.
		/// </summary>
		/// <param name="r">The red component.</param>
		/// <param name="instanceId">Instance identifier.</param>
		/// <param name="log">Log.</param>
		public override void OnGUI(ref Rect r, string guid)
		{
			r = CalcRect(r);
			bool isIllegal = Check(guid);
			if (isIllegal) {
				GUI.Label(r, icon, GUIStyle.none);
			}
		}

		/// <summary>
		/// Updates the data.
		/// </summary>
		/// <returns><c>true</c>, if data was updated, <c>false</c> otherwise.</returns>
		/// <param name="guid">GUID.</param>
		public bool Check(string guid)
		{
			var path = AssetDatabase.GUIDToAssetPath(guid);
			var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
			if (tex == null) {
				return false;
			}

			var isIllegal = AssetDatabase.LoadAllAssetsAtPath(path)
				.OfType<Sprite>()
				.Any(s => tex.width > (int)(s.border.x + s.border.z) || tex.height > (int)(s.border.y + s.border.w));
			return isIllegal;
		}

		public override int GetPriority()
		{
			return 0;
		}

		private static Texture2D icon;

		static IllegalBorderSprite()
		{
			var instance = new IllegalBorderSprite();

			ProjectDrawerManager.AddDrawer(instance);
			icon = AssetDatabase.LoadAssetAtPath(
				"Assets/Unifred/Image/Icon/IllegalBorderSprite.png",
				typeof(Texture2D)
			) as Texture2D;
		}
	}
}
