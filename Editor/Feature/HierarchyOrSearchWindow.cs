using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unifred
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
			normal = { textColor = EditorStyles.label.normal.textColor }
		};

		//set backgroudn color only
		private static GUIStyle defaultRowGuiStyle;

		//set backgroudn color only
		private static GUIStyle selectedRowGuiStyle;

		public override string GetDescription()
		{
			return "input gameobject name you want <color=yellow> OR </color> search";
		}

		public override bool IsMultipleSelect()
		{
			return true;
		}

		public override void OnInit()
		{
			defaultRowGuiStyle = new GUIStyle {
				normal = {background = TextureUtility.MakeSolidTexture(Color.clear),}
			};
			selectedRowGuiStyle = new GUIStyle {
				normal = { background = TextureUtility.MakeSolidTexture(Color.magenta + Color.gray * 1.25f),},
			};
		}

		public override void OnDestroy()
		{
			GameObject.DestroyImmediate(defaultRowGuiStyle.normal.background);
			GameObject.DestroyImmediate(selectedRowGuiStyle.normal.background);
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
					(word_unit) => {return gameobject.name.IndexOf(word_unit, StringComparison.OrdinalIgnoreCase) >= 0;}
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
			IEnumerable<HierarchyOrSearchObject> result_list,
			IEnumerable<IntRange> selected_list,
			int offset,
			int count
		) {

			IEnumerable<int> uniq_selected_list = IntRange.Split(selected_list);

			for (int i = offset ; i < offset + count ; ++i) {
				bool is_selected = uniq_selected_list.Any((index) => {return index == i;});
				GUIStyle style = (is_selected)? selectedRowGuiStyle:defaultRowGuiStyle;
				HierarchyOrSearchObject result = result_list.ElementAt(i);
	            GUILayout.BeginHorizontal(style);
	            GUILayout.Label(result.name, textGuiStyle);
	            GUILayout.EndHorizontal();
			}
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
