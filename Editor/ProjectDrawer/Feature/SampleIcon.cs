using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Unifred
{
	[InitializeOnLoad]
	public class SampleIcon : ProjectDrawerBase
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
			GUI.Label(r, icon, GUIStyle.none);
			return;
		}

		public override int GetPriority()
		{
			return 0;
		}

		private static Texture2D icon;

		static SampleIcon()
		{
			var instance = new SampleIcon();

			ProjectDrawerManager.AddDrawer(instance);
			icon = AssetDatabase.LoadAssetAtPath(
				"Assets/Unifred/Image/Icon/RayOff.png",
				typeof(Texture2D)
			) as Texture2D;
		}
	}
}
