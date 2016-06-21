using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using Unifred;

namespace Unifred
{
	public enum DuplicatedComponentState
	{
		DuplicatedByOwn,
		DuplicatedByChild,
	}

	public class DuplicatedComponentObject
	{
		public DuplicatedComponentState state;
		public int id;
	}

	[InitializeOnLoad]
	public class DuplicatedComponentIcon : HierarchyDrawerBase
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

			DuplicatedComponentObject result = ((DuplicatedComponentObject)result_raw);

			bool isPressed = false;
			switch (result.state) {
				case DuplicatedComponentState.DuplicatedByOwn:
					GUI.color = Color.white;
					isPressed = GUI.Button(r, icon, GUIStyle.none);
					if (isPressed) {
						Selection.activeGameObject = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
					}
					break;
				case DuplicatedComponentState.DuplicatedByChild:
					GUI.color = Color.gray;
					isPressed = GUI.Button(r, icon, GUIStyle.none);
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
			var types = go.GetComponents<Component>()
				.Where(c => c != null)
				.Select(c => c.GetType());
			if (types.Count() != types.Distinct().Count()) {
				var obj = new DuplicatedComponentObject();
				obj.state = DuplicatedComponentState.DuplicatedByOwn;
				return obj;
			}
			return null;
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
				var result = new DuplicatedComponentObject();
				result.state = DuplicatedComponentState.DuplicatedByChild;
				result.id = parentToChild.Value;
				data.Add(parentToChild.Key, result);
			}
		}

		public override int GetPriority()
		{
			return 0;
		}

		private static Texture2D icon;

		static DuplicatedComponentIcon()
		{
			var instance = new DuplicatedComponentIcon();

			HierarchyDrawerManager.AddDrawer(instance);
			icon = AssetDatabase.LoadAssetAtPath(
				"Assets/Unifred/Image/Icon/DuplicatedComponent.png",
				typeof(Texture2D)
			) as Texture2D;
		}
	}
}
