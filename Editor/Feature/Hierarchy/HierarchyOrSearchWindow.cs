using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unifred.Feature
{

	public class HierarchyOrSearchWindow : UnifredWindowController<HierarchyOrSearchObject>
	{
		public static void ShowWindow()
		{
			ShowWindow(new HierarchyOrSearch(), string.Empty);
		}

		public static void ShowWindow(string initial_word)
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
			return "Input name you wanna search. "
				+ "<color=white>Space</color> = OR. ";
		}

		public override CandidateSelectMode GetSelectMode()
		{
			return CandidateSelectMode.Multiple;
		}

		public override IEnumerable<HierarchyOrSearchObject> UpdateCandidate(string word)
		{
			List<HierarchyOrSearchObject> result = new List<HierarchyOrSearchObject>();
            if (string.IsNullOrEmpty(word)) {
                return result;
			}

			var gameobjects = GameObjectUtility.FindAllInHierarchy();
			var words = word.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
			if (words.Length <= 0) {
				return result;
			}

			foreach (var gameobject in gameobjects) {
				bool is_contain = words.Any(
					(word_unit) => {
						var is_in_name = gameobject.name.IndexOf(word_unit, StringComparison.OrdinalIgnoreCase) >= 0;
						var is_in_component = gameobject.GetComponents<Component>()
							.Where( c => c != null )
							.Any( c => c.GetType().Name.IndexOf(word_unit, StringComparison.OrdinalIgnoreCase) >= 0 );
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
			string color = candidate.target.activeSelf ? "#ffffffff" : "#ffffff88";
			string text	= string.Format("<color={0}>{1}</color>", color, candidate.name);
            GUILayout.Label(text, textGuiStyle);
		}

		public override void Select(string word, IEnumerable<HierarchyOrSearchObject> result_list)
		{
			if (string.IsNullOrEmpty(word)) {
				EditorApplication.delayCall += () => {
					HierarchyAndSearchWindow.ShowWindow();
				};
			}

			_SelectObject(result_list);
			_SaveHistory("HierarchyOrSearch", word);
		}

		private void _ShowRenameWindow(IEnumerable<HierarchyOrSearchObject> result_list, string word)
		{
			IEnumerable<GameObject> list = result_list.Select(item => item.target);
			EditorApplication.delayCall += () => {
				RenameWindow.ShowWindow(list, word);
			};
		}

		private void _SelectObject(IEnumerable<HierarchyOrSearchObject> result_list)
		{
			Selection.objects = result_list.Select((t) => {return t.target;}).Cast<GameObject>().ToArray();
			EditorApplication.ExecuteMenuItem("Window/Hierarchy");
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
