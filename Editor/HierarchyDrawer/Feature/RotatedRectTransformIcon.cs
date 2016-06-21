using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using Unifred;

namespace Unifred
{
	public enum RotatedRectTransformState
	{
		RotatedOwn,
		RotatedChild,
	}

	public class RotatedRectTransformObject
	{
		public RotatedRectTransformState state;
		public int id;
	}

	[InitializeOnLoad]
	public class RotatedRectTransformIcon : HierarchyDrawerBase
	{
		protected override int ScaleX { get{return 20;}}

		/// <summary>
		/// Raises the GUI event.
		/// </summary>
		/// <param name="r">The red component.</param>
		/// <param name="instanceId">Instance identifier.</param>
		/// <param name="log">Log.</param>
		public override void OnGUI(ref Rect r, int instanceId, Dictionary<int, object> log)
		{
			r = CalcRect(r);
			object result_raw;
			bool is_exist = log.TryGetValue(instanceId, out result_raw);

			if (!is_exist) {
				return;
			}

			RotatedRectTransformObject result = ((RotatedRectTransformObject)result_raw);

			bool isPressed = false;
			switch (result.state) {
				case RotatedRectTransformState.RotatedOwn:
					GUI.color = Color.white;
					isPressed = GUI.Button(r, RotatedIcon, GUIStyle.none);
					if (isPressed) {
						var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
						var rect = go.GetComponent<RectTransform>();
						rect.localRotation = Quaternion.identity;
					}
					break;
				case RotatedRectTransformState.RotatedChild:
					GUI.color = Color.gray;
					isPressed = GUI.Button(r, RotatedIcon, GUIStyle.none);
					if (isPressed) {
						Selection.activeGameObject = EditorUtility.InstanceIDToObject(result.id) as GameObject;
					}
					break;
			}
			GUI.color = Color.white;
			return;
		}

		/// <summary>
		/// detecte own object whether RectTransform was normalized or not.
		/// </summary>
		/// <returns>The data.</returns>
		/// <param name="instanceId">Instance identifier.</param>
		public override object UpdateData(int instanceId)
		{
			var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
			var rect = go.GetComponent<RectTransform>();
			return GetRotatedRectTransformObject(rect);
		}

		/// <summary>
		/// detecte parent object whether have children missing script.
		/// </summary>
		/// <returns>The data.</returns>
		/// <param name="instanceId">Instance identifier.</param>
		public override void FixedUpdate(ref Dictionary<int, object> data)
		{
			Dictionary<int, int> listDetectedByChildren = new Dictionary<int, int>();
			foreach (var pair in data) {
				var instance = EditorUtility.InstanceIDToObject(pair.Key) as GameObject;
				var parents = instance.GetComponentsInParent<Transform>().Select(t => t.gameObject);
				foreach (var parent in parents) {
					int id = parent.GetInstanceID();
					bool is_exist = data.ContainsKey(id);
					bool is_detect = listDetectedByChildren.ContainsKey(id);
					if (!is_exist && !is_detect) {
						listDetectedByChildren.Add(id, pair.Key);
					}
				}
			}

			foreach (var parentToChild in listDetectedByChildren) {
				var result = new RotatedRectTransformObject();
				result.state = RotatedRectTransformState.RotatedChild;
				result.id = parentToChild.Value;
				data.Add(parentToChild.Key, result);
			}
		}

		public RotatedRectTransformObject GetRotatedRectTransformObject(RectTransform rect)
		{
			if (rect == null) {
				return null;
			}

			if ( !Mathf.Approximately(rect.localRotation.eulerAngles.x , 0f)
				|| !Mathf.Approximately(rect.localRotation.eulerAngles.y , 0f)
				|| !Mathf.Approximately(rect.localRotation.eulerAngles.z , 0f)
			){
				var obj = new RotatedRectTransformObject();
				obj.state = RotatedRectTransformState.RotatedOwn;
				return obj;
			}
			return null;
		}

		public override int GetPriority()
		{
			return 0;
		}

		private static Texture2D RotatedIcon;

		static RotatedRectTransformIcon()
		{
			var instance = new RotatedRectTransformIcon();

			HierarchyDrawerManager.AddDrawer(instance);
			RotatedIcon = AssetDatabase.LoadAssetAtPath(
				"Assets/Unifred/Image/Icon/RotatedRectTransform.png",
				typeof(Texture2D)
			) as Texture2D;
		}
	}
}
