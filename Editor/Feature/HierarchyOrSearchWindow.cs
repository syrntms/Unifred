using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unifred.Feature
{

	public class HierarchyOrSearchWindow : UnifredWindowController<HierarchyOrSearchObject>
	{
		public static void SearchFromHierarchy()
		{
			ShowWindow(new HierarchyOrSearch(), string.Empty);
		}

		public static void SearchFromHierarchy(string initial_word)
		{
			ShowWindow(new HierarchyOrSearch(), initial_word);
		}
	}

	public class HierarchyOrSearch : UnifredFeatureBase<HierarchyOrSearchObject>
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
			return "input gameobject name you want <color=white> OR </color> search";
		}

		public override bool IsMultipleSelect()
		{
			return true;
		}

		public override IEnumerable<HierarchyOrSearchObject> UpdateCandidate(string word)
		{
			List<HierarchyOrSearchObject> result = new List<HierarchyOrSearchObject>();
            if (string.IsNullOrEmpty(word)) {
                return result;
			}

            var gameobjects = GameObject.FindObjectsOfType<GameObject>();
			var words = word.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
			if (words.Length <= 0) {
				return result;
			}

			foreach (var gameobject in gameobjects) {
				bool is_contain = words.Any(
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
				HierarchyOrSearchObject content = new HierarchyOrSearchObject(){
					name = gameobject.name,
					target = gameobject,
				};
				result.Add(content);
			}
			return result;
		}	

		public override void Draw(
			string word,
			HierarchyOrSearchObject candidate,
			bool is_selected
		) {
            GUILayout.Label(candidate.name, textGuiStyle);
		}

		public override void Select(string word, IEnumerable<HierarchyOrSearchObject> result_list)
		{
			if (string.IsNullOrEmpty(word)) {
				EditorApplication.delayCall += () => {
					HierarchyAndSearchWindow.SearchFromHierarchy();
				};
			}
			Selection.objects = result_list.Select((t) => {return t.target;}).Cast<GameObject>().ToArray();
			EditorApplication.ExecuteMenuItem("Window/Hierarchy");
			_SaveHistory("HierarchyOrSearch", word);
		}

		public override float GetRowHeight()
		{
			return textGuiStyle.CalcSize(new GUIContent("sample")).y
				+ textGuiStyle.margin.bottom + textGuiStyle.margin.top;
		}
	}

	public class HierarchyOrSearchObject
	{
		public string name;
		public GameObject target;
	};
}
