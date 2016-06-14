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
		public override void OnGUI(ref Rect r, string guid, Dictionary<int, object> log)
		{
			r = CalcRect(r);
			GUI.Label(r, missingIconTexture, GUIStyle.none);
			return;
		}

		/// <summary>
		/// detecte own object whether script was missed or not.
		/// </summary>
		/// <returns>The data.</returns>
		/// <param name="instanceId">Instance identifier.</param>
		public override object UpdateData(int instanceId)
		{
			return null;
//			var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
//
//			bool is_missing = HasSample(go);
//			if (is_missing) {
//				var result = new SampleObject();
//				result.State = SampleState.SampleOwn;
//				return result;
//			}
//			else {
//				return null;
//			}
		}

		/// <summary>
		/// detecte parent object whether have children missing script.
		/// </summary>
		/// <returns>The data.</returns>
		/// <param name="instanceId">Instance identifier.</param>
		public override void FixedUpdate(ref Dictionary<int, object> data)
		{
//			List<int> listDetectedByChildren = new List<int>();
//			foreach (var pair in data) {
//				var instance = EditorUtility.InstanceIDToObject(pair.Key) as GameObject;
//				var parents = instance.GetComponentsInParent<Transform>().Select(t => t.gameObject);
//				foreach (var parent in parents) {
//					object result_obj;
//					int id = parent.GetInstanceID();
//					bool is_exist = data.TryGetValue(id, out result_obj);
//					if (!is_exist) {
//						listDetectedByChildren.Add(id);
//					}
//				}
//			}
//
//			var uniqList = listDetectedByChildren.Distinct();
//			foreach (var id in uniqList) {
//				var result = new SampleObject();
//				result.State = SampleState.SampleChild;
//				data.Add(id, result);
//			}
		}

		public override int GetPriority()
		{
			return 0;
		}

		private static Texture2D missingIconTexture;

		static SampleIcon()
		{
			var instance = new SampleIcon();

			ProjectDrawerManager.AddDrawer(instance);
			missingIconTexture = AssetDatabase.LoadAssetAtPath(
				"Assets/Unifred/Image/Icon/RayOff.png",
				typeof(Texture2D)
			) as Texture2D;
		}
	}
}
