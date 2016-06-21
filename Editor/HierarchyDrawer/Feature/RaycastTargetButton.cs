using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Unifred
{
	[InitializeOnLoad]
	public class RaycastTargetButton : HierarchyDrawerBase
	{
		protected override int ScaleX { get{return 20;}}

		public override void OnGUI(ref Rect r, int instanceId, Dictionary<int, object> log)
		{
			r = CalcRect(r);
			object result;
			bool is_exist = log.TryGetValue(instanceId, out result);
			if (!is_exist) {
				return;
			}

			bool is_changed = false;
			bool next_state = false;
			GUI.color = Color.white;

			if (!is_exist || !(bool)result) {
				bool isPress = GUI.Button(r, offButtonTexture, GUIStyle.none);
				if (isPress) {
					is_changed = true;
					next_state = true;
				}
			}
			else {
				bool isPress = GUI.Button(r, onButtonTexture, GUIStyle.none);
				if (isPress) {
					is_changed = true;
					next_state = false;
				}
			}

			if (!is_changed) {
				return;
			}

			var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
			var graphic = go.GetComponent<Graphic>();
			if (graphic == null) {
				return;
			}
			graphic.raycastTarget = next_state; 
		}

		public override object UpdateData(int instanceId)
		{
			var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
			var graphic = go.GetComponent<Graphic>();
			if (graphic == null) {
				return null;
			}
			return graphic.raycastTarget;
		}

		public override int GetPriority()
		{
			return 10;
		}


		public override void FixedUpdate(ref Dictionary<int, object> data)
		{
		}

		private static Texture2D onButtonTexture;
		private static Texture2D offButtonTexture;

		static RaycastTargetButton()
		{
			var instance = new RaycastTargetButton();

			HierarchyDrawerManager.AddDrawer(instance);
			onButtonTexture = AssetDatabase.LoadAssetAtPath(
				"Assets/Unifred/Image/Icon/RayOn.png",
				typeof(Texture2D)
			) as Texture2D;
			offButtonTexture = AssetDatabase.LoadAssetAtPath(
				"Assets/Unifred/Image/Icon/RayOff.png",
				typeof(Texture2D)
			) as Texture2D;
		}
	}
}
