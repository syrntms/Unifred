using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unifred.Feature
{

	public class HierarchyAndSearchWindow : UnifredWindowController<HierarchyAndSearchObject>
	{
		public static void ShowWindow()
		{
			ShowWindow(new HierarchyAndSearch(), string.Empty);
		}

		public static void ShowWindow(string initial_word)
		{
			ShowWindow(new HierarchyAndSearch(), initial_word);
		}
	}

	public class HierarchyAndSearch : UnifredFeatureBase<HierarchyAndSearchObject>
	{

		private static GUIStyle textGuiStyle = new GUIStyle {
			richText = true,
			fontSize = 12,
			margin = new RectOffset(5, 5, 5, 5),
			alignment = TextAnchor.MiddleLeft,
			normal = { textColor = Color.white }
		};

		public override string GetDescription()
		{
			return "input gameobject name you want <color=white> AND </color> search";
		}

		public override CandidateSelectMode GetSelectMode()
		{
			return CandidateSelectMode.Multiple;
		}

		public override IEnumerable<HierarchyAndSearchObject> UpdateCandidate(string word)
		{
			List<HierarchyAndSearchObject> result = new List<HierarchyAndSearchObject>();
            if (string.IsNullOrEmpty(word)) {
                return result;
			}

            var gameobjects = GameObject.FindObjectsOfType<GameObject>();
			var words = word.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
			if (words.Length <= 0) {
				return result;
			}

			foreach (var gameobject in gameobjects) {
				bool is_contain = words.All(
					(word_unit) => {
						var is_in_name = gameobject.name.IndexOf(word_unit, StringComparison.OrdinalIgnoreCase) >= 0;
						var is_in_component = gameobject.GetComponents<Component>().Any(
							component => {
								if (component == null) {
									return false;
								}
								return component.GetType().Name.IndexOf(word_unit, StringComparison.OrdinalIgnoreCase) >= 0;
							}
						);
						return is_in_name || is_in_component;
					}
				);
				if (!is_contain) {
					continue;
				}
				HierarchyAndSearchObject content = new HierarchyAndSearchObject(){
					name = gameobject.name,
					target = gameobject,
				};
				result.Add(content);
			}
			return result;
		}	

		public override void Draw(
			string word,
			HierarchyAndSearchObject candidate,
			bool is_selected
		) {
            GUILayout.Label(candidate.name, textGuiStyle);
		}

		public override void Select(string word, IEnumerable<HierarchyAndSearchObject> result_list)
		{
			if (string.IsNullOrEmpty(word)) {
				EditorApplication.delayCall += () => {
					HierarchyOrSearchWindow.ShowWindow();
				};
			}
			Selection.objects = result_list.Select((t) => {return t.target;}).Cast<GameObject>().ToArray();
			EditorApplication.ExecuteMenuItem("Window/Hierarchy");
			_SaveHistory("HierarchyAndSearch", word);
		}

		public override float GetRowHeight()
		{
			return textGuiStyle.CalcSize(new GUIContent("sample")).y
				+ textGuiStyle.margin.bottom + textGuiStyle.margin.top;
		}
	}

	public class HierarchyAndSearchObject {
		public string name;
		public GameObject target;
	};
}
