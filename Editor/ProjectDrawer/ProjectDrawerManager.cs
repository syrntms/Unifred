using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Unifred
{
	[InitializeOnLoad]
	public class ProjectDrawerManager
	{
//		private static double lastTime = 0f;
		private const float SecondPerCheck = 0.3f;
		public static List<ProjectDrawerBase> list = new List<ProjectDrawerBase>();
		private static Dictionary<Type, Dictionary<int, object>> updateRecord
			= new Dictionary<Type, Dictionary<int, object>>();

		static ProjectDrawerManager()
		{
			EditorApplication.projectWindowItemOnGUI += onGUIProject;
//			EditorApplication.update += updateProject;
		}

		public static void AddDrawer(ProjectDrawerBase drawer)
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

		private static void updateProject()
		{
//			var currentTime = EditorApplication.timeSinceStartup;
//			if (currentTime - lastTime < SecondPerCheck) {
//				return;
//			}
//			lastTime = currentTime;
//
//			var go_list = GameObjectUtility.FindAllInProject();
//			foreach (var drawer in list) {
//				var type = drawer.GetType();
//				updateRecord[type].Clear();
//				if (!drawer.GetEnabled()) {
//					continue;
//				}
//				foreach (var go in go_list) {
//					int id = go.GetInstanceID();
//					object data = drawer.UpdateData(id);
//					if (data == null) {
//						continue;
//					}
//					updateRecord[type][id] = data;
//				}
//				Dictionary<int, object> dataList = updateRecord[type];
//				drawer.FixedUpdate(ref dataList);
//			}
//			EditorApplication.RepaintProjectWindow();
		}

		private static void onGUIProject(string guid, Rect selectionRect)
		{
			Rect r = new Rect(selectionRect);
			r.x = r.xMax;
			foreach (var drawer in list) {
				if (!drawer.GetEnabled()) {
					continue;
				}
				var type = drawer.GetType();
				drawer.OnGUI(ref r, guid, updateRecord[type]);
			}
		}
	}
}
