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
		private static float saveTime = 0f;
		private const float SecondPerCheck = 0.1f;
		public static List<IHierarchyDrawer> list = new List<IHierarchyDrawer>();
		private static Dictionary<Type, Dictionary<int, object>> updateRecord
			= new Dictionary<Type, Dictionary<int, object>>();

		static HierarchyDrawerManager()
		{
			EditorApplication.hierarchyWindowItemOnGUI += onGUIHierarchy;
			EditorApplication.update += updateHierarchy;
		}

		public static void AddDrawer(IHierarchyDrawer drawer)
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
			saveTime += Time.unscaledDeltaTime;
			if (saveTime < SecondPerCheck) {
				return;
			}
			saveTime = 0f;

			var go_list = GameObjectUtility.FindAllInHierarchy();
			foreach (var drawer in list) {
				var type = drawer.GetType();
				updateRecord[type].Clear();
				if (!drawer.IsEnable) {
					continue;
				}
				foreach (var go in go_list) {
					int id = go.GetInstanceID();
					object data = drawer.UpdateData(id);
					updateRecord[type][id] = data;
				}
			}
			EditorApplication.RepaintHierarchyWindow();
		}

		private static void onGUIHierarchy(int instanceID, Rect selectionRect)
		{
			Rect r = new Rect(selectionRect);
			r.x = r.xMax;
			foreach (var drawer in list) {
				var type = drawer.GetType();
				bool isDraw = drawer.OnGUI(r, instanceID, updateRecord[type]);
				if (!isDraw) {
					continue;
				}
				r.x -= 18;
				r.width = 20;
			}
		}
	}
}
