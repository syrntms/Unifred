using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Unifred
{
	public enum MissingScriptState
	{
		MissingScriptOwn,
		MissingScriptChild,
	}

	public class MissingScriptObject
	{
		public MissingScriptState State;
		public int id;
	}

	[InitializeOnLoad]
	public class MissingScriptIcon : HierarchyDrawerBase
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

			MissingScriptObject obj = (MissingScriptObject)result_raw;
			bool is_pressed = false;
			switch (obj.State) {
				case MissingScriptState.MissingScriptOwn:
					GUI.color = Color.white;
					is_pressed = GUI.Button(r, icon, GUIStyle.none);
					if (is_pressed) {
						Selection.activeGameObject = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
					}
					break;
				case MissingScriptState.MissingScriptChild:
					GUI.color = Color.gray;
					is_pressed = GUI.Button(r, icon, GUIStyle.none);
					if (is_pressed) {
						Selection.activeGameObject = EditorUtility.InstanceIDToObject(obj.id) as GameObject;
					}
					break;
			}
			GUI.color = Color.white;
			return;
		}

		/// <summary>
		/// detecte own object whether script was missed or not.
		/// </summary>
		/// <returns>The data.</returns>
		/// <param name="instanceId">Instance identifier.</param>
		public override object UpdateData(int instanceId)
		{
			var go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;

			bool is_missing = HasMissingScript(go);
			if (is_missing) {
				var result = new MissingScriptObject();
				result.State = MissingScriptState.MissingScriptOwn;
				return result;
			}
			else {
				return null;
			}
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

			foreach (var pair in listDetectedByChildren) {
				var result = new MissingScriptObject();
				result.State = MissingScriptState.MissingScriptChild;
				result.id = pair.Value;
				data.Add(pair.Key, result);
			}
		}

		public bool HasMissingScript(GameObject go)
		{
			var components = go.GetComponents<Component>();
			bool isDisplay = components.Any(component => component == null);
			if (isDisplay) {
				return true;
			}
			return false;
		}

		public override int GetPriority()
		{
			return 0;
		}

		private static Texture2D icon;

		static MissingScriptIcon()
		{
			var instance = new MissingScriptIcon();

			HierarchyDrawerManager.AddDrawer(instance);
			icon = AssetDatabase.LoadAssetAtPath(
				"Assets/Unifred/Image/Icon/MissingScript.png",
				typeof(Texture2D)
			) as Texture2D;
		}
	}
}
