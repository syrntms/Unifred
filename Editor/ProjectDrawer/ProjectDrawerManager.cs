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
		public static List<ProjectDrawerBase> list = new List<ProjectDrawerBase>();

		static ProjectDrawerManager()
		{
			EditorApplication.projectWindowItemOnGUI += onGUIProject;
		}

		public static void AddDrawer(ProjectDrawerBase drawer)
		{
			list.Add(drawer);
			list = list.Distinct()
				.OrderByDescending(d => d.GetPriority())
				.ToList();
		}

		private static void onGUIProject(string guid, Rect selectionRect)
		{
			Rect r = new Rect(selectionRect);
			r.x = r.xMax;
			foreach (var drawer in list) {
				if (!drawer.GetEnabled()) {
					continue;
				}
			drawer.OnGUI(ref r, guid);
			}
		}
	}
}
