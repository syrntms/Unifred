using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Unifred.Feature
{
	public class ProjectDrawerActivateWindow : UnifredWindowController<ProjectDrawerActivateObject>
	{
		public static void ShowWindow()
		{
			ShowWindow(new ProjectDrawerActivateFeature(), string.Empty);
		}
	}

	public class ProjectDrawerActivateFeature : UnifredFeatureBase<ProjectDrawerActivateObject>
	{
		private static GUIStyle textStyle = new GUIStyle
		{
			richText = true,
			fontSize = 12,
			margin = new RectOffset(5, 5, 5, 5),
			fixedWidth = 50,
			alignment = TextAnchor.MiddleLeft,
			normal = { textColor = Color.white, },
		};

		public override string GetDescription()
		{
			return "input hotkey";
		}

		public override CandidateSelectMode GetSelectMode()
		{
			return CandidateSelectMode.Multiple;
		}

		public override IEnumerable<ProjectDrawerActivateObject> UpdateCandidate(string word)
		{
			var result = new List<ProjectDrawerActivateObject>();
			foreach (var item in ProjectDrawerManager.list) {
				var obj = new ProjectDrawerActivateObject();
				obj.drawer = item;
				obj.isActive = item.GetEnabled();
				result.Add(obj);
			}
			return result;
		}	

		public override void Draw(
			string word,
			ProjectDrawerActivateObject candidate,
			bool is_selected
		) {
			string color = candidate.isActive ? "#ffffffff" : "#ffffff88";
			string text	= string.Format("<color={0}>type: {1}</color>", color, candidate.drawer);
			GUILayout.Label(text,	textStyle);
		}

		public override void Select(string word, IEnumerable<ProjectDrawerActivateObject> result_list)
		{
			foreach (var result in result_list) {
				bool rt = !result.drawer.GetEnabled();
				result.drawer.SetEnable(rt);
			}
		}

		public override float GetRowHeight()
		{
			return textStyle.CalcSize(new GUIContent("sample")).y
				+ textStyle.margin.bottom + textStyle.margin.top;
		}
	}

	public class ProjectDrawerActivateObject
	{
		public ProjectDrawerBase	drawer;
		public bool					isActive;
	};
}
