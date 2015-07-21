using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unifred
{

	public class HierarchyAndSearchWindow : UnifredWindowController<HierarchyAndSearchObject>
	{
		[MenuItem("Unifred/SearchHierarchy %g")]
		public static void SearchFromHierarchy()
		{
			ShowWindow(new HierarchyAndSearch(), string.Empty);
		}

		public static void SearchFromHierarchy(string initial_word)
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
			return "input gameobject name you want <color=yellow> AND </color> search";
		}

		public override bool IsMultipleSelect()
		{
			return true;
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
					(word_unit) => {return gameobject.name.IndexOf(word_unit, StringComparison.OrdinalIgnoreCase) >= 0;}
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
					HierarchyOrSearchWindow.SearchFromHierarchy();
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
