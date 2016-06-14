using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using Unifred;

namespace Unifred
{
	public enum ScaledRectTransformState
	{
		ScaledOwn,
		ScaledChild,
	}

	public class ScaledRectTransformObject
	{
		public ScaledRectTransformState state;
		public int id;
	}

	[InitializeOnLoad]
	public class ScaledRectTransformIcon : HierarchyDrawerBase
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

			ScaledRectTransformObject result = ((ScaledRectTransformObject)result_raw);

			bool isPressed = false;
			switch (result.state) {
				case ScaledRectTransformState.ScaledOwn:
					isPressed = GUI.Button(r, scaledIcon, GUIStyle.none);
					if (isPressed) {
						var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
						var rect = go.GetComponent<RectTransform>();
						rect.localScale = Vector3.one;
					}
					break;
				case ScaledRectTransformState.ScaledChild:
					isPressed = GUI.Button(r, scaledIcon, GUIStyle.none);
					if (isPressed) {
						Selection.activeGameObject = EditorUtility.InstanceIDToObject(result.id) as GameObject;
					}
					break;
			}
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
			return GetScaledRectTransformObject(rect);
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
				var result = new ScaledRectTransformObject();
				result.state = ScaledRectTransformState.ScaledChild;
				result.id = parentToChild.Value;
				data.Add(parentToChild.Key, result);
			}
		}

		public ScaledRectTransformObject GetScaledRectTransformObject(RectTransform rect)
		{
			if (rect == null) {
				return null;
			}

			// キャンバスは必ずスケールするので除外する
			if (rect.GetComponent<Canvas>() != null) {
				return null;
			}

			if ( !Mathf.Approximately(rect.localScale.x , 1f)
				|| !Mathf.Approximately(rect.localScale.y , 1f)
				|| !Mathf.Approximately(rect.localScale.z , 1f)
			){
				var obj = new ScaledRectTransformObject();
				obj.state = ScaledRectTransformState.ScaledOwn;
				return obj;
			}
			return null;
		}

		public override int GetPriority()
		{
			return 0;
		}

		private static Texture2D scaledIcon;

		static ScaledRectTransformIcon()
		{
			var instance = new ScaledRectTransformIcon();

			HierarchyDrawerManager.AddDrawer(instance);
			scaledIcon = AssetDatabase.LoadAssetAtPath(
				"Assets/Unifred/Image/Icon/ScaledRectTransform.png",
				typeof(Texture2D)
			) as Texture2D;
		}
	}
}
