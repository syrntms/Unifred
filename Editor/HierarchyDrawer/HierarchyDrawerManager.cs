using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Unifred
{
	[InitializeOnLoad]
	public class HierarchyDrawerManager
	{
		private static double lastTime = 0f;
		private const float SecondPerCheck = 0.3f;
		public static List<HierarchyDrawerBase> list = new List<HierarchyDrawerBase>();
		private static Dictionary<Type, Dictionary<int, object>> updateRecord
			= new Dictionary<Type, Dictionary<int, object>>();

		static HierarchyDrawerManager()
		{
			EditorApplication.hierarchyWindowItemOnGUI += onGUIHierarchy;
			EditorApplication.update += updateHierarchy;
		}

		public static void AddDrawer(HierarchyDrawerBase drawer)
		{
			list.Add(drawer);
			list = list.Distinct()
				.OrderByDescending(d => d.GetPriority())
				.ToList();
			var type = drawer.GetType();
			Dictionary<int, object> v;
			if (!updateRecord.TryGetValue(type, out v)) {
				updateRecord.Add(type, new Dictionary<int, object>());
			}
		}

		private static void updateHierarchy()
		{
			var currentTime = EditorApplication.timeSinceStartup;
			if (currentTime - lastTime < SecondPerCheck) {
				return;
			}
			lastTime = currentTime;

			var go_list = GameObjectUtility.FindAllInHierarchy();
			foreach (var drawer in list) {
				var type = drawer.GetType();
				updateRecord[type].Clear();
				if (!drawer.GetEnabled()) {
					continue;
				}
				foreach (var go in go_list) {
					int id = go.GetInstanceID();
					object data = drawer.UpdateData(id);
					if (data == null) {
						continue;
					}
					updateRecord[type][id] = data;
				}
				Dictionary<int, object> dataList = updateRecord[type];
				drawer.FixedUpdate(ref dataList);
			}
			EditorApplication.RepaintHierarchyWindow();
		}

		private static void onGUIHierarchy(int instanceID, Rect selectionRect)
		{
			Rect r = new Rect(selectionRect);
			r.x = r.xMax;
			foreach (var drawer in list) {
				if (!drawer.GetEnabled()) {
					continue;
				}
				var type = drawer.GetType();
				drawer.OnGUI(ref r, instanceID, updateRecord[type]);
			}
		}
	}
}
