using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unifred.Feature
{
	public class IconSetupWindow : UnifredWindowController<IconSetupObject>
	{
		public static void ShowWindow()
		{
			ShowWindow(new IconSetup(), string.Empty);
		}
	}

	public class IconSetup : UnifredFeatureBase<IconSetupObject>
	{
		private const string deleteIconName = "None";
		private static GUIStyle textGuiStyle = new GUIStyle {
			richText = true,
			fontSize = 12,
			margin = new RectOffset(5, 5, 5, 5),
			alignment = TextAnchor.MiddleLeft,
			normal = { textColor = Color.white }
		};

		public override string GetDescription()
		{
			return "input icon name you want to set";
		}

		public override CandidateSelectMode GetSelectMode()
		{
			return CandidateSelectMode.Single;
		}

		public override IEnumerable<IconSetupObject> UpdateCandidate(string line)
		{
			IEnumerable<IconSetupObject> candidates = _GetDefaultList();
            if (string.IsNullOrEmpty(line)) {
                return candidates;
			}

			var words = line.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
			if (words.Length <= 0) {
				return candidates;
			}

			List<IconSetupObject> result = new List<IconSetupObject>();
			foreach (var candidate in candidates) {
				bool is_contain = words.All(
					(word_unit) => candidate.name.IndexOf(word_unit, StringComparison.OrdinalIgnoreCase) >= 0
				);
				if (!is_contain) {
					continue;
				}
				result.Add(candidate);
			}
			return result;
		}	

		public override void Draw(
			string word,
			IconSetupObject candidate,
			bool is_selected
		) {
            GUILayout.Label(candidate.name, textGuiStyle);
		}

		public override void Select(string word, IEnumerable<IconSetupObject> result_list)
		{
			IconSetupObject result = result_list.FirstOrDefault();
			if (result == null) {
				return;
			}

			if (result.name == deleteIconName) {
				Selection.gameObjects.ForEach( go => _UnsetIcon(go) );
				return;
			}


			Selection.gameObjects.ForEach( go => _SetIcon(go, result.content) );
		}

		public override float GetRowHeight()
		{
			return textGuiStyle.CalcSize(new GUIContent("sample")).y
				+ textGuiStyle.margin.bottom + textGuiStyle.margin.top;
		}

		private static void _UnsetIcon(GameObject gameObject)
		{
			var setIcon = typeof(EditorGUIUtility).GetMethod(
				"SetIconForObject",
				BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic,
				null,
				new Type[]{typeof(UnityEngine.Object), typeof(Texture2D)},
				null
			);
			object[] param = new object[]{gameObject, null};
			setIcon.Invoke(null, param);
		}

		private static void _SetIcon(GameObject gameObject, GUIContent content)
		{
			var setIcon = typeof(EditorGUIUtility).GetMethod(
				"SetIconForObject",
				BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic,
				null,
				new Type[]{typeof(UnityEngine.Object), typeof(Texture2D)},
				null
			);
			object[] param = new object[]{gameObject, content.image};
			setIcon.Invoke(null, param);
		}

		private IEnumerable<IconSetupObject> _GetDefaultList()
		{
			var list = new []{
				new {template = "sv_label_{0}",					startIndex = 0, count = 8 },
				new {template = "sv_icon_name{0}",				startIndex = 0, count = 8 },
				new {template = "sv_icon_dot{0}_sml",			startIndex = 0, count = 16},
				new {template = "sv_icon_dot{0}_pix16_gizmo",	startIndex = 0, count = 16},
				new {template =  deleteIconName,				startIndex = 0, count = 1},
			};

			return list.SelectMany( x => _GetTextures(x.template, x.startIndex, x.count) );
		}

		private static IEnumerable<IconSetupObject> _GetTextures(string template, int startIndex, int count)
		{
			return Enumerable.Range(startIndex, count)
				.Select( index => string.Format(template, index))
				.Select( file_name => new IconSetupObject(){ content = EditorGUIUtility.IconContent(file_name), name = file_name,});
		}
	}

	public class IconSetupObject
	{
		public GUIContent content;
		public string name;
	};
}
